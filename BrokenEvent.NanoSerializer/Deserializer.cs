﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace BrokenEvent.NanoSerializer
{
  /// <summary>
  /// Core class for NanoSerializer deserialization.
  /// </summary>
  public sealed class Deserializer: SerializationBase
  {
    /// <summary>
    /// The delegate using in type resolve process.
    /// </summary>
    /// <param name="name">Assembly or namespace qualified typename.</param>
    /// <returns>The type for given name or null if not found. Null return will fail whole deserialization process</returns>
    public delegate Type TypeResolverDelegate(string name);

    /// <summary>
    /// Deserializes object with default settings.
    /// </summary>
    /// <typeparam name="T">Type of the class to deserialize root. Use <see cref="object"/> if class is unknown.</typeparam>
    /// <param name="data">Data source to deserialize from</param>
    /// <returns>The object or root of object model deserialized from <paramref name="data"/>.</returns>
    /// <exception cref="SerializationException">thrown when deserialization fails</exception>
    public static T Deserialize<T>(IDataAdapter data)
    {
      return new Deserializer().DeserializeObject<T>(data);
    }

    /// <summary>
    /// Deserialize object from source data.
    /// </summary>
    /// <typeparam name="T">Type of the class to deserialize root. Use <see cref="object"/> if class is unknown.</typeparam>
    /// <param name="data">Data source to deserialize from</param>
    /// <returns>The object or root of object model deserialized from <paramref name="data"/>.</returns>
    /// <exception cref="SerializationException">thrown when deserialization fails</exception>
    public T DeserializeObject<T>(IDataAdapter data)
    {
      string flagsStr = data.GetAttribute(ATTRIBUTE_FLAGS, true);
      if (flagsStr != null)
        flags = (OptimizationFlags)int.Parse(flagsStr);
      return (T)DeserializeObject(typeof(T), data, null);
    }

    private Dictionary<int, object> objectCache = new Dictionary<int, object>();
    private Dictionary<string, object> constructorArgs = new Dictionary<string, object>();
    private int maxObjId = 0;
    private OptimizationFlags flags;
    private TypeResolverDelegate typeResolverDelegate = DefaultTypeResolver;

    /// <summary>
    /// Gets the dictinary of default constructor args. This values are used when <see cref="NanoArgAttribute"/> is set to constructor arg.
    /// </summary>
    public IDictionary<string, object> ConstructorArgs
    {
      get { return constructorArgs; }
    }

    /// <summary>
    /// Gets or sets the type resolver for this deserializer instance.
    /// </summary>
    public TypeResolverDelegate TypeResolver
    {
      get { return typeResolverDelegate; }
      set { typeResolverDelegate = value; }
    }

    private class CacheObjectLink
    {
      public CacheObjectLink(int objId)
      {
        ObjId = objId;
      }

      public int ObjId { get; private set; }
    }

    private static Type DefaultTypeResolver(string name)
    {
      return Type.GetType(name);
    }

    private object ResolveObjectLink(object obj)
    {
      CacheObjectLink link = obj as CacheObjectLink;
      return link == null ? obj : objectCache[link.ObjId];
    }

    private float TestConstructor(ConstructorInfo info, Dictionary<int, Tuple<object, Type>> args)
    {
      float result = 0;
      ParameterInfo[] parameterInfos = info.GetParameters();

      // using hint
      NanoConstructorAttribute attr = info.GetCustomAttribute<NanoConstructorAttribute>();
      if (attr != null)
        result += 10;

      // can't be less than minimal count
      int minParams = args == null ? 0 : args.Count;
      if (parameterInfos.Length < minParams)
        return 0;

      float okDelta = 1f / parameterInfos.Length;

      for (int i = 0; i < parameterInfos.Length; i++)
      {
        Tuple<object, Type> arg;
        if (args == null || !args.TryGetValue(i, out arg))
        {
          // unable to set null to primitive type arg
          if (parameterInfos[i].ParameterType.IsPrimitive)
            result -= 1;
          continue;
        }

        // arg from global list, if possible
        NanoArgAttribute argAttr = parameterInfos[i].GetCustomAttribute<NanoArgAttribute>();
        if (argAttr != null && constructorArgs.ContainsKey(argAttr.ArgName))
        {
          result += okDelta;
          continue;
        }

        if (CompareTypesSafe(parameterInfos[i].ParameterType, arg.Value2))
          result += okDelta;
      }
      
      return result;
    }

    private object CreateObject(Type type, Dictionary<int, Tuple<object, Type>> constructorArgs)
    {
      ConstructorInfo ctor = null;
      float ctorWeight = -999;

      // find best constructor
      foreach (ConstructorInfo info in type.GetConstructors())
      {
        float weight = TestConstructor(info, constructorArgs);
        if (weight > ctorWeight)
        {
          ctor = info;
          ctorWeight = weight;
        }
      }

      if (ctor == null)
        throw new SerializationException($"Unable to find constructor for {type.Name}");

      // populate constructor args
      ParameterInfo[] parameterInfos = ctor.GetParameters();
      object[] args = new object[parameterInfos.Length];
      for (int i = 0; i < parameterInfos.Length; i++)
      {
        object arg;
        NanoArgAttribute argAttr = parameterInfos[i].GetCustomAttribute<NanoArgAttribute>();
        if (argAttr != null && this.constructorArgs.TryGetValue(argAttr.ArgName, out arg))
        {
          args[i] = arg;
          continue;
        }

        Tuple<object, Type> propertyArg;
        if (constructorArgs.TryGetValue(i, out propertyArg))
        {
          args[i] = propertyArg.Value1;
          continue;
        }
      }

      // create object
      return ctor.Invoke(args);
    }

    private object DeserializeObject(Type type, IDataAdapter data, object target)
    {
      if (target != null)
      {
        if (!DeserializeContainer(type, data, ref target))
          FillObject(target.GetType(), target, data);
        return target;
      }

      int objId = -1;

      if ((flags & OptimizationFlags.NoReferences) == 0)
      {
        string objIdStr = data.GetAttribute(ATTRIBUTE_OBJID, true);
        if (objIdStr != null)
        {
          objId = int.Parse(objIdStr);
          object obj;
          return objectCache.TryGetValue(objId, out obj) ? obj : new CacheObjectLink(objId);
        }
      }

      string typeName = data.GetAttribute(ATTRIBUTE_TYPE, true);
      if (typeName != null)
      {
        type = typeResolverDelegate(typeName);
        if (type == null)
          throw new SerializationException($"Unable to find type {typeName}");
      }

      if (IsPrimitive(type))
        return DeserializePrimitive(type, data.Value);

      if (DeserializeContainer(type, data, ref target))
      {
        if ((flags & OptimizationFlags.NoReferences) == 0)
          objectCache.Add(maxObjId++, target);
        return target;
      }

      Dictionary<int, Tuple<object, Type>> constructorArgs = null;

      // reserve space in cache for this object
      if ((flags & OptimizationFlags.NoReferences) == 0)
      {
        objId = maxObjId++;
        objectCache.Add(objId, new CacheObjectLink(objId));
      }

      // read properties required for object creation
      foreach (PropertyInfo info in type.GetProperties())
      {
        NanoSerializationAttribute attr = info.GetCustomAttribute<NanoSerializationAttribute>();
        if (attr == null || attr.ConstructorArg == -1)
          continue;

        bool isPrimitive = IsPrimitive(info.PropertyType);
        NanoLocation location = attr.Location;

        if (location == NanoLocation.Auto)
          location = isPrimitive ? NanoLocation.Attribute : NanoLocation.SubNode;

        object value = null;
        if (isPrimitive)
        {
          string stringValue;
          if (location == NanoLocation.Attribute)
            stringValue = data.GetAttribute(info.Name, false);
          else
          {
            IDataAdapter e = data.GetChild(info.Name);
            stringValue = e?.Value;
          }

          if (stringValue != null)
            value = DeserializePrimitive(info.PropertyType, stringValue);
        }
        else
        {
          IDataAdapter subnode = data.GetChild(info.Name);
          if (subnode != null)
            value = DeserializeObject(info.PropertyType, subnode, null);
        }

        if (constructorArgs == null)
          constructorArgs = new Dictionary<int, Tuple<object, Type>>();
        constructorArgs.Add(attr.ConstructorArg, new Tuple<object, Type>(value, info.PropertyType));
      }

      // create object
      target = CreateObject(type, constructorArgs);
      if ((flags & OptimizationFlags.NoReferences) == 0)
        objectCache[objId] = target;

      FillObject(type, target, data);

      return target;
    }

    private void FillObject(Type type, object target, IDataAdapter data)
    {
      // read common properties
      foreach (PropertyInfo info in type.GetProperties())
      {
        // can't be read, so can't be serialized
        if (!info.CanRead)
          continue;

        NanoState state = NanoState.Serialize;

        NanoSerializationAttribute attr = info.GetCustomAttribute<NanoSerializationAttribute>();

        if (attr != null)
        {
          if (attr.ConstructorArg != -1 && state != NanoState.SerializeSet)
            continue;

          state = attr.State;
        }

        if (state == NanoState.Ignore)
          continue;

        bool isPrimitive = IsPrimitive(info.PropertyType);
        NanoLocation location = NanoLocation.Auto;
        if (attr != null)
          location = attr.Location;

        if (location == NanoLocation.Auto)
          location = isPrimitive ? NanoLocation.Attribute : NanoLocation.SubNode;

        object value = null;
        if (isPrimitive)
        {
          if (!info.CanWrite)
            continue;

          string stringValue;
          if (location == NanoLocation.Attribute)
            stringValue = data.GetAttribute(info.Name, false);
          else
          {
            IDataAdapter e = data.GetChild(info.Name);
            stringValue = e?.Value;
          }

          if (stringValue != null)
            value = DeserializePrimitive(info.PropertyType, stringValue);

          info.SetValue(target, value);
        }
        else
        {
          IDataAdapter subnode = data.GetChild(info.Name);
          if (subnode != null)
          {
            object currentValue = info.GetValue(target);
            if (currentValue == null)
              info.SetValue(target, DeserializeObject(info.PropertyType, subnode, null));
            else
              DeserializeObject(info.PropertyType, subnode, currentValue);
          }
        }
      }

      // read fields
      foreach (FieldInfo info in type.GetFields())
      {
        NanoState state = NanoState.Serialize;

        NanoSerializationAttribute attr = info.GetCustomAttribute<NanoSerializationAttribute>();
        if (attr != null)
          state = attr.State;

        if (state == NanoState.Ignore || info.IsNotSerialized)
          continue;

        bool isPrimitive = IsPrimitive(info.FieldType);
        NanoLocation location = NanoLocation.Auto;
        if (attr != null)
          location = attr.Location;

        if (location == NanoLocation.Auto)
          location = isPrimitive ? NanoLocation.Attribute : NanoLocation.SubNode;

        object value = null;
        if (isPrimitive)
        {
          string stringValue;
          if (location == NanoLocation.Attribute)
            stringValue = data.GetAttribute(info.Name, false);
          else
          {
            IDataAdapter e = data.GetChild(info.Name);
            stringValue = e?.Value;
          }

          if (stringValue != null)
            value = DeserializePrimitive(info.FieldType, stringValue);
        }
        else
        {
          IDataAdapter subnode = data.GetChild(info.Name);
          if (subnode != null)
            value = DeserializeObject(info.FieldType, subnode, null);
        }

        info.SetValue(target, value);
      }
    }

    private object DeserializeContainer(ref object container, Type type, Type elementType, IDataAdapter data, string addMethodName, bool reverse = false)
    {
      if (container == null)
        container = Activator.CreateInstance(type);
      MethodInfo addMethod = type.GetMethod(addMethodName, new []{elementType});
      object[] argsCache = new object[1];

      if (reverse)
        foreach (IDataAdapter element in data.GetChildrenReversed())
        {
          argsCache[0] = DeserializeObject(elementType, element, null);
          addMethod.Invoke(container, argsCache);
        }
      else
        foreach (IDataAdapter element in data.GetChildren())
        {
          argsCache[0] = DeserializeObject(elementType, element, null);
          addMethod.Invoke(container, argsCache);
        }

      return container;
    }

    private void DeserializeArrayRank(Array array, Type elementType, int[] coords, int r, IDataAdapter data)
    {
      if (r == coords.Length - 1)
      {
        int index = 0;
        foreach (IDataAdapter element in data.GetChildren())
        {
          coords[r] = index++;
          array.SetValue(DeserializeObject(elementType, element, null), coords);
        }
      }
      else
      {
        int index = 0;
        foreach (IDataAdapter element in data.GetChildren())
        {
          coords[r] = index++;
          DeserializeArrayRank(array, elementType, coords, r + 1, element);
        }
      }
    }

    private static void ScanArrayRanks(IDataAdapter data, int[] lengths, int index)
    {
      int count = 0;
      IDataAdapter firstChild = null;
      foreach (IDataAdapter e in data.GetChildren())
      {
        if (firstChild == null)
          firstChild = e;

        count++;
      }

      lengths[index] = count;
      if (index < lengths.Length - 1)
        ScanArrayRanks(firstChild, lengths, index + 1);
    }

    private bool DeserializeContainer(Type type, IDataAdapter data, ref object target)
    {
      if ((flags & OptimizationFlags.NoContainers) != 0)
        return false;

      if (target != null)
        type = target.GetType();

      if (type.IsArray)
      {
        Type elementType = type.GetElementType();
        int[] lengths = new int[int.Parse(data.GetAttribute(ATTRIBUTE_ARRAY_RANK, true))];
        ScanArrayRanks(data, lengths, 0);

        Array array;
        if (target == null)
          target = array = Array.CreateInstance(elementType, lengths);
        else
          array = (Array)target;

        int[] coords = new int[array.Rank];
        DeserializeArrayRank(array, elementType, coords, 0, data);

        return true;
      }

      if (type.IsGenericType)
      {
        if (HaveInterface(type, typeof(IList<>)))
        {
          DeserializeContainer(ref target, type, type.GetGenericArguments()[0], data, "Add");
          return true;
        }

        if (type.GetGenericTypeDefinition() == typeof(Queue<>))
        {
          DeserializeContainer(ref target, type, type.GetGenericArguments()[0], data, "Enqueue");
          return true;
        }

        if (type.GetGenericTypeDefinition() == typeof(Stack<>))
        {
          DeserializeContainer(ref target, type, type.GetGenericArguments()[0], data, "Push", true);
          return true;
        }

        if (HaveInterface(type.GetGenericTypeDefinition(), typeof(ISet<>)))
        {
          DeserializeContainer(ref target, type, type.GetGenericArguments()[0], data, "Add");
          return true;
        }

        if (type.GetGenericTypeDefinition() == typeof(LinkedList<>))
        {
          DeserializeContainer(ref target, type, type.GetGenericArguments()[0], data, "AddLast");
          return true;
        }

        if (HaveInterface(type.GetGenericTypeDefinition(), typeof(IDictionary<,>)))
        {
          if (target == null)
            target = Activator.CreateInstance(type);

          Type keyType = type.GetGenericArguments()[0];
          Type valueType = type.GetGenericArguments()[1];
          MethodInfo addMethod = type.GetMethod("Add");

          object[] argsCache = new object[2];
          foreach (IDataAdapter element in data.GetChildren())
          {
            argsCache[0] = DeserializeObject(keyType, element.GetChild("Key"), null);
            argsCache[1] = DeserializeObject(valueType, element.GetChild("Value"), null);
            addMethod.Invoke(target, argsCache);
          }

          return true;
        }
      }
      else
      {
        if (HaveInterface(type, typeof(IList)))
        {
          DeserializeContainer(ref target, type, typeof(object), data, "Add");
          return true;
        }

        if (type == typeof(Queue))
        {
          DeserializeContainer(ref target, type, typeof(object), data, "Enqueue");
          return true;
        }

        if (type == typeof(Stack))
        {
          DeserializeContainer(ref target, type, typeof(object), data, "Push", true);
          return true;
        }
      }

      return false;
    }
  }
}

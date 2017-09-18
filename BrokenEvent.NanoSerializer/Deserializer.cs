using System;
using System.Collections.Generic;
using System.Reflection;

using BrokenEvent.NanoSerializer.Caching;

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
    private Dictionary<string, Type> typeResolverCache = new Dictionary<string, Type>();

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

    private Type InternalResolveTypes(string typeName)
    {
      Type result;
      if (typeResolverCache.TryGetValue(typeName, out result))
        return result;

      result = typeResolverDelegate(typeName);
      if (result != null)
        typeResolverCache.Add(typeName, result);

      return result;
    }

    private static Type DefaultTypeResolver(string name)
    {
      return Type.GetType(name);
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
      // existing object
      TypeCategory category;

      if (target != null)
      {
        type = target.GetType();
        category = GetTypeCategory(type);
        if (category == TypeCategory.Unknown)
          FillObject(TypeCache.GetWrapper(type), target, data);
        else
          DeserializeContainer(type, category, data, ref target);
          
        return target;
      }

      int objId = -1;

      // resolve reference, if any
      if ((flags & OptimizationFlags.NoReferences) == 0)
      {
        string objIdStr = data.GetAttribute(ATTRIBUTE_OBJID, true);
        if (objIdStr != null)
        {
          objId = int.Parse(objIdStr);
          return objectCache[objId];
        }
      }

      // fix type, if needed
      string typeName = data.GetAttribute(ATTRIBUTE_TYPE, true);
      if (typeName != null)
      {
        type = InternalResolveTypes(typeName);
        if (type == null)
          throw new SerializationException($"Unable to find type {typeName}");
      }

      // primitive type?
      if (IsPrimitive(type))
        return DeserializePrimitive(type, data.Value);

      // load container
      category = GetTypeCategory(type);
      if (category != TypeCategory.Unknown)
      {
        DeserializeContainer(type, category, data, ref target);

        if ((flags & OptimizationFlags.NoReferences) == 0)
          objectCache.Add(maxObjId++, target);
        return target;
      }

      // this is unknown object
      TypeWrapper wrapper = TypeCache.GetWrapper(type);

      Dictionary<int, Tuple<object, Type>> constructorArgs = null;
      for (int i = 0; i < wrapper.Properties.Count; i++)
      {
        PropertyWrapper property = wrapper.Properties[i];
        if (property.ConstructorArg == -1)
          continue;

        object value = null;
        if (property.TypeCategory == TypeCategory.Primitive)
        {
          string stringValue = ReadString(data, property.Location, property.Info.Name);
          if (stringValue != null)
            value = DeserializePrimitive(property.MemberType, stringValue);
        }
        else
        {
          IDataAdapter subnode = data.GetChild(property.Info.Name);
          if (subnode != null)
            value = DeserializeObject(property.MemberType, subnode, null);
        }

        // TODO can we avoid the temporary dictionary?
        if (constructorArgs == null)
          constructorArgs = new Dictionary<int, Tuple<object, Type>>();
        constructorArgs.Add(property.ConstructorArg, new Tuple<object, Type>(value, property.MemberType));
      }

      // create object
      target = CreateObject(type, constructorArgs);
      if ((flags & OptimizationFlags.NoReferences) == 0)
        objectCache[maxObjId++] = target;

      FillObject(wrapper, target, data);

      return target;
    }

    private void FillObject(TypeWrapper wrapper, object target, IDataAdapter data)
    {
      // read properties
      for (int i = 0; i < wrapper.Properties.Count; i++)
      {
        PropertyWrapper property = wrapper.Properties[i];

        // should be already loaded at this time
        if (property.ConstructorArg != -1)
          continue;

        if (property.TypeCategory == TypeCategory.Primitive)
        {
          if (!property.Info.CanWrite)
            continue;

          string stringValue = ReadString(data, property.Location, property.Info.Name);
          if (stringValue != null)
            property.SetValue(target, DeserializePrimitive(property.MemberType, stringValue));
        }
        else
        {
          IDataAdapter subnode = data.GetChild(property.Info.Name);
          if (subnode != null)
          {
            object currentValue = property.GetValue(target);
            if (currentValue == null)
              property.SetValue(target, DeserializeObject(property.MemberType, subnode, null));
            else
              DeserializeObject(property.MemberType, subnode, currentValue);
          }
        }
      }

      // read fields
      for (int i = 0; i < wrapper.Fields.Count; i++)
      {
        FieldWrapper field = wrapper.Fields[i];

        if (field.TypeCategory == TypeCategory.Primitive)
        {
          string stringValue = ReadString(data, field.Location, field.Info.Name);
          if (stringValue != null)
            field.SetValue(target, DeserializePrimitive(field.Info.FieldType, stringValue));
        }
        else
        {
          IDataAdapter subnode = data.GetChild(field.Info.Name);
          if (subnode != null)
          {
            object value = DeserializeObject(field.MemberType, subnode, null);
            if (value != null)
              field.SetValue(target, value);
          }
        }
      }
    }

    private static string ReadString(IDataAdapter data, NanoLocation location, string name)
    {
      if (location == NanoLocation.Attribute)
        return data.GetAttribute(name, false);

      IDataAdapter e = data.GetChild(name);
      return e?.Value;
    }

    private void DeserializeContainer(ref object container, Type type, Type elementType, IDataAdapter data, string addMethodName, bool reverse = false)
    {
      if (container == null)
        container = TypeCache.CreateParameterless(type);

      Action<object, object> addAction = null;
      if (!TypeCache.TryGetTypeAccessor(type, ref addAction))
      {
        addAction = InvocationHelper.CreateSetDelegate(type, elementType, addMethodName);
        TypeCache.AddTypeAccessor(type, addAction);
      }

      if (reverse)
        foreach (IDataAdapter element in data.GetChildrenReversed())
          addAction(container, DeserializeObject(elementType, element, null));
      else
        foreach (IDataAdapter element in data.GetChildren())
          addAction(container, DeserializeObject(elementType, element, null));
    }

    private void DeserializeContainer(ref object container, Type type, Type elementType, Type returnType, IDataAdapter data, string addMethodName)
    {
      if (container == null)
        container = TypeCache.CreateParameterless(type);

      Func<object, object, object> addFunc = null;
      if (!TypeCache.TryGetTypeAccessor(type, ref addFunc))
      {
        addFunc = InvocationHelper.CreateGetSetDelegate(type, elementType, returnType, addMethodName);
        TypeCache.AddTypeAccessor(type, addFunc);
      }

      foreach (IDataAdapter element in data.GetChildren())
        addFunc(container, DeserializeObject(elementType, element, null));
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

    private void DeserializeContainer(Type type, TypeCategory category, IDataAdapter data, ref object target)
    {
      // arrays
      if (category == TypeCategory.Array)
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

        return;
      }

      // non generic containers
      switch (category)
      {
        case TypeCategory.IList:
          DeserializeContainer(ref target, type, typeof(object), data, "Add");
          return;

        case TypeCategory.Queue:
          DeserializeContainer(ref target, type, typeof(object), data, "Enqueue");
          return;

        case TypeCategory.Stack:
          DeserializeContainer(ref target, type, typeof(object), data, "Push", true);
          return;
      }

      // generics
      Type[] genericArgs = TypeCache.GetTypeGenericArgs(type);

      switch (category)
      {
        case TypeCategory.GenericIList:
          DeserializeContainer(ref target, type, genericArgs[0], data, "Add");
          return;

        case TypeCategory.GenericQueue:
          DeserializeContainer(ref target, type, genericArgs[0], data, "Enqueue");
          return;

        case TypeCategory.GenericStack:
          DeserializeContainer(ref target, type, genericArgs[0], data, "Push", true);
          return;

        case TypeCategory.ISet:
          DeserializeContainer(ref target, type, genericArgs[0], typeof(bool), data, "Add");
          return;

        case TypeCategory.LinkedList:
          DeserializeContainer(ref target, type, genericArgs[0], typeof(LinkedListNode<>).MakeGenericType(genericArgs[0]), data, "AddLast");
          return;
      }

      if (category == TypeCategory.IDictionary)
      {
        if (target == null)
          target = TypeCache.CreateParameterless(type);

        Action<object, object, object> setAction = null;
        if (!TypeCache.TryGetTypeAccessor(type, ref setAction))
        {
          setAction = InvocationHelper.CreateSetDelegate(type, genericArgs[0], genericArgs[1], "Add");
          TypeCache.AddTypeAccessor(type, setAction);
        }

        foreach (IDataAdapter element in data.GetChildren())
          setAction(
              target,
              DeserializeObject(genericArgs[0], element.GetChild("Key"), null),
              DeserializeObject(genericArgs[1], element.GetChild("Value"), null)
            );
      }
    }
  }
}

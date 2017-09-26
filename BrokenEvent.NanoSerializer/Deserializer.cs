using System;
using System.Collections.Generic;

using BrokenEvent.NanoSerializer.Adapter;
using BrokenEvent.NanoSerializer.Caching;

namespace BrokenEvent.NanoSerializer
{
  /// <summary>
  /// Core class for NanoSerializer deserialization.
  /// </summary>
  public sealed class Deserializer: SerializationBase, ISubDeserializer
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
      maxObjId = 1;
      objectCache.Clear();
      flags = (OptimizationFlags)data.GetIntValue(ATTRIBUTE_FLAGS, true);

      return (T)DeserializeObject(typeof(T), data, null);
    }

    /// <summary>
    /// Fills existing object with deserialized data.
    /// </summary>
    /// <typeparam name="T">Target type of the object</typeparam>
    /// <param name="target">Target object to fill</param>
    /// <param name="data">Data source to deserialize from</param>
    public void FillObject<T>(T target, IDataAdapter data)
    {
      maxObjId = 1;
      objectCache.Clear();
      flags = (OptimizationFlags)data.GetIntValue(ATTRIBUTE_FLAGS, true); 

      TypeWrapper wrapper = TypeCache.GetWrapper(typeof(T));
      FillObject(wrapper, target, data);
    }

    private Dictionary<int, object> objectCache = new Dictionary<int, object>();
    private Dictionary<string, object> constructorArgs = new Dictionary<string, object>();
    private int maxObjId = 1;
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

    private object CreateObject(TypeWrapper wrapper, object[] args)
    {
      if (wrapper.ConstructorArgNames != null)
      {
        if (args == null)
          args = new object[wrapper.ConstructorArgsCount];

        for (int i = 0; i < args.Length; i++)
        {
          object value;
          string argName = wrapper.ConstructorArgNames[i];
          if (argName != null &&
              constructorArgs.TryGetValue(argName, out value))
            args[i] = value;
        }
      }

      // create object
      return wrapper.CreateObject(args);
    }

    private object DeserializeObject(Type type, IDataAdapter data, object target)
    {
      // existing object
      TypeCategory category;

      if (target != null)
      {
        type = target.GetType();
        if ((flags & OptimizationFlags.NoContainers) == 0)
        {
          category = GetTypeCategory(type);
          if (category != TypeCategory.Unknown)
          {
            DeserializeContainer(type, category, data, ref target);
            return target;
          }
        }
        
        FillObject(TypeCache.GetWrapper(type), target, data);
        return target;
      }

      // resolve reference, if any
      if (type.IsClass && (flags & OptimizationFlags.NoReferences) == 0)
      {
        int id = (int)data.GetIntValue(ATTRIBUTE_OBJID, true);
        if (id > 0)
          return objectCache[id];
      }

      // fix type, if needed
      string typeName = data.GetStringValue(ATTRIBUTE_TYPE, true);
      if (typeName != null)
      {
        type = InternalResolveTypes(typeName);
        if (type == null)
          throw new SerializationException($"Unable to find type {typeName}");
      }

      // primitive type?
      if (IsPrimitive(type))
        return DeserializePrimitive(type, data, (flags & OptimizationFlags.EnumAsValue) != 0);

      if ((flags & OptimizationFlags.NoContainers) == 0)
      {
        // load container
        category = GetTypeCategory(type);
        if (category != TypeCategory.Unknown)
        {
          DeserializeContainer(type, category, data, ref target);

          if ((flags & OptimizationFlags.NoReferences) == 0)
            objectCache.Add(maxObjId++, target);
          return target;
        }
      }

      // this is unknown object
      TypeWrapper wrapper = TypeCache.GetWrapper(type);

      if (wrapper.Serializer != null)
        return wrapper.Serializer.DeserializeObject(data, this);

      object[] constructorArgs = null;

      for (int i = 0; i < wrapper.Properties.Count; i++)
      {
        PropertyWrapper property = wrapper.Properties[i];
        if (property.ConstructorArg == -1)
          continue;

        object value = null;
        if (property.TypeCategory == TypeCategory.Primitive ||
            property.TypeCategory == TypeCategory.Enum)
        {
          value = DeserializePrimitive(
              property.MemberType,
              data,
              property.Name,
              property.Location != NanoLocation.SubNode,
              (flags & OptimizationFlags.EnumAsValue) != 0
            );
        }
        else
        {
          IDataAdapter subnode = data.GetChild(property.Name);
          if (subnode != null)
            value = DeserializeObject(property.MemberType, subnode, null);
        }

        if (constructorArgs == null)
          constructorArgs = new object[wrapper.ConstructorArgsCount];
        constructorArgs[property.ConstructorArg] = value;
      }

      for (int i = 0; i < wrapper.Fields.Count; i++)
      {
        FieldWrapper field = wrapper.Fields[i];
        if (field.ConstructorArg == -1)
          continue;

        object value = null;
        if (field.TypeCategory == TypeCategory.Primitive ||
            field.TypeCategory == TypeCategory.Enum)
        {
          value = DeserializePrimitive(
              field.MemberType,
              data,
              field.Name,
              field.Location != NanoLocation.SubNode,
              (flags & OptimizationFlags.EnumAsValue) != 0
            );
        }
        else
        {
          IDataAdapter subnode = data.GetChild(field.Name);
          if (subnode != null)
            value = DeserializeObject(field.MemberType, subnode, null);
        }

        if (constructorArgs == null)
          constructorArgs = new object[wrapper.ConstructorArgsCount];
        constructorArgs[field.ConstructorArg] = value;
      }

      // create object
      target = CreateObject(wrapper, constructorArgs);
      if ((flags & OptimizationFlags.NoReferences) == 0)
        objectCache[maxObjId++] = target;

      FillObject(wrapper, target, data);

      return target;
    }

    private void FillObject(TypeWrapper wrapper, object target, IDataAdapter data)
    {
      if (wrapper.IsSelfSerializable)
      {
        ((INanoSerializable)target).Deserialize(data, this);
        return;
      }

      // read properties
      for (int i = 0; i < wrapper.Properties.Count; i++)
      {
        PropertyWrapper property = wrapper.Properties[i];

        // should be already loaded at this time
        if (property.ConstructorArg != -1 && property.State != NanoState.SerializeSet)
          continue;

        if (property.Info.GetMethod.IsPrivate && property.Info.SetMethod.IsPrivate &&
            (flags & OptimizationFlags.PrivateProperties) == 0)
          continue;

        if (property.TypeCategory == TypeCategory.Primitive ||
            property.TypeCategory == TypeCategory.Enum)
        {
          if (!property.Info.CanWrite)
            continue;

          property.SetValue(
              target,
              DeserializePrimitive(
                  property.MemberType,
                  data,
                  property.Name,
                  property.Location != NanoLocation.SubNode,
                  (flags & OptimizationFlags.EnumAsValue) != 0
                )
            );
        }
        else
        {
          IDataAdapter subnode = data.GetChild(property.Name);
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

        // should be already loaded at this time
        if (field.ConstructorArg != -1 && field.State != NanoState.SerializeSet)
          continue;

        if (field.TypeCategory == TypeCategory.Primitive ||
            field.TypeCategory == TypeCategory.Enum)
        {
          field.SetValue(
              target,
              DeserializePrimitive(
                  field.MemberType,
                  data,
                  field.Name,
                  field.Location != NanoLocation.SubNode,
                  (flags & OptimizationFlags.EnumAsValue) != 0
                )
            );
        }
        else
        {
          IDataAdapter subnode = data.GetChild(field.Name);
          if (subnode != null)
          {
            object value = DeserializeObject(field.MemberType, subnode, null);
            if (value != null)
              field.SetValue(target, value);
          }
        }
      }
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

      if (elementType.IsPrimitive)
      {
        byte[] buffer = Convert.FromBase64String(data.GetStringValue());
        int size = ByteUtils.GetSizeOf(elementType);
        int count = buffer.Length / size;
        Func<byte[], int, object> reader = ByteUtils.GetBinaryReader(elementType);

        if (reverse)
          for (int i = count - 1; i >= 0; i--)
            addAction(container, reader(buffer, size * i));
        else
          for (int i = 0; i < count; i++)
            addAction(container, reader(buffer, size * i));

        return;
      }

      IDataArray array = data.GetArray();
      if (reverse)
        foreach (IDataAdapter element in array.GetChildrenReversed())
          addAction(container, DeserializeObject(elementType, element, null));
      else
        foreach (IDataAdapter element in array.GetChildren())
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

      foreach (IDataAdapter element in data.GetArray().GetChildren())
        addFunc(container, DeserializeObject(elementType, element, null));
    }

    private void DeserializeArrayRank(Array array, Type elementType, int[] coords, int r, IDataAdapter data)
    {
      if (r == coords.Length - 1)
      {
        if (elementType.IsPrimitive)
        {
          byte[] buffer = Convert.FromBase64String(data.GetStringValue());
          int size = ByteUtils.GetSizeOf(elementType);
          int count = array.GetLength(r);
          Func<byte[], int, object> reader = ByteUtils.GetBinaryReader(elementType);

          for (int i = 0; i < count; i++)
          {
            coords[r] = i;
            array.SetValue(reader(buffer, i * size), coords);
          }
        }
        else
        {
          int index = 0;
          foreach (IDataAdapter element in data.GetArray().GetChildren())
          {
            coords[r] = index++;
            array.SetValue(DeserializeObject(elementType, element, null), coords);
          }
        }
      }
      else
      {
        int index = 0;
        foreach (IDataAdapter element in data.GetArray().GetChildren())
        {
          coords[r] = index++;
          DeserializeArrayRank(array, elementType, coords, r + 1, element);
        }
      }
    }

    private static void ScanArrayRanks(IDataAdapter data, Type elementType, int[] lengths, int index)
    {
      if (index == lengths.Length - 1 && elementType.IsPrimitive)
      {
        string value = data.GetStringValue();
        lengths[index] = ByteUtils.GetBytesInBase64(value) / ByteUtils.GetSizeOf(elementType);
        return;
      }

      int count = 0;
      IDataAdapter firstChild = null;
      foreach (IDataAdapter e in data.GetArray().GetChildren())
      {
        if (firstChild == null)
          firstChild = e;

        count++;
      }

      lengths[index] = count;
      if (index < lengths.Length - 1)
        ScanArrayRanks(firstChild, elementType, lengths, index + 1);
    }

    private void DeserializeContainer(Type type, TypeCategory category, IDataAdapter data, ref object target)
    {
      // arrays
      if (category == TypeCategory.Array)
      {
        Type elementType = type.GetElementType();
        int rank = (int)data.GetIntValue(ATTRIBUTE_ARRAY_RANK, true);
        if (rank == 0)
          rank = 1;

        int[] lengths = new int[rank];
        ScanArrayRanks(data, elementType, lengths, 0);

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

        foreach (IDataAdapter element in data.GetArray().GetChildren())
          setAction(
              target,
              DeserializeObject(genericArgs[0], element.GetChild("Key"), null),
              DeserializeObject(genericArgs[1], element.GetChild("Value"), null)
            );
      }
    }

    #region ISubDeserializer

    void ISubDeserializer.ContinueDeserialization(Type targetType, IDataAdapter data, ref object value)
    {
      value = DeserializeObject(targetType, data, value);
    }

    #endregion
  }
}

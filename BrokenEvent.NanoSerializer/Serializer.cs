using System;
using System.Collections;
using System.Collections.Generic;

using BrokenEvent.NanoSerializer.Caching;

namespace BrokenEvent.NanoSerializer
{
  /// <summary>
  /// Core class for NanoSerializer serialization.
  /// </summary>
  public sealed class Serializer: SerializationBase, ISubSerializer
  {
    private struct Info
    {
      public readonly string Name;
      public readonly NanoLocation Location;
      public readonly TypeCategory Category;
      public readonly Type[] GenericArgs;

      public Info(MemberWrapper member):
        this(member.MemberInfo.Name, member.Location, member.TypeCategory, member.GenericArguments) { }

      private Info(string name, NanoLocation location, TypeCategory category, Type[] genericArgs)
      {
        Name = name;
        Location = location;
        Category = category;
        GenericArgs = genericArgs;
      }

      public Info(string name, NanoLocation location, Type type):
        this(name, location, GetTypeCategory(type), type.IsGenericType ? type.GetGenericArguments() : null) { }
    }

    /// <summary>
    /// Creates instance of the serializer with given settings object.
    /// </summary>
    /// <param name="settings">The serializer settings</param>
    /// <exception cref="ArgumentNullException">thrown when the <paramref name="settings"/> is null.</exception>
    public Serializer(SerializationSettings settings)
    {
      if (settings == null)
        throw new ArgumentNullException(nameof(settings));

      this.settings = settings;
    }

    /// <summary>
    /// Creates instance of the serializer with the default settings.
    /// </summary>
    public Serializer(): this(new SerializationSettings())
    {
    }

    private Dictionary<object, int> objectsCache = new Dictionary<object, int>();
    private int maxObjId = 0;
    private bool haveContainers = false;
    private bool haveReferences = false;
    private bool havePrivateProperties = false;
    private SerializationSettings settings;

    /// <summary>
    /// Gets the serialization settings object.
    /// </summary>
    public SerializationSettings Settings
    {
      get { return settings; }
    }

    /// <summary>
    /// Serialize the object with default settings.
    /// </summary>
    /// <param name="data">Data carrier to serialize to</param>
    /// <param name="target">Object or root of object model to be serialized</param>
    public static void Serialize(IDataAdapter data, object target)
    {
      new Serializer().SerializeObject(data, target);
    }

    /// <summary>
    /// Serialize the object.
    /// </summary>
    /// <param name="data">Data carrier to serialize to</param>
    /// <param name="target">Object or root of object model to be serialized</param>
    /// <exception cref="ArgumentNullException">thrown if <paramref name="data"/> is null or <paramref name="target"/> is null</exception>
    public void SerializeObject(IDataAdapter data, object target)
    {
      if (data == null)
        throw new ArgumentNullException(nameof(data));
      if (target == null)
        throw new ArgumentNullException(nameof(target));

      haveContainers = false;
      haveReferences = false;
      havePrivateProperties = false;

      SerializeValue(data, target);

      if (!settings.SaveOptimizationFlags)
        return;

      OptimizationFlags flags = 0;
      if (!haveContainers)
        flags |= OptimizationFlags.NoContainers;
      if (!haveReferences)
        flags |= OptimizationFlags.NoReferences;
      if (havePrivateProperties)
        flags |= OptimizationFlags.PrivateProperties;

      if (flags != 0)
        data.AddSystemAttribute(ATTRIBUTE_FLAGS, ((int)flags).ToString());
    }

    private void SerializeValue(IDataAdapter data, object target)
    {
      objectsCache.Add(target, maxObjId++);
      TypeWrapper wrapper = TypeCache.GetWrapper(target.GetType());

      if (wrapper.IsSelfSerializable)
      {
        ((INanoSerializable)target).Serialize(data, this);
        return;
      }

      for (int i = 0; i < wrapper.Properties.Count; i++)
      {
        PropertyWrapper property = wrapper.Properties[i];

        if ((!property.CanRead || property.Info.GetMethod.IsPrivate) && 
            (!property.CanWrite || property.Info.SetMethod.IsPrivate))
        {
          if (!settings.SerializePrivateProperties)
            continue;

          havePrivateProperties = true;
        }
            
        bool isReadOnly;
        if (property.ConstructorArg != -1 || // will be used in constructor
            settings.SerializeReadOnly)
          isReadOnly = false; // always serialize
        else
          isReadOnly = !property.CanWrite;

        object value = property.GetValue(target);
        if (value != null)
          SerializeValue(property.MemberType, value, data, new Info(property), isReadOnly);
      }

      for (int i = 0; i < wrapper.Fields.Count; i++)
      {
        FieldWrapper field = wrapper.Fields[i];

        if (!settings.IgnoreNotSerialized && field.Info.IsNotSerialized)
          continue;

        object value = field.GetValue(target);
        if (value != null)
          SerializeValue(field.MemberType, value, data, new Info(field), false);
      }
    }

    private void SerializeValue(Type type, object value, IDataAdapter data, Info info, bool isReadOnly)
    {
      if (info.Category == TypeCategory.Primitive)
      {
        // no need to serialize
        if (isReadOnly)
          return;

        if (info.Location == NanoLocation.SubNode)
          data.AddChild(info.Name).Value = value.ToString();
        else
          data.AddAttribute(info.Name, value.ToString());

        return;
      }

      if (info.Location == NanoLocation.Attribute)
        throw new SerializationException($"Unable to serialize {type.Name} to attribute");

      IDataAdapter subNode = data.AddChild(info.Name);

      SerializeSubValue(type, value, subNode, info.GenericArgs);
    }

    private void SerializeSubValue(Type type, object value, IDataAdapter data, Type[] genericArgs)
    {
      int objId;
      if (objectsCache.TryGetValue(value, out objId))
      {
        data.AddSystemAttribute(ATTRIBUTE_OBJID, objId.ToString());
        haveReferences = true;
        return;
      }

      Type targetType = value.GetType();
      if (targetType != type)
      {
        data.AddSystemAttribute(ATTRIBUTE_TYPE, TypeCache.GetTypeFullName(targetType, settings.AssemblyQualifiedNames));
        type = targetType;
        genericArgs = type.GetGenericArguments();
      }

      TypeCategory category = GetTypeCategory(type);

      switch (category)
      {
        case TypeCategory.Primitive:
          data.Value = value.ToString();
          break;
        case TypeCategory.Unknown:
          SerializeValue(data, value);
          break;
        default:
          SerializeContainer(value, data, category, genericArgs);
          break;
      }
    }

    private void SerializeContainer(IEnumerable e, Type elementType, IDataAdapter data)
    {
      Info info = new Info(settings.ContainerItemName, NanoLocation.SubNode, elementType);
      foreach (object o in e)
        SerializeValue(elementType, o, data, info, false);

      haveContainers = true;
    }

    private void SerializeArrayRank(Array array, Type elementType, int[] coords, int r, IDataAdapter data, Info info)
    {
      if (r == coords.Length - 1)
        for (int i = 0; i < array.GetLength(r); i++)
        {
          coords[r] = i;
          SerializeValue(elementType, array.GetValue(coords), data, info, false);
        }
      else
        for (int i = 0; i < array.GetLength(r); i++)
        {
          coords[r] = i;
          SerializeArrayRank(array, elementType, coords, r + 1, data.AddChild(settings.ArrayItemName), info);
        }
    }

    private void SerializeContainer(object value, IDataAdapter data, TypeCategory category, Type[] genericArgs)
    {
      Type type = value.GetType();

      // arrays
      if (category == TypeCategory.Array)
      {
        Array array = (Array)value;
        Type elementType = type.GetElementType();
        Info localInfo = new Info(settings.ArrayItemName, NanoLocation.SubNode, elementType);
        int[] coords = new int[array.Rank];
        SerializeArrayRank(array, elementType, coords, 0, data, localInfo);
        data.AddSystemAttribute(ATTRIBUTE_ARRAY_RANK, array.Rank.ToString());

        haveContainers = true;
        return;
      }

      // dictionaries
      if (category == TypeCategory.IDictionary)
      {
        Type keyType = genericArgs[0];
        Type valueType = genericArgs[1];
        Info keyInfo = new Info(settings.DictionaryKeyName, NanoLocation.SubNode, genericArgs[0]);
        Info valueInfo = new Info(settings.DictionaryValueName, NanoLocation.SubNode, genericArgs[1]);

        Func<object, object> getKeyFunc = null;
        Func<object, object> getValueFunc = null;

        foreach (object o in (IEnumerable)value)
        {
          if (getKeyFunc == null)
          {
            Type objectType = o.GetType();
            if (!TypeCache.TryGetNamedAccessor(objectType, "Key", ref getKeyFunc))
            {
              getKeyFunc = InvocationHelper.CreateGetDelegate(o.GetType(), keyType, objectType.GetProperty("Key").GetMethod);
              TypeCache.AddTypeNamedAccessor(objectType, "Key", getKeyFunc);
            }
            if (!TypeCache.TryGetNamedAccessor(objectType, "Value", ref getValueFunc))
            {
              getValueFunc = InvocationHelper.CreateGetDelegate(o.GetType(), valueType, objectType.GetProperty("Value").GetMethod);
              TypeCache.AddTypeNamedAccessor(objectType, "Value", getValueFunc);
            }
          }

          IDataAdapter itemEl = data.AddChild(settings.ContainerItemName);
          SerializeValue(keyType, getKeyFunc(o), itemEl, keyInfo, false);
          SerializeValue(valueType, getValueFunc(o), itemEl, valueInfo, false);
        }
        haveContainers = true;

        return;
      }

      // generics
      if (category == TypeCategory.GenericIList ||
          category == TypeCategory.ISet ||
          category == TypeCategory.GenericQueue ||
          category == TypeCategory.GenericStack ||
          category == TypeCategory.LinkedList)
      {
        SerializeContainer((IEnumerable)value, genericArgs[0], data);
        return;
      }

      // non-generic versions
      if (category == TypeCategory.IList ||
          category == TypeCategory.Queue ||
          category == TypeCategory.Stack)
      {
        SerializeContainer((IEnumerable)value, typeof(object), data);
        return;
      }
    }

    #region ISubSerializer

    void ISubSerializer.ContinueSerialization(Type type, object value, IDataAdapter data)
    {
      SerializeSubValue(type, value, data, type.GetGenericArguments());
    }

    #endregion
  }
}

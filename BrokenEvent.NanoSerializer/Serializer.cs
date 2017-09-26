using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using BrokenEvent.NanoSerializer.Adapter;
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
        this(member.Name, member.Location, member.TypeCategory, member.GenericArguments) { }

      private Info(string name, NanoLocation location, TypeCategory category, Type[] genericArgs)
      {
        Name = name;
        Location = location;
        Category = category;
        GenericArgs = genericArgs;
      }
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
    /// Creates instance of the serializer with the default settings (<see cref="SerializationSettings.Instance"/>).
    /// </summary>
    public Serializer(): this(SerializationSettings.Instance)
    {
    }

    private Dictionary<object, int> objectsCache = new Dictionary<object, int>();
    private int maxObjId = 1;
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
    public static void Serialize<TTarget>(IDataAdapter data, TTarget target)
    {
      new Serializer().SerializeObject(data, target);
    }

    /// <summary>
    /// Serialize the object.
    /// </summary>
    /// <param name="data">Data carrier to serialize to</param>
    /// <param name="target">Object or root of object model to be serialized</param>
    /// <exception cref="ArgumentNullException">thrown if <paramref name="data"/> is null or <paramref name="target"/> is null</exception>
    public void SerializeObject<TTarget>(IDataAdapter data, TTarget target)
    {
      if (data == null)
        throw new ArgumentNullException(nameof(data));
      if (target == null)
        throw new ArgumentNullException(nameof(target));

      haveContainers = false;
      haveReferences = false;
      havePrivateProperties = false;

      SerializeSubValue(typeof(TTarget), target, data, null);

      if (!settings.SaveOptimizationFlags)
        return;

      OptimizationFlags flags = 0;
      if (!haveContainers)
        flags |= OptimizationFlags.NoContainers;
      if (!haveReferences)
        flags |= OptimizationFlags.NoReferences;
      if (havePrivateProperties)
        flags |= OptimizationFlags.PrivateProperties;
      if (settings.EnumsAsValue)
        flags |= OptimizationFlags.EnumAsValue;

      if (flags != 0)
        data.AddIntValue((long)flags, ATTRIBUTE_FLAGS, true);
    }

    private void SerializeValue(IDataAdapter data, object target)
    {
      objectsCache.Add(target, maxObjId++);
      TypeWrapper wrapper = TypeCache.GetWrapper(target.GetType());

      if (wrapper.TrySerialize(target, data, this))
        return;

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
        else if (settings.SerializeNull)
          data.AddNullValue(property.Name, property.Location != NanoLocation.SubNode);
      }

      for (int i = 0; i < wrapper.Fields.Count; i++)
      {
        FieldWrapper field = wrapper.Fields[i];

        if (!settings.IgnoreNotSerialized && field.Info.IsNotSerialized)
          continue;

        object value = field.GetValue(target);
        if (value != null)
          SerializeValue(field.MemberType, value, data, new Info(field), false);
        else if (settings.SerializeNull)
          data.AddNullValue(field.Name, field.Location != NanoLocation.SubNode);
      }
    }

    private void SerializeValue(Type type, object value, IDataAdapter data, Info info, bool isReadOnly)
    {
      if (info.Category == TypeCategory.Primitive)
      {
        // no need to serialize
        if (isReadOnly)
          return;

        SerializePrimitive(type, value, data, info.Name, info.Location != NanoLocation.SubNode);
        return;
      }
      if (info.Category == TypeCategory.Enum)
      {
        // no need to serialize
        if (isReadOnly)
          return;

        if (settings.EnumsAsValue)
          data.AddIntValue(((IConvertible)value).ToInt64(null), info.Name, info.Location != NanoLocation.SubNode);
        else
          data.AddStringValue(value.ToString(), info.Name, info.Location != NanoLocation.SubNode);
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
      if (settings.EnableObjectCache && type.IsClass && objectsCache.TryGetValue(value, out objId))
      {
        data.AddIntValue(objId, ATTRIBUTE_OBJID, true);
        haveReferences = true;
        return;
      }

      Type targetType = value.GetType();
      if (targetType != type)
      {
        if (settings.EnableTypeMarkers)
          data.AddStringValue(TypeCache.GetTypeFullName(targetType, settings.AssemblyQualifiedNames), ATTRIBUTE_TYPE, true);
        type = targetType;
        genericArgs = type.GetGenericArguments();
      }

      TypeCategory category = GetTypeCategory(type);

      switch (category)
      {
        case TypeCategory.Primitive:
          data.SetStringValue(value.ToString());
          break;
        case TypeCategory.Enum:
          if (settings.EnumsAsValue)
            data.SetIntValue(((IConvertible)value).ToInt64(null));
          else
            data.SetStringValue(value.ToString());
          break;
        case TypeCategory.Unknown:
          SerializeValue(data, value);
          break;
        default:
          SerializeContainer(value, data, category, genericArgs);
          break;
      }
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    private void SerializeContainer(IEnumerable e, Type elementType, IDataAdapter data)
    {
      if (settings.PrimitiveAsBase64 && elementType.IsPrimitive)
      {
        int size = ByteUtils.GetSizeOf(elementType);
        int count = 0;
        // ReSharper disable once UnusedVariable
        foreach (object o in e)
          count++;

        Action<byte[], int, object> writer = ByteUtils.GetBinaryWriter(elementType);

        byte[] buffer = new byte[size * count];
        int index = 0;
        foreach (object o in e)
        {
          writer(buffer, index, o);
          index += size;
        }
        data.SetStringValue(Convert.ToBase64String(buffer));
      }
      else
      {
        IDataArray array = data.AddArray();
        foreach (object o in e)
          SerializeSubValue(elementType, o, array.AddArrayValue(), null);
      }

      haveContainers = true;
    }

    private void SerializeArrayRank(Array array, Type elementType, int[] coords, int r, IDataAdapter data)
    {
      if (r == coords.Length - 1)
      {
        if (settings.PrimitiveAsBase64 && elementType.IsPrimitive)
        {
          int size = ByteUtils.GetSizeOf(elementType);
          int count = array.GetLength(r);
          Action<byte[], int, object> writer = ByteUtils.GetBinaryWriter(elementType);

          byte[] buffer = new byte[size * count];
          int index = 0;
          for (int i = 0; i < count; i++)
          {
            coords[r] = i;
            writer(buffer, index, array.GetValue(coords));
            index += size;
          }
          data.SetStringValue(Convert.ToBase64String(buffer));
        }
        else
        {
          IDataArray a = data.AddArray();
          for (int i = 0; i < array.GetLength(r); i++)
          {
            coords[r] = i;
            SerializeSubValue(elementType, array.GetValue(coords), a.AddArrayValue(), null);
          }
        }
      }
      else
      {
        IDataArray a = data.AddArray();
        for (int i = 0; i < array.GetLength(r); i++)
        {
          coords[r] = i;
          SerializeArrayRank(array, elementType, coords, r + 1, a.AddArrayValue());
        }
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
        int[] coords = new int[array.Rank];
        if (array.Rank > 1)
          data.AddIntValue(array.Rank, ATTRIBUTE_ARRAY_RANK, true);
        SerializeArrayRank(array, elementType, coords, 0, data);

        haveContainers = true;
        return;
      }

      // dictionaries
      if (category == TypeCategory.IDictionary)
      {
        Type keyType = genericArgs[0];
        Type valueType = genericArgs[1];

        Func<object, object> getKeyFunc = null;
        Func<object, object> getValueFunc = null;

        IDataArray array = data.AddArray();

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


          IDataAdapter itemEl = array.AddArrayValue();
          SerializeSubValue(keyType, getKeyFunc(o), itemEl.AddChild(settings.DictionaryKeyName), null);
          SerializeSubValue(valueType, getValueFunc(o), itemEl.AddChild(settings.DictionaryValueName), null);
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace BrokenEvent.NanoSerializer
{
  /// <summary>
  /// Core class for NanoSerializer serialization.
  /// </summary>
  public sealed class Serializer: SerializationBase
  {
    private struct Info
    {
      public NanoLocation Location;
      public string Name;

      public Info(NanoLocation location, string name)
      {
        Location = location;
        Name = name;
      }

      public Info(string name, NanoSerializationAttribute attr): this()
      {
        Name = name;
        if (attr == null)
          Location = NanoLocation.Auto;
        else
          Location = attr.Location;
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
    /// Creates instance of the serializer with the default settings.
    /// </summary>
    public Serializer(): this(new SerializationSettings())
    {
    }

    private Dictionary<object, int> objectsCache = new Dictionary<object, int>();
    private int maxObjId = 0;
    private bool haveContainers = false;
    private bool haveReferences = false;
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

      SerializeValue(data, target);

      if (!settings.SaveOptimizationFlags)
        return;

      OptimizationFlags flags = 0;
      if (!haveContainers)
        flags |= OptimizationFlags.NoContainers;
      if (!haveReferences)
        flags |= OptimizationFlags.NoReferences;

      if (flags != 0)
        data.AddAttribute(ATTRIBUTE_FLAGS, ((int)flags).ToString(), true);
    }

    private void SerializeValue(IDataAdapter data, object target)
    {
      objectsCache.Add(target, maxObjId++);

      foreach (PropertyInfo info in target.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
      {
        NanoSerializationAttribute attr = info.GetCustomAttribute<NanoSerializationAttribute>();

        if (attr != null && attr.State == NanoState.Ignore)
          continue;

        if (!info.CanRead)
          continue;

        bool isReadOnly = !info.CanWrite;
        if (attr != null && attr.ConstructorArg != -1)
          isReadOnly = false; // will be used in constructor
        if (settings.SerializeReadOnly)
          isReadOnly = false; // always serialize

        object value = info.GetValue(target);
        if (value != null)
          SerializeValue(info.PropertyType, value, data, new Info(info.Name, attr), isReadOnly);
      }

      foreach (FieldInfo info in target.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
      {
        NanoSerializationAttribute attr = info.GetCustomAttribute<NanoSerializationAttribute>();

        if (attr != null && attr.State == NanoState.Ignore)
          continue;

        if (!settings.IgnoreNotSerialized && info.IsNotSerialized)
          continue;

        object value = info.GetValue(target);
        if (value != null)
          SerializeValue(info.FieldType, value, data, new Info(info.Name, attr), false);
      }
    }

    private void SerializeValue(Type type, object value, IDataAdapter data, Info info, bool isReadOnly)
    {
      bool isPrimitive = IsPrimitive(type);

      if (isPrimitive)
      {
        // no need to serialize
        if (isReadOnly)
          return;

        if (info.Location == NanoLocation.SubNode)
          data.AddChild(info.Name).Value = value.ToString();
        else
          data.AddAttribute(info.Name, value.ToString(), false);

        return;
      }

      if (info.Location == NanoLocation.Attribute)
        throw new SerializationException($"Unable to serialize {type.Name} to attribute");

      IDataAdapter subNode = data.AddChild(info.Name);

      int objId;
      if (objectsCache.TryGetValue(value, out objId))
      {
        subNode.AddAttribute(ATTRIBUTE_OBJID, objId.ToString(), true);
        haveReferences = true;
        return;
      }

      Type valueType = value.GetType();
      if (valueType != type)
      {
        string typeName = valueType.Assembly.GetName().Name == "mscorlib" || !settings.AssemblyQualifiedNames ?
          valueType.FullName :
          valueType.AssemblyQualifiedName;
        subNode.AddAttribute(ATTRIBUTE_TYPE, typeName, true);
        type = valueType;
      }

      if (IsPrimitive(type))
      {
        subNode.Value = value.ToString();
        return;
      }

      if (SerializeContainer(value, subNode))
        return;

      SerializeValue(subNode, value);
    }

    private void SerializeContainer(IEnumerable e, Type elementType, IDataAdapter data)
    {
      Info info = new Info(NanoLocation.SubNode, settings.ContainerItemName);
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

    private bool SerializeContainer(object value, IDataAdapter data)
    {
      Type type = value.GetType();
      if (type.IsArray)
      {
        Array array = (Array)value;
        Type elementType = type.GetElementType();
        Info info = new Info(NanoLocation.SubNode, settings.ArrayItemName);
        int[] coords = new int[array.Rank];
        SerializeArrayRank(array, elementType, coords, 0, data, info);
        data.AddAttribute(ATTRIBUTE_ARRAY_RANK, array.Rank.ToString(), true);

        haveContainers = true;
        return true;
      }

      IList list = value as IList;
      if (list != null)
      {
        Type elementType = type.IsGenericType ? type.GetGenericArguments()[0] : typeof(object);
        SerializeContainer(list, elementType, data);
        return true;
      }

      if (type.IsGenericType && HaveInterface(type.GetGenericTypeDefinition(), typeof(IDictionary<,>)))
      {
        Info keyInfo = new Info(NanoLocation.SubNode, settings.DictionaryKeyName);
        Info valueInfo = new Info(NanoLocation.SubNode, settings.DictionaryValueName);

        Type keyType = type.GetGenericArguments()[0];
        Type valueType = type.GetGenericArguments()[1];

        PropertyInfo keyPropertyInfo = null;
        PropertyInfo valuePropertyInfo = null;

        foreach (object o in (IEnumerable)value)
        {
          if (keyPropertyInfo == null)
          {
            keyPropertyInfo = o.GetType().GetProperty(settings.DictionaryKeyName);
            valuePropertyInfo = o.GetType().GetProperty(settings.DictionaryValueName);
          }

          IDataAdapter itemEl = data.AddChild(settings.ContainerItemName);
          SerializeValue(keyType, keyPropertyInfo.GetValue(o), itemEl, keyInfo, false);
          SerializeValue(valueType, valuePropertyInfo.GetValue(o), itemEl, valueInfo, false);
        }
        haveContainers = true;

        return true;
      }

      ICollection collection = value as ICollection;
      if (collection != null)
      {
        Type elementType = type.IsGenericType ? type.GetGenericArguments()[0] : typeof(object);
        Array array = Array.CreateInstance(elementType, collection.Count);
        collection.CopyTo(array, 0);
        SerializeContainer(array, elementType, data);
        return true;
      }

      if (type.IsGenericType && HaveInterface(type.GetGenericTypeDefinition(), typeof(ISet<>)))
      {
        Type elementType = type.GetGenericArguments()[0];
        SerializeContainer((IEnumerable)value, elementType, data);
        return true;
      }

      if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(LinkedList<>))
      {
        Type elementType = type.GetGenericArguments()[0];
        SerializeContainer((IEnumerable)value, elementType, data);
        return true;
      }

      return false;
    }
  }
}

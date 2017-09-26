using System;
using System.Collections;
using System.Collections.Generic;

using BrokenEvent.NanoSerializer.Adapter;
using BrokenEvent.NanoSerializer.Caching;

#pragma warning disable 1591

namespace BrokenEvent.NanoSerializer
{
  /// <summary>
  /// Base class contains all common methods for <see cref="Serializer"/> and <see cref="Deserializer"/>.
  /// </summary>
  public abstract class SerializationBase
  {
    public const string ATTRIBUTE_TYPE = "_type";
    public const string ATTRIBUTE_OBJID = "_objId";
    public const string ATTRIBUTE_FLAGS = "_flags";
    public const string ATTRIBUTE_ARRAY_RANK = "_r";
    public const string ELEMENT_TYPES = "_types";

    // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/built-in-types-table
    private static Dictionary<Type, Action<object, IDataAdapter, string, bool>> primitiveSerializers = new Dictionary<Type, Action<object, IDataAdapter, string, bool>>
    {
      { typeof(bool), (o, d, n, a) => d.AddBoolValue((bool)o, n, a) },
      { typeof(byte), (o, d, n, a) => d.AddIntValue((byte)o, n, a) },
      { typeof(sbyte), (o, d, n, a) => d.AddIntValue((sbyte)o, n, a) },
      { typeof(char), (o, d, n, a) => d.AddStringValue(((char)o).ToString(), n, a) },
      { typeof(decimal), (o, d, n, a) => d.AddFloatValue((double)(decimal)o, n, a) },
      { typeof(double), (o, d, n, a) => d.AddFloatValue((double)o, n, a) },
      { typeof(float), (o, d, n, a) => d.AddFloatValue((float)o, n, a) },
      { typeof(int), (o, d, n, a) => d.AddIntValue((int)o, n, a) },
      { typeof(uint), (o, d, n, a) => d.AddIntValue((uint)o, n, a) },
      { typeof(long), (o, d, n, a) => d.AddIntValue((long)o, n, a) },
      { typeof(ulong), (o, d, n, a) => d.AddIntValue((long)(ulong)o, n, a) },
      { typeof(short), (o, d, n, a) => d.AddIntValue((short)o, n, a) },
      { typeof(ushort), (o, d, n, a) => d.AddIntValue((ushort)o, n, a) },
      { typeof(string), (o, d, n, a) => d.AddStringValue((string)o, n, a) },
    };

    private static Dictionary<Type, Func<IDataAdapter, object>> primitiveDeserializers = new Dictionary<Type, Func<IDataAdapter, object>>
    {
      { typeof(bool), d => d.GetBoolValue() },
      { typeof(byte), d => (byte)d.GetIntValue() },
      { typeof(sbyte), d => (sbyte)d.GetIntValue() },
      { typeof(char), d => d.GetStringValue()[0] },
      { typeof(decimal), d => (decimal)d.GetFloatValue() },
      { typeof(double), d => d.GetFloatValue() },
      { typeof(float), d => (float)d.GetFloatValue() },
      { typeof(int), d => (int)d.GetIntValue() },
      { typeof(uint), d => (uint)d.GetIntValue() },
      { typeof(long), d => d.GetIntValue() },
      { typeof(ulong), d => (ulong)d.GetIntValue() },
      { typeof(short), d => (short)d.GetIntValue() },
      { typeof(ushort), d => (ushort)d.GetIntValue() },
      { typeof(string), d => d.GetStringValue() },
    };

    private static Dictionary<Type, Func<IDataAdapter, string, bool, object>> primitiveNamedDeserializers = new Dictionary<Type, Func<IDataAdapter, string, bool, object>>
    {
      { typeof(bool), (d, n, a) => d.GetBoolValue(n, a) },
      { typeof(byte), (d, n, a) => (byte)d.GetIntValue(n, a) },
      { typeof(sbyte), (d, n, a) => (sbyte)d.GetIntValue(n, a) },
      { typeof(char), (d, n, a) => d.GetStringValue(n, a)[0] },
      { typeof(decimal), (d, n, a) => (decimal)d.GetFloatValue(n, a) },
      { typeof(double), (d, n, a) => d.GetFloatValue(n, a) },
      { typeof(float), (d, n, a) => (float)d.GetFloatValue(n, a) },
      { typeof(int), (d, n, a) => (int)d.GetIntValue(n, a) },
      { typeof(uint), (d, n, a) => (uint)d.GetIntValue(n, a) },
      { typeof(long), (d, n, a) => d.GetIntValue(n, a) },
      { typeof(ulong), (d, n, a) => (ulong)d.GetIntValue(n, a) },
      { typeof(short), (d, n, a) => (short)d.GetIntValue(n, a) },
      { typeof(ushort), (d, n, a) => (ushort)d.GetIntValue(n, a) },
      { typeof(string), (d, n, a) => d.GetStringValue(n, a) },
    };

    [Flags]
    protected enum OptimizationFlags
    {
      NoContainers = 1 << 0,
      NoReferences = 1 << 1,
      PrivateProperties = 1 << 2,
      EnumAsValue = 1 << 3
    }

    protected struct Tuple<T1, T2>
    {
      public T1 Value1;
      public T2 Value2;

      public Tuple(T1 value1, T2 value2)
      {
        Value1 = value1;
        Value2 = value2;
      }
    }

    internal static bool IsPrimitive(Type type)
    {
      return primitiveSerializers.ContainsKey(type) || type.IsEnum;
    }

    protected void SerializePrimitive(Type type, object value, IDataAdapter data, string name, bool isAttribute)
    {
      Action<object, IDataAdapter, string, bool> action;
      if (primitiveSerializers.TryGetValue(type, out action))
      {
        action(value, data, name, isAttribute);
        return;
      }

      throw new InvalidOperationException();
    }

    protected static object DeserializePrimitive(Type type, IDataAdapter data, bool enumAsValue)
    {
      Func<IDataAdapter, object> func;
      if (primitiveDeserializers.TryGetValue(type, out func))
        return func(data);

      if (type.IsEnum)
      {
        if (enumAsValue)
          return Enum.ToObject(type, data.GetIntValue());

        return Enum.Parse(type, data.GetStringValue());
      }

      return null;
    }

    protected static object DeserializePrimitive(Type type, IDataAdapter data, string name, bool isAttribute, bool enumAsValue)
    {
      Func<IDataAdapter, string, bool, object> func;
      if (primitiveNamedDeserializers.TryGetValue(type, out func))
        return func(data, name, isAttribute);

      if (type.IsEnum)
      {
        if (enumAsValue)
          return Enum.ToObject(type, data.GetIntValue(name, isAttribute));

        return Enum.Parse(type, data.GetStringValue(name, isAttribute));
      }

      return null;
    }

    protected static bool CompareTypesSafe(Type a, Type b)
    {
      return a.Name == b.Name &&
             a.Namespace == b.Namespace &&
             a.Assembly == b.Assembly;
    }

    internal static bool HaveInterface(Type type, Type iface)
    {
      if (CompareTypesSafe(type, iface))
        return true;

      foreach (Type @interface in TypeCache.GetTypeInterfaces(type))
        if (CompareTypesSafe(@interface, iface))
          return true;

      return false;
    }

    internal static TypeCategory GetTypeCategory(Type type)
    {
      TypeCategory category;
      if (TypeCache.TryGetCategory(type, out category))
        return category;

      category = GetTypeCategoryDetect(type);
      TypeCache.AddTypeCategory(type, category);
      return category;
    }

    internal static TypeCategory GetTypeCategoryDetect(Type type)
    {
      if (type.IsEnum)
        return TypeCategory.Enum;

      if (IsPrimitive(type))
        return TypeCategory.Primitive;

      if (type.IsArray)
        return TypeCategory.Array;

      if (type.IsGenericType)
      {
        Type genericType = type.GetGenericTypeDefinition();

        if (HaveInterface(type, typeof(IList<>)))
          return TypeCategory.GenericIList;

        if (genericType == typeof(Queue<>))
          return TypeCategory.GenericQueue;

        if (genericType == typeof(Stack<>))
          return TypeCategory.GenericStack;

        if (HaveInterface(genericType, typeof(ISet<>)))
          return TypeCategory.ISet;

        if (genericType == typeof(LinkedList<>))
          return TypeCategory.LinkedList;

        if (HaveInterface(genericType, typeof(IDictionary<,>)))
          return TypeCategory.IDictionary;
      }
      else
      {
        if (HaveInterface(type, typeof(IList)))
          return TypeCategory.IList;

        if (type == typeof(Queue))
          return TypeCategory.Queue;

        if (type == typeof(Stack))
          return TypeCategory.Stack;
      }

      return TypeCategory.Unknown;
    }
  }
}

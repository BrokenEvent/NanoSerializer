using System;
using System.Collections;
using System.Collections.Generic;

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

    // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/built-in-types-table
    private static Dictionary<Type, Func<string, object>> primitiveTypes = new Dictionary<Type, Func<string, object>>
    {
      { typeof(bool), s => bool.Parse(s) },
      { typeof(byte), s => byte.Parse(s) },
      { typeof(sbyte), s => sbyte.Parse(s) },
      { typeof(char), s => char.Parse(s) },
      { typeof(decimal), s => decimal.Parse(s) },
      { typeof(double), s => double.Parse(s) },
      { typeof(float), s => float.Parse(s) },
      { typeof(int), s => int.Parse(s) },
      { typeof(uint), s => uint.Parse(s) },
      { typeof(long), s => long.Parse(s) },
      { typeof(ulong), s => ulong.Parse(s) },
      { typeof(short), s => short.Parse(s) },
      { typeof(ushort), s => ushort.Parse(s) },
      { typeof(string), s => s },
    };

    [Flags]
    protected enum OptimizationFlags
    {
      NoContainers = 1 << 0,
      NoReferences = 1 << 1,
      PrivateProperties = 1 << 2,
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
      return primitiveTypes.ContainsKey(type) || type.IsEnum;
    }

    protected static object DeserializePrimitive(Type type, string value)
    {
      Func<string, object> func;
      if (primitiveTypes.TryGetValue(type, out func))
        return func(value);

      if (type.IsEnum)
        return Enum.Parse(type, value);

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

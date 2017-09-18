using System;
using System.Collections.Generic;

namespace BrokenEvent.NanoSerializer.Caching
{
  internal static class TypeCache
  {
    private static Dictionary<Type, TypeWrapper> wrappers = new Dictionary<Type, TypeWrapper>();
    private static Dictionary<Type, string> typeFullNames = new Dictionary<Type, string>();
    private static Dictionary<Type, string> typeAssemblyNames = new Dictionary<Type, string>();
    private static Dictionary<Type, object> typeAccessors = new Dictionary<Type, object>();
    private static Dictionary<Type, Dictionary<string, object>> typeNamesAccessors = new Dictionary<Type, Dictionary<string, object>>();
    private static Dictionary<Type, Type[]> typeInterfaces = new Dictionary<Type, Type[]>();
    private static Dictionary<Type, Type[]> typeGenericArgs = new Dictionary<Type, Type[]>();
    private static Dictionary<Type, Func<object>> typeConstructors = new Dictionary<Type, Func<object>>();

    public static TypeWrapper GetWrapper(Type type)
    {
      TypeWrapper result;
      if (wrappers.TryGetValue(type, out result))
        return result;

      result = new TypeWrapper(type);
      wrappers.Add(type, result);
      return result;
    }

    public static string GetTypeFullName(Type type, bool useAssemblyName)
    {
      if (useAssemblyName)
      {
        string result;
        if (typeAssemblyNames.TryGetValue(type, out result))
          return result;

        result = type.Assembly.GetName().Name == "mscorlib" ? type.FullName : type.AssemblyQualifiedName;
        typeAssemblyNames.Add(type, result);
        return result;
      }
      else
      {
        string result;
        if (typeFullNames.TryGetValue(type, out result))
          return result;

        result = type.FullName;
        typeFullNames.Add(type, result);
        return result;
      }
    }

    public static bool TryGetTypeAccessor<TAccessor>(Type type, ref TAccessor accessor)
      where TAccessor : class
    {
      object value;
      if (!typeAccessors.TryGetValue(type, out value))
        return false;

      accessor = (TAccessor)value;
      return true;
    }

    public static void AddTypeAccessor(Type type, object accessor)
    {
      typeAccessors.Add(type, accessor);
    }

    public static bool TryGetNamedAccessor<TAccessor>(Type type, string name, ref TAccessor accessor)
      where TAccessor : class
    {
      Dictionary<string, object> dict;
      if (!typeNamesAccessors.TryGetValue(type, out dict))
        return false;

      object value;
      if (!dict.TryGetValue(name, out value))
        return false;

      accessor = (TAccessor)value;
      return true;
    }

    public static void AddTypeNamedAccessor(Type type, string name, object accessor)
    {
      Dictionary<string, object> dict;
      if (!typeNamesAccessors.TryGetValue(type, out dict))
        typeNamesAccessors.Add(type, dict = new Dictionary<string, object>());

      dict.Add(name, accessor);
    }

    public static Type[] GetTypeInterfaces(Type type)
    {
      Type[] result;
      if (typeInterfaces.TryGetValue(type, out result))
        return result;

      result = type.GetInterfaces();
      typeInterfaces.Add(type, result);

      return result;
    }

    public static Type[] GetTypeGenericArgs(Type type)
    {
      if (!type.IsGenericType)
        return null;

      Type[] result;
      if (typeGenericArgs.TryGetValue(type, out result))
        return result;

      result = type.GetGenericArguments();
      typeGenericArgs.Add(type, result);

      return result;
    }

    public static object CreateParameterless(Type type)
    {
      Func<object> func;
      if (!typeConstructors.TryGetValue(type, out func))
      {
        func = InvocationHelper.CreateConstructorDelegate(type);
        typeConstructors.Add(type, func);
      }

      return func();
    }
  }
}

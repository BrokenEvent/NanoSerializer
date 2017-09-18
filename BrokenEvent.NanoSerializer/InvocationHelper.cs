using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace BrokenEvent.NanoSerializer
{
  internal static class InvocationHelper
  {
    private static readonly MethodInfo genericSetHelper1;
    private static readonly MethodInfo genericSetHelper2;
    private static readonly MethodInfo genericGetHelper;
    private static readonly MethodInfo genericGetStructHelper;
    private static readonly MethodInfo genericGetSetHelper;

    static InvocationHelper()
    {
      genericSetHelper1 = typeof(InvocationHelper).GetMethod(nameof(GenericSetHelper1), BindingFlags.Static | BindingFlags.NonPublic);
      genericSetHelper2 = typeof(InvocationHelper).GetMethod(nameof(GenericSetHelper2), BindingFlags.Static | BindingFlags.NonPublic);
      genericGetHelper = typeof(InvocationHelper).GetMethod(nameof(GenericGetHelper), BindingFlags.Static | BindingFlags.NonPublic);
      genericGetStructHelper = typeof(InvocationHelper).GetMethod(nameof(GenericGetStructHelper), BindingFlags.Static | BindingFlags.NonPublic);
      genericGetSetHelper = typeof(InvocationHelper).GetMethod(nameof(GenericGetSetHelper), BindingFlags.Static | BindingFlags.NonPublic);
    }

    private static Dictionary<Type, Dictionary<string, Action<object, object>>> namedSet1Cache =
      new Dictionary<Type, Dictionary<string, Action<object, object>>>();
    private static Dictionary<Type, Dictionary<MethodInfo, Action<object, object>>> unnamedSet1Cache =
      new Dictionary<Type, Dictionary<MethodInfo, Action<object, object>>>();
    private static Dictionary<Type, Dictionary<string, Action<object, object, object>>> namedSet2Cache =
      new Dictionary<Type, Dictionary<string, Action<object, object, object>>>();
    private static Dictionary<Type, Dictionary<MethodInfo, Action<object, object, object>>> unnamedSet2Cache =
      new Dictionary<Type, Dictionary<MethodInfo, Action<object, object, object>>>();
    private static Dictionary<Type, Dictionary<PropertyInfo, Func<object, object>>> propertyGetCache =
      new Dictionary<Type, Dictionary<PropertyInfo, Func<object, object>>>();
    private static Dictionary<Type, Dictionary<MethodInfo, Func<object, object, object>>> getSetCache =
      new Dictionary<Type, Dictionary<MethodInfo, Func<object, object, object>>>();

    // it is not thread safe even without this
    private static object[] argsCache1 = new object[1];
    private static Type[] typeCache1 = new Type[1];
    private static Type[] typeCache2 = new Type[2];

    private static Action<object, object> GenericSetHelper1<TTarget, TArg>(MethodInfo info)
    {
      Action<TTarget, TArg> action = (Action<TTarget, TArg>)Delegate.CreateDelegate(typeof(Action<TTarget, TArg>), info);
      return (target, arg) => action((TTarget)target, (TArg)arg);
    }

    private static Action<object, object, object> GenericSetHelper2<TTarget, TArg1, TArg2>(MethodInfo info)
    {
      Action<TTarget, TArg1, TArg2> action = (Action<TTarget, TArg1, TArg2>)Delegate.CreateDelegate(typeof(Action<TTarget, TArg1, TArg2>), info);
      return (target, arg1, arg2) => action((TTarget)target, (TArg1)arg1, (TArg2)arg2);
    }

    private static Func<object, object> GenericGetHelper<TTarget, TResult>(MethodInfo info)
    {
      Func<TTarget, TResult> func = (Func<TTarget, TResult>)Delegate.CreateDelegate(typeof(Func<TTarget, TResult>), info);
      return target => { return func((TTarget)target); };
    }

    private delegate TResult FuncRef<TTarget, TResult>(ref TTarget target);

    private static Func<object, object> GenericGetStructHelper<TTarget, TResult>(MethodInfo info)
    {
      FuncRef<TTarget, TResult> func = (FuncRef<TTarget, TResult>)Delegate.CreateDelegate(typeof(FuncRef<TTarget, TResult>), info);
      return target =>
        {
          TTarget t = (TTarget)target;
          return func(ref t);
        };
    }

    private static Func<object, object, object> GenericGetSetHelper<TTarget, TArg, TResult>(MethodInfo info)
    {
      Func<TTarget, TArg, TResult> func = (Func<TTarget, TArg, TResult>)Delegate.CreateDelegate(typeof(Func<TTarget, TArg, TResult>), info);
      return (target, arg) => { return func((TTarget)target, (TArg)arg); };
    }

    public static Action<object, object> CreateSetDelegate(Type type, Type argType, MethodInfo info)
    {
      MethodInfo actualHelper = genericSetHelper1.MakeGenericMethod(type, argType);
      argsCache1[0] = info;
      return (Action<object, object>)actualHelper.Invoke(null, argsCache1);
    }

    public static Action<object, object> CreateSetDelegate(Type type, Type argType, string name)
    {
      typeCache1[0] = argType;
      MethodInfo info = type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public, null, typeCache1, null);
      MethodInfo actualHelper = genericSetHelper1.MakeGenericMethod(type, argType);
      argsCache1[0] = info;
      return (Action<object, object>)actualHelper.Invoke(null, argsCache1);
    }

    public static Action<object, object, object> CreateSetDelegate(Type type, Type arg1Type, Type arg2Type, MethodInfo info)
    {
      MethodInfo actualHelper = genericSetHelper2.MakeGenericMethod(type, arg1Type, arg2Type);
      argsCache1[0] = info;
      return (Action<object, object, object>)actualHelper.Invoke(null, argsCache1);
    }

    // TODO cleanup unused methods and caching
    public static Action<object, object, object> CreateSetDelegate(Type type, Type arg1Type, Type arg2Type, string name)
    {
      typeCache2[0] = arg1Type;
      typeCache2[1] = arg2Type;
      MethodInfo info = type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public, null, typeCache2, null);
      MethodInfo actualHelper = genericSetHelper2.MakeGenericMethod(type, arg1Type, arg2Type);
      argsCache1[0] = info;
      return (Action<object, object, object>)actualHelper.Invoke(null, argsCache1);
    }

    public static Func<object, object> CreateGetDelegate(Type type, Type resultType, MethodInfo info)
    {
      MethodInfo actualHelper;
      if (type.IsClass)
        actualHelper = genericGetHelper.MakeGenericMethod(type, resultType);
      else
        actualHelper = genericGetStructHelper.MakeGenericMethod(type, resultType);

      argsCache1[0] = info;
      return (Func<object, object>)actualHelper.Invoke(null, argsCache1);
    }

    public static Func<object, object, object> CreateGetSetDelegate(Type type, Type argType, Type resultType, MethodInfo info)
    {
      MethodInfo actualHelper = genericGetSetHelper.MakeGenericMethod(type, argType, resultType);
      argsCache1[0] = info;
      return (Func<object, object, object>)actualHelper.Invoke(null, argsCache1);
    }

    public static Func<object, object, object> CreateGetSetDelegate(Type type, Type argType, Type resultType, string name)
    {
      typeCache1[0] = argType;
      MethodInfo info = type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public, null, typeCache1, null);

      MethodInfo actualHelper = genericGetSetHelper.MakeGenericMethod(type, argType, resultType);
      argsCache1[0] = info;
      return (Func<object, object, object>)actualHelper.Invoke(null, argsCache1);
    }

    public static Func<object> CreateConstructorDelegate(Type type)
    {
      DynamicMethod method = new DynamicMethod("_create__" + type.AssemblyQualifiedName, type, null, typeof(InvocationHelper));
      ILGenerator ilGenerator = method.GetILGenerator();
      ilGenerator.Emit(OpCodes.Newobj, type.GetConstructor(Type.EmptyTypes));
      ilGenerator.Emit(OpCodes.Ret);
      return (Func<object>)method.CreateDelegate(typeof(Func<object>));
    }

    public static Action<object, object> GetSetDelegate(Type type, Type argType, string name)
    {
      Dictionary<string, Action<object, object>> typeDelegates;
      if (!namedSet1Cache.TryGetValue(type, out typeDelegates))
      {
        typeDelegates = new Dictionary<string, Action<object, object>>();
        namedSet1Cache.Add(type, typeDelegates);
      }

      Action<object, object> action;
      if (typeDelegates.TryGetValue(name, out action))
        return action;

      MethodInfo method = type.GetMethod(name, new []{argType});
      action = CreateSetDelegate(type, argType, method);
      typeDelegates.Add(name, action);
      return action;
    }

    public static Action<object, object> GetSetDelegate(Type type, Type argType, MethodInfo info)
    {
      Dictionary<MethodInfo, Action<object, object>> typeDelegates;
      if (!unnamedSet1Cache.TryGetValue(type, out typeDelegates))
      {
        typeDelegates = new Dictionary<MethodInfo, Action<object, object>>();
        unnamedSet1Cache.Add(type, typeDelegates);
      }

      Action<object, object> action;
      if (typeDelegates.TryGetValue(info, out action))
        return action;

      action = CreateSetDelegate(type, argType, info);
      typeDelegates.Add(info, action);
      return action;
    }

    public static Action<object, object, object> GetSetDelegate(Type type, Type arg1Type, Type arg2Type, string name)
    {
      Dictionary<string, Action<object, object, object>> typeDelegates;
      if (!namedSet2Cache.TryGetValue(type, out typeDelegates))
      {
        typeDelegates = new Dictionary<string, Action<object, object, object>>();
        namedSet2Cache.Add(type, typeDelegates);
      }

      Action<object, object, object> action;
      if (typeDelegates.TryGetValue(name, out action))
        return action;

      MethodInfo method = type.GetMethod(name, new []{arg1Type, arg2Type});
      action = CreateSetDelegate(type, arg1Type, arg2Type, method);
      typeDelegates.Add(name, action);
      return action;
    }

    public static Action<object, object, object> GetSetDelegate(Type type, Type arg1Type, Type arg2Type, MethodInfo info)
    {
      Dictionary<MethodInfo, Action<object, object, object>> typeDelegates;
      if (!unnamedSet2Cache.TryGetValue(type, out typeDelegates))
      {
        typeDelegates = new Dictionary<MethodInfo, Action<object, object, object>>();
        unnamedSet2Cache.Add(type, typeDelegates);
      }

      Action<object, object, object> action;
      if (typeDelegates.TryGetValue(info, out action))
        return action;

      action = CreateSetDelegate(type, arg1Type, arg2Type, info);
      typeDelegates.Add(info, action);
      return action;
    }

    public static void SetProperty(object target, Type type, PropertyInfo info, object value)
    {
      Action<object, object> action = GetSetDelegate(type, info.PropertyType, info.SetMethod);
      action(target, value);
    }

    public static Func<object, object> GetGetDelegate(Type type, Type resultType, PropertyInfo info)
    {
      Dictionary<PropertyInfo, Func<object, object>> typeDelegates;
      if (!propertyGetCache.TryGetValue(type, out typeDelegates))
      {
        typeDelegates = new Dictionary<PropertyInfo, Func<object, object>>();
        propertyGetCache.Add(type, typeDelegates);
      }

      Func<object, object> func;
      if (typeDelegates.TryGetValue(info, out func))
        return func;

      func = CreateGetDelegate(type, resultType, info.GetMethod);
      typeDelegates.Add(info, func);
      return func;
    }

    public static object GetProperty(object target, Type type, PropertyInfo info)
    {
      Func<object, object> func = GetGetDelegate(type, info.PropertyType, info);
      return func(target);
    }

    public static Func<object, object, object> GetGetSetDelegate(Type type, Type argType, Type resultType, MethodInfo info)
    {
      Dictionary<MethodInfo, Func<object, object, object>> typeDelegates;
      if (!getSetCache.TryGetValue(type, out typeDelegates))
      {
        typeDelegates = new Dictionary<MethodInfo, Func<object, object, object>>();
        getSetCache.Add(type, typeDelegates);
      }

      Func<object, object, object> func;
      if (typeDelegates.TryGetValue(info, out func))
        return func;

      func = CreateGetSetDelegate(type, argType, resultType, info);
      typeDelegates.Add(info, func);
      return func;
    }
  }
}

using System;
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

    // it is not thread safe even without this
    private static object[] argsCache1 = new object[1];
    private static Type[] typeCache1 = new Type[1];
    private static Type[] typeCache2 = new Type[2];
    private static Type[] typeCache3 = new Type[3];

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
      typeCache2[0] = type;
      typeCache2[1] = argType;
      MethodInfo actualHelper = genericSetHelper1.MakeGenericMethod(typeCache2);
      argsCache1[0] = info;
      return (Action<object, object>)actualHelper.Invoke(null, argsCache1);
    }

    public static Action<object, object> CreateSetDelegate(Type type, Type argType, string name)
    {
      typeCache1[0] = argType;
      MethodInfo info = type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public, null, typeCache1, null);

      typeCache2[0] = type;
      typeCache2[1] = argType;
      MethodInfo actualHelper = genericSetHelper1.MakeGenericMethod(typeCache2);
      argsCache1[0] = info;
      return (Action<object, object>)actualHelper.Invoke(null, argsCache1);
    }

    public static Action<object, object, object> CreateSetDelegate(Type type, Type arg1Type, Type arg2Type, string name)
    {
      typeCache2[0] = arg1Type;
      typeCache2[1] = arg2Type;
      MethodInfo info = type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public, null, typeCache2, null);

      typeCache3[0] = type;
      typeCache3[1] = arg1Type;
      typeCache3[2] = arg2Type;

      MethodInfo actualHelper = genericSetHelper2.MakeGenericMethod(typeCache3);
      argsCache1[0] = info;
      return (Action<object, object, object>)actualHelper.Invoke(null, argsCache1);
    }

    public static Func<object, object> CreateGetDelegate(Type type, Type resultType, MethodInfo info)
    {
      typeCache2[0] = type;
      typeCache2[1] = resultType;
      MethodInfo actualHelper;
      if (type.IsClass)
        actualHelper = genericGetHelper.MakeGenericMethod(typeCache2);
      else
        actualHelper = genericGetStructHelper.MakeGenericMethod(typeCache2);

      argsCache1[0] = info;
      return (Func<object, object>)actualHelper.Invoke(null, argsCache1);
    }

    public static Func<object, object, object> CreateGetSetDelegate(Type type, Type argType, Type resultType, string name)
    {
      typeCache1[0] = argType;
      MethodInfo info = type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public, null, typeCache1, null);

      typeCache3[0] = type;
      typeCache3[1] = argType;
      typeCache3[2] = resultType;
      MethodInfo actualHelper = genericGetSetHelper.MakeGenericMethod(typeCache3);
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
  }
}

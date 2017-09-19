using System;
using System.Reflection;
using System.Reflection.Emit;

namespace BrokenEvent.NanoSerializer
{
  internal static class InvocationHelper
  {
    // it is not thread safe even without this
    private static Type[] typeCache1 = new Type[1];
    private static Type[] typeCache2 = new Type[2];
    private static Type[] typeCache3 = new Type[3];

    private static void EmitLdc(ILGenerator il, int index)
    {
      switch (index)
      {
        case 0: il.Emit(OpCodes.Ldc_I4_0); break;
        case 1: il.Emit(OpCodes.Ldc_I4_1); break;
        case 2: il.Emit(OpCodes.Ldc_I4_2); break;
        case 3: il.Emit(OpCodes.Ldc_I4_3); break;
        case 4: il.Emit(OpCodes.Ldc_I4_4); break;
        case 5: il.Emit(OpCodes.Ldc_I4_5); break;
        case 6: il.Emit(OpCodes.Ldc_I4_6); break;
        case 7: il.Emit(OpCodes.Ldc_I4_7); break;
        case 8: il.Emit(OpCodes.Ldc_I4_8); break;
        default:
          il.Emit(OpCodes.Ldc_I4_S, index);
          break;
      }
    }

    private static Type GetOwnerType(Type type)
    {
      if (type.Assembly.GetName().Name == "mscorlib")
        return typeof(InvocationHelper);
      return type;
    }

    public static Action<object, object> CreateSetDelegate(Type type, Type argType, string name)
    {
      typeCache1[0] = argType;
      MethodInfo info = type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public, null, typeCache1, null);
      return CreateSetDelegate(type, argType, info);
    }

    public static Action<object, object> CreateSetDelegate(Type type, Type argType, MethodInfo info)
    {
      typeCache2[0] = typeof(object);
      typeCache2[1] = typeof(object);
      DynamicMethod method = new DynamicMethod("_call__" + type.FullName + "_" + info.Name, null, typeCache2, GetOwnerType(type));
      ILGenerator ilGenerator = method.GetILGenerator();

      ilGenerator.Emit(OpCodes.Ldarg_0);
      if (type.IsValueType)
        ilGenerator.Emit(OpCodes.Unbox, type);
      else
        ilGenerator.Emit(OpCodes.Castclass, type);

      ilGenerator.Emit(OpCodes.Ldarg_1);
      if (argType.IsValueType)
        ilGenerator.Emit(OpCodes.Unbox_Any, argType);
      else
        ilGenerator.Emit(OpCodes.Castclass, argType);

      ilGenerator.Emit(OpCodes.Call, info);
      ilGenerator.Emit(OpCodes.Ret);
      return (Action<object, object>)method.CreateDelegate(typeof(Action<object, object>));
    }

    public static Action<object, object, object> CreateSetDelegate(Type type, Type arg1Type, Type arg2Type, string name)
    {
      typeCache2[0] = arg1Type;
      typeCache2[1] = arg2Type;
      MethodInfo info = type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public, null, typeCache2, null);
      return CreateSetDelegate(type, arg1Type, arg2Type, info);
    }

    public static Action<object, object, object> CreateSetDelegate(Type type, Type arg1Type, Type arg2Type, MethodInfo info)
    {
      typeCache3[0] = typeof(object);
      typeCache3[1] = typeof(object);
      typeCache3[2] = typeof(object);
      DynamicMethod method = new DynamicMethod("_call__" + type.FullName + "_" + info.Name, null, typeCache3, GetOwnerType(type));
      ILGenerator ilGenerator = method.GetILGenerator();

      ilGenerator.Emit(OpCodes.Ldarg_0);
      if (type.IsValueType)
        ilGenerator.Emit(OpCodes.Unbox, type);
      else
        ilGenerator.Emit(OpCodes.Castclass, type);

      ilGenerator.Emit(OpCodes.Ldarg_1);

      if (arg1Type.IsValueType)
        ilGenerator.Emit(OpCodes.Unbox_Any, arg1Type);
      else
        ilGenerator.Emit(OpCodes.Castclass, arg1Type);

      ilGenerator.Emit(OpCodes.Ldarg_2);

      if (arg2Type.IsValueType)
        ilGenerator.Emit(OpCodes.Unbox_Any, arg2Type);
      else
        ilGenerator.Emit(OpCodes.Castclass, arg2Type);

      ilGenerator.Emit(OpCodes.Call, info);
      ilGenerator.Emit(OpCodes.Ret);
      return (Action<object, object, object>)method.CreateDelegate(typeof(Action<object, object, object>));
    }

    public static Func<object, object> CreateGetDelegate(Type type, Type resultType, MethodInfo info)
    {
      typeCache1[0] = typeof(object);
      DynamicMethod method = new DynamicMethod("_get__" + type.FullName + "_" + info.Name, typeof(object), typeCache1, GetOwnerType(type));
      ILGenerator ilGenerator = method.GetILGenerator();

      ilGenerator.Emit(OpCodes.Ldarg_0);
      if (!type.IsValueType)
        ilGenerator.Emit(OpCodes.Castclass, type);
      else
        ilGenerator.Emit(OpCodes.Unbox, type);

      ilGenerator.Emit(OpCodes.Callvirt, info);

      if (resultType.IsValueType)
        ilGenerator.Emit(OpCodes.Box, resultType);

      ilGenerator.Emit(OpCodes.Ret);
      return (Func<object, object>)method.CreateDelegate(typeof(Func<object, object>));
    }

    public static Func<object, object, object> CreateGetSetDelegate(Type type, Type argType, Type resultType, string name)
    {
      typeCache1[0] = argType;
      MethodInfo info = type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public, null, typeCache1, null);
      return CreateGetSetDelegate(type, argType, resultType, info);
    }

    public static Func<object, object, object> CreateGetSetDelegate(Type type, Type argType, Type resultType, MethodInfo info)
    {
      typeCache2[0] = typeof(object);
      typeCache2[1] = typeof(object);
      DynamicMethod method = new DynamicMethod("_get__" + type.FullName + "_" + info.Name, typeof(object), typeCache2, GetOwnerType(type));
      ILGenerator ilGenerator = method.GetILGenerator();

      ilGenerator.Emit(OpCodes.Ldarg_0);
      if (!type.IsValueType)
        ilGenerator.Emit(OpCodes.Castclass, type);
      else
        ilGenerator.Emit(OpCodes.Unbox, type);

      ilGenerator.Emit(OpCodes.Ldarg_1);
      if (argType.IsValueType)
        ilGenerator.Emit(OpCodes.Unbox_Any, argType);
      else
        ilGenerator.Emit(OpCodes.Castclass, argType);

      ilGenerator.Emit(OpCodes.Callvirt, info);

      if (resultType.IsValueType)
        ilGenerator.Emit(OpCodes.Box, resultType);

      ilGenerator.Emit(OpCodes.Ret);
      return (Func<object, object, object>)method.CreateDelegate(typeof(Func<object, object, object>));
    }

    public static Func<object> CreateConstructorDelegate(Type type)
    {
      DynamicMethod method = new DynamicMethod("_create__" + type.FullName, type, null, GetOwnerType(type));
      ILGenerator ilGenerator = method.GetILGenerator();

      if (type.IsClass)
        ilGenerator.Emit(OpCodes.Newobj, type.GetConstructor(Type.EmptyTypes));
      else
      {
        ilGenerator.Emit(OpCodes.Initobj, type);
        ilGenerator.Emit(OpCodes.Box, type);
      }

      ilGenerator.Emit(OpCodes.Ret);
      return (Func<object>)method.CreateDelegate(typeof(Func<object>));
    }

    public static Func<object[], object> CreateConstructorDelegate(Type type, ConstructorInfo ctor, ParameterInfo[] parameterInfos)
    {
      typeCache1[0] = typeof(object[]);

      DynamicMethod method = new DynamicMethod("_create2__" + type.FullName, typeof(object), typeCache1, GetOwnerType(type));
      ILGenerator ilGenerator = method.GetILGenerator();
      if (type.IsValueType)
      {
        ilGenerator.DeclareLocal(type);
        ilGenerator.Emit(OpCodes.Ldloca_S, 0);
      }

      if (parameterInfos != null && parameterInfos.Length > 0)
        for (int i = 0; i < parameterInfos.Length; i++)
        {
          ilGenerator.Emit(OpCodes.Ldarg_0);
          EmitLdc(ilGenerator, i);
          ilGenerator.Emit(OpCodes.Ldelem_Ref);

          if (parameterInfos[i].ParameterType.IsValueType)
            ilGenerator.Emit(OpCodes.Unbox_Any, parameterInfos[i].ParameterType);
          else
            ilGenerator.Emit(OpCodes.Castclass, parameterInfos[i].ParameterType);
        }

      if (type.IsValueType)
      {
        if (ctor == null)
          ilGenerator.Emit(OpCodes.Initobj, type);
        else
          ilGenerator.Emit(OpCodes.Call, ctor);

        ilGenerator.Emit(OpCodes.Ldloc_0);
        ilGenerator.Emit(OpCodes.Box, type);
      }
      else
        ilGenerator.Emit(OpCodes.Newobj, ctor);

      ilGenerator.Emit(OpCodes.Ret);
      return (Func<object[], object>)method.CreateDelegate(typeof(Func<object[], object>));
    }

    public static Func<object, object> CreateGetFieldDelegate(Type type, FieldInfo field)
    {
      typeCache1[0] = typeof(object);
      DynamicMethod method = new DynamicMethod("_get__" + type.FullName + "_" + field.Name, typeof(object), typeCache1, GetOwnerType(type));
      ILGenerator ilGenerator = method.GetILGenerator();

      ilGenerator.Emit(OpCodes.Ldarg_0);
      if (!type.IsValueType)
        ilGenerator.Emit(OpCodes.Castclass, type);
      else
        ilGenerator.Emit(OpCodes.Unbox, type);
      ilGenerator.Emit(OpCodes.Ldfld, field);

      if (field.FieldType.IsValueType)
        ilGenerator.Emit(OpCodes.Box, field.FieldType);

      ilGenerator.Emit(OpCodes.Ret);
      return (Func<object, object>)method.CreateDelegate(typeof(Func<object, object>));
    }

    public static Action<object, object> CreateSetFieldDelegate(Type type, FieldInfo field)
    {
      typeCache2[0] = typeof(object);
      typeCache2[1] = typeof(object);
      DynamicMethod method = new DynamicMethod("_set__" + type.FullName + "_" + field.Name, null, typeCache2, GetOwnerType(type));
      ILGenerator ilGenerator = method.GetILGenerator();

      ilGenerator.Emit(OpCodes.Ldarg_0);
      if (!type.IsValueType)
        ilGenerator.Emit(OpCodes.Castclass, type);
      else
        ilGenerator.Emit(OpCodes.Unbox, type);
      ilGenerator.Emit(OpCodes.Ldarg_1);

      if (field.FieldType.IsValueType)
        ilGenerator.Emit(OpCodes.Unbox_Any, field.FieldType);

      ilGenerator.Emit(OpCodes.Stfld, field);
      ilGenerator.Emit(OpCodes.Ret);
      return (Action<object, object>)method.CreateDelegate(typeof(Action<object, object>));
    }
  }
}
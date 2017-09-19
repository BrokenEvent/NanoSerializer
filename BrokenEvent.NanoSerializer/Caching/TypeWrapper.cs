using System;
using System.Collections.Generic;
using System.Reflection;

namespace BrokenEvent.NanoSerializer.Caching
{
  internal class TypeWrapper
  {
    public readonly Type Type;
    public readonly List<PropertyWrapper> Properties = new List<PropertyWrapper>();
    public readonly List<FieldWrapper> Fields = new List<FieldWrapper>();
    public readonly string[] ConstructorArgNames;
    public readonly int ConstructorArgsCount;
    private readonly Func<object[], object> createFunc;
    public readonly bool IsSelfSerializable;
    
    public TypeWrapper(Type type)
    {
      Type = type;
      int maxArgIndex = -1;

      UpdateFields(ref maxArgIndex);
      UpdateProperties(ref maxArgIndex);
      createFunc = UpdateConstructors(maxArgIndex, out ConstructorArgsCount, out ConstructorArgNames);

      IsSelfSerializable = SerializationBase.HaveInterface(type, typeof(INanoSerializable));
    }

    private void UpdateFields(ref int maxArgIndex)
    {
      foreach (FieldInfo info in Type.GetFields(BindingFlags.Instance | BindingFlags.Public))
      {
        NanoSerializationAttribute attr = info.GetCustomAttribute<NanoSerializationAttribute>();

        if (attr != null && attr.State == NanoState.Ignore)
          continue;

        NanoLocation location = NanoLocation.Auto;
        NanoState state = NanoState.Serialize;
        int constructorArg = -1;
        if (attr != null)
        {
          location = attr.Location;
          state = attr.State;
          constructorArg = attr.ConstructorArg;
          if (constructorArg > maxArgIndex)
            maxArgIndex = constructorArg;
        }

        Fields.Add(new FieldWrapper(Type, info, location, state, constructorArg));
      }
    }

    private void UpdateProperties(ref int maxArgIndex)
    {
      foreach (PropertyInfo info in Type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
      {
        if (!info.CanRead)
          continue;

        ParameterInfo[] indexParams = info.GetIndexParameters();

        // indexers are not supported
        if (indexParams.Length > 0)
          continue;

        NanoSerializationAttribute attr = info.GetCustomAttribute<NanoSerializationAttribute>();
        NanoLocation location = NanoLocation.Auto;
        NanoState state = NanoState.Serialize;
        int constructorArg = -1;

        if (attr != null)
        {
          if (attr.State == NanoState.Ignore)
            continue;

          location = attr.Location;
          constructorArg = attr.ConstructorArg;
          if (constructorArg > maxArgIndex)
            maxArgIndex = constructorArg;
          state = attr.State;
        }

        Properties.Add(new PropertyWrapper(Type, info, location, state, constructorArg));
      }
    }

    private static float TestConstructor(ConstructorInfo info, MemberWrapper[] args, ref string[] globals, ref ParameterInfo[] parameterInfos)
    {
      float result = 0;
      parameterInfos = info.GetParameters();

      // using hint
      NanoConstructorAttribute attr = info.GetCustomAttribute<NanoConstructorAttribute>();
      if (attr != null)
        result += 999;

      float okDelta = 1f / parameterInfos.Length;

      for (int i = 0; i < parameterInfos.Length; i++)
      {
        // arg from global list
        NanoArgAttribute argAttr = parameterInfos[i].GetCustomAttribute<NanoArgAttribute>();
        if (argAttr != null)
        {
          result += okDelta;
          if (globals == null)
            globals = new string[parameterInfos.Length];
          globals[i] = argAttr.ArgName;
          continue;
        }

        Type targetType = null;

        if (args != null && i < args.Length)
          targetType = args[i].MemberType;

        if (targetType == null)
        {
          // bad idea to set null to primitive
          if (SerializationBase.IsPrimitive(parameterInfos[i].ParameterType))
            result -= 1;
        }
        else
        {
          // compare types
          if (parameterInfos[i].ParameterType.IsAssignableFrom(targetType))
            result += okDelta;
        }
      }

      return result;
    }

    private Func<object[], object> UpdateConstructors(int maxArgIndex, out int argsCount, out string[] globalArgNames)
    {
      MemberWrapper[] args = null;
      if (maxArgIndex > -1)
      {
        args = new MemberWrapper[maxArgIndex + 1];
        foreach (PropertyWrapper wrapper in Properties)
          if (wrapper.ConstructorArg != -1)
            args[wrapper.ConstructorArg] = wrapper;
        foreach (FieldWrapper wrapper in Fields)
          if (wrapper.ConstructorArg != -1)
            args[wrapper.ConstructorArg] = wrapper;
      }

      globalArgNames = null;
      ConstructorInfo bestCtor = null;
      float bestCtorScore = -1;
      ParameterInfo[] bestCtorParameterInfos = null;

      foreach (ConstructorInfo info in Type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
      {
        ParameterInfo[] parameterInfos = null;
        string[] names = null;

        float score = TestConstructor(info, args, ref names, ref parameterInfos);
        if (score > bestCtorScore)
        {
          bestCtorScore = score;
          bestCtor = info;
          bestCtorParameterInfos = parameterInfos;
          globalArgNames = names;
        }
      }

      if (bestCtor == null)
        throw new SerializationException($"Unable to get best constructor for {Type.FullName}");

      argsCount = bestCtorParameterInfos.Length;
      return InvocationHelper.CreateConstructorDelegate(Type, bestCtor, bestCtorParameterInfos);
    }

    public object CreateObject(object[] args)
    {
      return createFunc(args);
    }
  }
}

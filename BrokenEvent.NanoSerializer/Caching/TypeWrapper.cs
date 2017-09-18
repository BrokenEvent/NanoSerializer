using System;
using System.Collections.Generic;
using System.Reflection;

namespace BrokenEvent.NanoSerializer.Caching
{
  internal class TypeWrapper
  {
    private readonly Type type;
    private readonly List<PropertyWrapper> properties = new List<PropertyWrapper>();
    private readonly List<FieldWrapper> fields = new List<FieldWrapper>();
    private readonly ConstructorInfo constructor;
    private readonly Dictionary<int, string> constructorArgNames;
    private readonly int constructorArgsCount;
    
    public TypeWrapper(Type type)
    {
      this.type = type;
      UpdateFields();

      int maxArgIndex = -1;
      UpdateProperties(ref maxArgIndex);
      constructor = UpdateConstructors(maxArgIndex, out constructorArgsCount, out constructorArgNames);
    }

    private void UpdateFields()
    {
      foreach (FieldInfo info in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
      {
        NanoSerializationAttribute attr = info.GetCustomAttribute<NanoSerializationAttribute>();

        if (attr != null && attr.State == NanoState.Ignore)
          continue;

        NanoLocation location = NanoLocation.Auto;
        NanoState state = NanoState.Serialize;
        if (attr != null)
        {
          location = attr.Location;
          state = attr.State;
        }
        fields.Add(new FieldWrapper(type, info, location, state));
      }
    }

    private void UpdateProperties(ref int maxArgIndex)
    {
      foreach (PropertyInfo info in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
      {
        if (!info.CanRead)
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

        properties.Add(new PropertyWrapper(type, info, location, state, constructorArg));
      }
    }

    private static float TestConstructor(ConstructorInfo info, PropertyWrapper[] args, Dictionary<int, string> globals, out int paramsCount)
    {
      float result = 0;
      ParameterInfo[] parameterInfos = info.GetParameters();
      paramsCount = parameterInfos.Length;

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
          globals.Add(i, argAttr.ArgName);
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

    private ConstructorInfo UpdateConstructors(int maxArgIndex, out int argsCount, out Dictionary<int, string> globalArgNames)
    {
      PropertyWrapper[] args = null;
      if (maxArgIndex > -1)
      {
        args = new PropertyWrapper[maxArgIndex + 1];
        foreach (PropertyWrapper wrapper in properties)
          if (wrapper.ConstructorArg != -1)
            args[wrapper.ConstructorArg] = wrapper;
      }

      globalArgNames = null;
      ConstructorInfo bestCtor = null;
      Dictionary<int, string> argNames = new Dictionary<int, string>();
      float bestCtorScore = -1;
      int bestCtorArgsCount = -1;

      foreach (ConstructorInfo info in type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
      {
        int count;
        if (argNames == null)
          argNames = new Dictionary<int, string>();
        else
          argNames.Clear();

        float score = TestConstructor(info, args, argNames, out count);
        if (score > bestCtorScore)
        {
          bestCtorScore = score;
          bestCtor = info;
          bestCtorArgsCount = count;
          globalArgNames = argNames;
          argNames = null;
        }
      }

      if (bestCtor == null)
        throw new SerializationException($"Unable to get best constructor for {type.FullName}");

      argsCount = bestCtorArgsCount;
      return bestCtor;
    }

    public IList<PropertyWrapper> Properties
    {
      get { return properties; }
    }

    public IList<FieldWrapper> Fields
    {
      get { return fields; }
    }

    public Dictionary<int, string> ConstructorArgNames
    {
      get { return constructorArgNames; }
    }

    public int ConstructorArgsCount
    {
      get { return constructorArgsCount; }
    }

    public object CreateObject(object[] args)
    {
      return constructor.Invoke(args);
    }
  }
}

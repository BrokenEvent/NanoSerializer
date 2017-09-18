using System;
using System.Reflection;

namespace BrokenEvent.NanoSerializer.Caching
{
  internal class PropertyWrapper: MemberWrapper
  {
    public readonly PropertyInfo Info;
    public readonly int ConstructorArg;
    private readonly Func<object, object> readFunc;
    private readonly Action<object, object> writeAction;

    public PropertyWrapper(Type ownerType, PropertyInfo info, NanoLocation location, NanoState state, int constructorArg)
      : base(ownerType, info, info.PropertyType, location, state)
    {
      Info = info;
      ConstructorArg = constructorArg;

      if (info.CanWrite)
        writeAction = InvocationHelper.CreateSetDelegate(ownerType, info.PropertyType, info.SetMethod);
      readFunc = InvocationHelper.CreateGetDelegate(ownerType, info.PropertyType, info.GetMethod);
    }

    public override object GetValue(object target)
    {
      return readFunc(target);
    }

    public override void SetValue(object target, object value)
    {
      writeAction(target, value);
    }
  }
}

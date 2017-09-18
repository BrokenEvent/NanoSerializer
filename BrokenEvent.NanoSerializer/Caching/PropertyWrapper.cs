using System;
using System.Reflection;

namespace BrokenEvent.NanoSerializer.Caching
{
  internal class PropertyWrapper: MemberWrapper
  {
    private readonly PropertyInfo info;
    private int constructorArg = -1;
    private readonly Func<object, object> readFunc;
    private readonly Action<object, object> writeAction;

    public PropertyWrapper(Type ownerType, PropertyInfo info, NanoLocation location)
      : base(ownerType, info, info.PropertyType, location)
    {
      this.info = info;

      if (info.CanWrite)
        writeAction = InvocationHelper.CreateSetDelegate(ownerType, info.PropertyType, info.SetMethod);
      readFunc = InvocationHelper.CreateGetDelegate(ownerType, info.PropertyType, info.GetMethod);
    }

    public PropertyInfo Info
    {
      get { return info; }
    }
    public int ConstructorArg
    {
      get { return constructorArg; }
      set { constructorArg = value; }
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

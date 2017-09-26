using System;
using System.Reflection;

namespace BrokenEvent.NanoSerializer.Caching
{
  internal class PropertyWrapper: MemberWrapper
  {
    public readonly PropertyInfo Info;
    private readonly Func<object, object> readFunc;
    private readonly Action<object, object> writeAction;
    public readonly bool CanWrite;
    public readonly bool CanRead;
    public readonly bool IsPrivate;

    public PropertyWrapper(Type ownerType, PropertyInfo info, NanoLocation location, NanoState state, int constructorArg, string name)
      : base(ownerType, info, info.PropertyType, location, state, constructorArg, name)
    {
      Info = info;

      if (info.CanWrite)
        writeAction = InvocationHelper.CreateSetDelegate(ownerType, info.PropertyType, info.SetMethod);
      readFunc = InvocationHelper.CreateGetDelegate(ownerType, info.PropertyType, info.GetMethod);

      CanWrite = info.CanWrite;
      CanRead = info.CanRead;

      IsPrivate = (!info.CanRead || info.GetMethod.IsPrivate) &&
                  (!info.CanWrite || info.SetMethod.IsPrivate);
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
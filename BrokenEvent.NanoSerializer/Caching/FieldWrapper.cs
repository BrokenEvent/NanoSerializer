using System;
using System.Reflection;

namespace BrokenEvent.NanoSerializer.Caching
{
  internal class FieldWrapper: MemberWrapper
  {
    public readonly FieldInfo Info;
    private readonly Func<object, object> readFunc;
    private readonly Action<object, object> writeFunc;

    public FieldWrapper(Type ownerType, FieldInfo info, NanoLocation location, NanoState state, int constructorArg, string name) :
      base(ownerType, info, info.FieldType, location, state, constructorArg, name)
    {
      Info = info;

      readFunc = InvocationHelper.CreateGetFieldDelegate(ownerType, info);
      writeFunc = InvocationHelper.CreateSetFieldDelegate(ownerType, info);
    }

    public override object GetValue(object target)
    {
      return readFunc(target);
    }

    public override void SetValue(object target, object value)
    {
      writeFunc(target, value);
    }
  }
}

using System;
using System.Reflection;

namespace BrokenEvent.NanoSerializer.Caching
{
  internal class FieldWrapper: MemberWrapper
  {
    public readonly FieldInfo Info;

    public FieldWrapper(Type ownerType, FieldInfo info, NanoLocation location, NanoState state) :
      base(ownerType, info, info.FieldType, location, state)
    {
      Info = info;
    }

    public override object GetValue(object target)
    {
      // TODO optz fields
      return Info.GetValue(target);
    }

    public override void SetValue(object target, object value)
    {
      // TODO optz fields
      Info.SetValue(target, value);
    }
  }
}

using System;
using System.Reflection;

namespace BrokenEvent.NanoSerializer.Caching
{
  internal class FieldWrapper: MemberWrapper
  {
    private readonly FieldInfo info;

    public FieldWrapper(Type ownerType, FieldInfo info, NanoLocation location):
      base(ownerType, info, info.FieldType, location)
    {
      this.info = info;
    }

    public FieldInfo Info
    {
      get { return info; }
    }

    public override object GetValue(object target)
    {
      // TODO optz fields
      return info.GetValue(target);
    }

    public override void SetValue(object target, object value)
    {
      // TODO optz fields
      info.SetValue(target, value);
    }
  }
}

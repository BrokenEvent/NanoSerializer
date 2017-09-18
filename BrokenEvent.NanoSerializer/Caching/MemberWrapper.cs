using System;
using System.Reflection;

namespace BrokenEvent.NanoSerializer.Caching
{
  internal abstract class MemberWrapper
  {
    private readonly Type ownerType;
    private readonly Type memberType;
    private readonly MemberInfo memberInfo;
    private readonly NanoLocation location;
    private readonly TypeCategory typeCategory;
    private readonly Type[] genericArguments;

    protected MemberWrapper(Type ownerType, MemberInfo memberInfo, Type memberType, NanoLocation location)
    {
      this.ownerType = ownerType;
      this.memberInfo = memberInfo;
      this.memberType = memberType;
      typeCategory = SerializationBase.GetTypeCategory(memberType);

      if (location == NanoLocation.Auto)
        this.location = typeCategory == TypeCategory.Primitive ? NanoLocation.Attribute : NanoLocation.SubNode;
      else
        this.location = location;

      if (memberType.IsGenericType)
        genericArguments = memberType.GetGenericArguments();
    }

    public Type OwnerType
    {
      get { return ownerType; }
    }

    public MemberInfo MemberInfo
    {
      get { return memberInfo; }
    }

    public NanoLocation Location
    {
      get { return location; }
    }

    public Type MemberType
    {
      get { return memberType; }
    }

    public TypeCategory TypeCategory
    {
      get { return typeCategory; }
    }

    public Type[] GenericArguments
    {
      get { return genericArguments; }
    }

    public abstract object GetValue(object target);

    public abstract void SetValue(object target, object value);
  }
}

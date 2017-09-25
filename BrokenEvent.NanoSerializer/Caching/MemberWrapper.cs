using System;
using System.Reflection;

namespace BrokenEvent.NanoSerializer.Caching
{
  internal abstract class MemberWrapper
  {
    public readonly Type OwnerType;
    public readonly Type MemberType;
    public readonly MemberInfo MemberInfo;
    public readonly NanoLocation Location;
    public readonly NanoState State;
    public readonly TypeCategory TypeCategory;
    public readonly Type[] GenericArguments;
    public readonly int ConstructorArg;
    public readonly string Name;

    protected MemberWrapper(Type ownerType, MemberInfo memberInfo, Type memberType, NanoLocation location, NanoState state, int constructorArg, string name)
    {
      OwnerType = ownerType;
      MemberInfo = memberInfo;
      MemberType = memberType;
      State = state;
      ConstructorArg = constructorArg;
      Name = name ?? memberInfo.Name;

      TypeCategory = SerializationBase.GetTypeCategory(memberType);

      if (location == NanoLocation.Auto)
        Location = TypeCategory == TypeCategory.Primitive || TypeCategory == TypeCategory.Enum ? NanoLocation.Attribute : NanoLocation.SubNode;
      else
        Location = location;

      GenericArguments = TypeCache.GetTypeGenericArgs(memberType);
    }

    public abstract object GetValue(object target);

    public abstract void SetValue(object target, object value);
  }
}

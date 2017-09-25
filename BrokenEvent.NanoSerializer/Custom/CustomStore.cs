using System;
using System.Collections.Generic;

namespace BrokenEvent.NanoSerializer.Custom
{
  /// <summary>
  /// Store of the custom serializers and attributes.
  /// </summary>
  /// <remarks>All custom serializers and attributes must be registered
  /// before the first serialization or deserialization of objects take place.</remarks>
  public static class CustomStore
  {
    private static Dictionary<Type, INanoSerializer> customSerializers = new Dictionary<Type, INanoSerializer>();
    private static Dictionary<Type, Dictionary<string, NanoSerializationAttribute>> customAttributes = new Dictionary<Type, Dictionary<string, NanoSerializationAttribute>>();

    /// <summary>
    /// Registers custom serializer for given type. This serializer will be used when serializing or deserializing all objects of this type.
    /// </summary>
    /// <param name="type">Type to register serializer for</param>
    /// <param name="serializer">The serializer which will provide serialization/deserialization logic for given type</param>
    public static void RegisterCustomSerializer(Type type, INanoSerializer serializer)
    {
      customSerializers[type] = serializer;
    }

    /// <summary>
    /// Registers custom serializer for given type. This serializer will be used when serializing or deserializing all objects of this type.
    /// </summary>
    /// <typeparam name="TTarget">Type to register serializer for</typeparam>
    /// <param name="serializer">The serializer which will provide serialization/deserialization logic for given type</param>
    public static void RegisterCustomSerializer<TTarget>(INanoSerializer serializer)
    {
      RegisterCustomSerializer(typeof(TTarget), serializer);
    }

    internal static INanoSerializer FindSerializer(Type type)
    {
      INanoSerializer serializer;
      return customSerializers.TryGetValue(type, out serializer) ? serializer : null;
    }

    /// <summary>
    /// Register custom attribute for type member.
    /// </summary>
    /// <param name="type">Type for what member to register</param>
    /// <param name="memberName">Name of memeber for which to register attribute. Using nameof is recommended</param>
    /// <param name="attribute">Attribute to register</param>
    /// <remarks>This attributes will override the attributes declared for members. Use these methods when you need to declare
    /// custom serialization settings for type while can't declare it in usual way.</remarks>
    public static void RegisterCustomAttribute(Type type, string memberName, NanoSerializationAttribute attribute)
    {
      Dictionary<string, NanoSerializationAttribute> dict;
      if (!customAttributes.TryGetValue(type, out dict))
      {
        dict = new Dictionary<string, NanoSerializationAttribute>();
        customAttributes.Add(type, dict);
      }

      dict[memberName] = attribute;
    }

    /// <summary>
    /// Register custom serialization settings for the type member.
    /// </summary>
    /// <param name="type">Type for what member to register</param>
    /// <param name="memberName">Name of memeber for which to register attribute. Using nameof is recommended</param>
    /// <param name="state">Serialization state of the member</param>
    /// <param name="constructorArg">Constructor argument index. -1 if this member shouldn't be used in constructor</param>
    /// <param name="location">Location of the member in serialized output</param>
    /// <param name="serializationName">Name of the member in serialized output</param>
    /// <remarks>This settings will override the attribtues declared for member. Use these methods when you need to declare
    /// custom serialization settings for type while can't declare it in usual way.</remarks>
    public static void RegisterCustomSettings(
        Type type,
        string memberName,
        NanoState state = NanoState.Serialize,
        int constructorArg = -1,
        NanoLocation location = NanoLocation.Auto,
        string serializationName = null
      )
    {
      RegisterCustomAttribute(
          type,
          memberName,
          new NanoSerializationAttribute { ConstructorArg = constructorArg, State = state, Name = serializationName, Location = location }
        );
    }

    internal static NanoSerializationAttribute FindAttribute(Type type, string memberName)
    {
      Dictionary<string, NanoSerializationAttribute> dict;
      if (!customAttributes.TryGetValue(type, out dict))
        return null;

      NanoSerializationAttribute attr;
      return dict.TryGetValue(memberName, out attr) ? attr : null;
    }
  }
}

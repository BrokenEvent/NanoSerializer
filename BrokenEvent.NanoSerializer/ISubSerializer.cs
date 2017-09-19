using System;

namespace BrokenEvent.NanoSerializer
{
  /// <summary>
  /// Used to continue serialization of subobjects of <see cref="INanoSerializable"/> object.
  /// </summary>
  public interface ISubSerializer
  {
    /// <summary>
    /// Serializes the object. Continues the serialization with current serializer.
    /// </summary>
    /// <param name="targetType">Base type of object to serialize. Actual <paramref name="value"/> can be a child class of this.</param>
    /// <param name="value">Object to serlialize</param>
    /// <param name="data">Data carrier to serialize to</param>
    /// <remarks>Target type is the type the deserializer should expect. Actual <paramref name="value"/> can be child class or
    /// something that can be assigned to <paramref name="targetType"/>.</remarks>
    void ContinueSerialization(Type targetType, object value, IDataAdapter data);
  }
}

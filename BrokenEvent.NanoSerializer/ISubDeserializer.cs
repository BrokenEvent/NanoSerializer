using System;

namespace BrokenEvent.NanoSerializer
{
  /// <summary>
  /// Used to continue deserialization of subobjects of <see cref="INanoSerializable"/> object.
  /// </summary>
  public interface ISubDeserializer
  {
    /// <summary>
    /// Deserializes the object. Continues deserialization using the current deserializer.
    /// </summary>
    /// <param name="targetType">Type to expect</param>
    /// <param name="data">Data carrier to deserialize from</param>
    /// <param name="value">Deserialization result to be filled with deserialized data. If null, the object will be created.</param>
    /// <remarks>Target type is the type to expect. Actuall deserialized object may be subclass or something assignable to
    /// <paramref name="targetType"/>.</remarks>
    void ContinueDeserialization(Type targetType, IDataAdapter data, ref object value);
  }
}

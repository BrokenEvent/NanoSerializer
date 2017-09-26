using BrokenEvent.NanoSerializer.Adapter;

namespace BrokenEvent.NanoSerializer
{
  /// <summary>
  /// Class that can serialize and deserialize by itself in its own way.
  /// </summary>
  public interface INanoSerializable
  {
    /// <summary>
    /// Performs serialization to the <paramref name="data"/>.
    /// </summary>
    /// <param name="data">Data carrier to serialize to</param>
    /// <param name="subSerializer">Serializer to continue serialization chain. May be used
    ///  to serialize inner objects.</param>
    void Serialize(IDataAdapter data, ISubSerializer subSerializer);

    /// <summary>
    /// Performs deserialization from the <paramref name="data"/>
    /// </summary>
    /// <param name="data">Data carrier to deserialize from</param>
    /// <param name="subDeserializer">Deserializer to continue deserialization chain. May be used
    /// to deserialize inner objects.</param>
    void Deserialize(IDataAdapter data, ISubDeserializer subDeserializer);
  }
}

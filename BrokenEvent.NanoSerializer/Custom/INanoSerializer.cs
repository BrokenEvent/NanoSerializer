namespace BrokenEvent.NanoSerializer.Custom
{
  /// <summary>
  /// Custom serialization provider for type.
  /// </summary>
  public interface INanoSerializer
  {
    /// <summary>
    /// Serialize the object.
    /// </summary>
    /// <param name="target">Target object to serialize</param>
    /// <param name="data">Data carrier to serialize to</param>
    /// <param name="subSerializer">Serializer to continue serialization chain. May be used
    ///  to serialize inner objects.</param>
    void SerializeObject(object target, IDataAdapter data, ISubSerializer subSerializer);

    /// <summary>
    /// Deserialize object.
    /// </summary>
    /// <param name="data">Data carrier to deserialize from</param>
    /// <param name="subDeserializer">Deserializer to continue deserialization chain. May be used
    /// to deserialize inner objects.</param>
    /// <returns>The deserialized object</returns>
    object DeserializeObject(IDataAdapter data, ISubDeserializer subDeserializer);
  }
}

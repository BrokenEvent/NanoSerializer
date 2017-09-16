using System;

namespace BrokenEvent.NanoSerializer
{
  /// <summary>
  /// Thrown when serializer or deserializer fails.
  /// </summary>
  public class SerializationException: Exception
  {
    /// <inheritdoc />
    public SerializationException(string message): base(message) { }
  }
}

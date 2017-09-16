namespace BrokenEvent.NanoSerializer
{
  /// <summary>
  /// State of the value.
  /// </summary>
  public enum NanoState
  {
    /// <summary>
    /// Value will be serialized
    /// </summary>
    Serialize,
    /// <summary>
    /// Value will be serialized and set, even if <see cref="NanoSerializationAttribute.ConstructorArg"/> is set.
    /// Works as <see cref="Serialize"/> if this field/property is not constructor arg.
    /// </summary>
    SerializeSet,
    /// <summary>
    /// Value will not be serialized
    /// </summary>
    Ignore,
  }
}
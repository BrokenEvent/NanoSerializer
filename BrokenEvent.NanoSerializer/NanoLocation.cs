namespace BrokenEvent.NanoSerializer
{
  /// <summary>
  /// Data location in XML.
  /// </summary>
  public enum NanoLocation
  {
    /// <summary>
    /// Auto selection, default value. Primitives will be stored as attributes, all other as subnodes
    /// </summary>
    Auto,
    /// <summary>
    /// Value will become attribute. Unavailable for non-primitive types.
    /// </summary>
    Attribute,
    /// <summary>
    /// Value will become subnode
    /// </summary>
    SubNode,
  }
}
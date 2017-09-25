using System;

namespace BrokenEvent.NanoSerializer
{
  /// <summary>
  /// The attribute controls the serialization behavior of the property.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
  public class NanoSerializationAttribute: Attribute
  {
    /// <summary>
    /// Serialization state for the property or field.
    /// </summary>
    public NanoState State { get; set; } = NanoState.Serialize;

    /// <summary>
    /// If the class have no parameterless constructor, this property value will be used as parameter. Index is 0-based.
    /// </summary>
    public int ConstructorArg { get; set; } = -1;

    /// <summary>
    /// Data location. <see cref="NanoLocation"/> is not available for complex types.
    /// </summary>
    public NanoLocation Location { get; set; } = NanoLocation.Auto;

    /// <summary>
    /// Custom name of the property or field.
    /// </summary>
    public string Name { get; set; }
  }
}

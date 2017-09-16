using System;

namespace BrokenEvent.NanoSerializer
{
  /// <summary>
  /// Used for precise constructor arguments configuration.
  /// </summary>
  [AttributeUsage(AttributeTargets.Parameter)]
  public class NanoArgAttribute: Attribute
  {
    /// <summary>
    /// Creates the instance of the attribute.
    /// </summary>
    /// <param name="argName">Key for object in <see cref="Deserializer.ConstructorArgs"/> to pass into constructor instead of deserialized value.</param>
    public NanoArgAttribute(string argName)
    {
      ArgName = argName;
    }

    /// <summary>
    /// Key for object in <see cref="Deserializer.ConstructorArgs"/> to pass into constructor instead of deserialized value.
    /// </summary>
    public string ArgName { get; set; }
  }
}

using System;

namespace BrokenEvent.NanoSerializer
{
  /// <summary>
  /// Hint for deserializer to use this constructor among the others.
  /// </summary>
  [AttributeUsage(AttributeTargets.Constructor)]
  public class NanoConstructorAttribute: Attribute
  {
  }
}

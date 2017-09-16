using System.Collections.Generic;

namespace BrokenEvent.NanoSerializer
{
  /// <summary>
  /// Data abstraction for <see cref="Serializer"/> and <see cref="Deserializer"/>.
  /// </summary>
  public interface IDataAdapter
  {
    /// <summary>
    /// Add attribute. Should add subelement if attributes are unavailable.
    /// </summary>
    /// <param name="name">Name of attribute</param>
    /// <param name="value">Value of attribute</param>
    /// <param name="isSystem">True if this is internal NanoSerializer attribute and false if regular data</param>
    void AddAttribute(string name, string value, bool isSystem);

    /// <summary>
    /// Gets or sets text value of this data element.
    /// </summary>
    string Value { get; set; }

    /// <summary>
    /// Gets the attribute value by name. Should return subelement if attributes are unavailable.
    /// </summary>
    /// <param name="name">Name of the attribute to get</param>
    /// <param name="isSystem">True if this is internal NanoSerializer attribute and false if regular data</param>
    /// <returns>Text value of the attribute or subelement. Null if no attribute with this name found</returns>
    string GetAttribute(string name, bool isSystem);

    /// <summary>
    /// Gets the child element by name.
    /// </summary>
    /// <param name="name">Name of subelement to get</param>
    /// <returns>The element itself or null if no element with given name found</returns>
    IDataAdapter GetChild(string name);

    /// <summary>
    /// Adds child element by name.
    /// </summary>
    /// <param name="name">Name of the element to add</param>
    /// <returns>The added subelement.</returns>
    IDataAdapter AddChild(string name);

    /// <summary>
    /// Gets the children elements enumeration in straight (as stored) order.
    /// </summary>
    /// <returns>Child elements enumeration</returns>
    IEnumerable<IDataAdapter> GetChildren();

    /// <summary>
    /// Gets the children elements enumeration in reversed (relative to stored order) order.
    /// </summary>
    /// <returns>Child elements reversed enumeration</returns>
    IEnumerable<IDataAdapter> GetChildrenReversed();
  }
}

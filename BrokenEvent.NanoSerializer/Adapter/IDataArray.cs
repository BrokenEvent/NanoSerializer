using System.Collections.Generic;

namespace BrokenEvent.NanoSerializer.Adapter
{
  /// <summary>
  /// The array/list of elements.
  /// </summary>
  public interface IDataArray
  {
    /// <summary>
    /// Adds an array value.
    /// </summary>
    /// <returns>Data adapter with child item or ready to create child item with required type</returns>
    /// <remarks>At this time we don't know what is the type of value to add. Untyped carriers (such as XML) may simply add a value
    /// to alter it later. Typed carriers (such as JSON) may not add a value right now, but the adapter returned should be ready to add it to current
    /// array as necessary.</remarks>
    IDataAdapter AddArrayValue();

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

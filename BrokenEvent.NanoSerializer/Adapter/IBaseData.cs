using System.Collections.Generic;

namespace BrokenEvent.NanoSerializer.Adapter
{
  public interface IBaseData
  {
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

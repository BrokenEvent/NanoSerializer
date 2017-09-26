using System.Collections.Generic;

namespace BrokenEvent.NanoSerializer.Adapter
{
  public interface IDataArray: IBaseData
  {
    IDataAdapter AddArrayValue();
  }
}

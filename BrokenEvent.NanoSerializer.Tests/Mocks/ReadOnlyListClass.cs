using System.Collections.Generic;

namespace BrokenEvent.NanoSerializer.Tests.Mocks
{
  internal class ReadOnlyListClass
  {
    public ReadOnlyListClass()
    {
      Strings = new List<string>();
    }

    public IList<string> Strings { get; }
  }
}
namespace BrokenEvent.NanoSerializer.Tests.Mocks
{
  internal class ThreeSubnodesTestClass
  {
    [NanoSerialization(Location = NanoLocation.SubNode)]
    public int A { get; set; }
    [NanoSerialization(Location = NanoLocation.SubNode, Name = "B_")]
    public string B { get; set; }
    [NanoSerialization(Location = NanoLocation.SubNode)]
    public NanoState C { get; set; }
    [NanoSerialization(State = NanoState.Ignore)]
    public int D { get; set; }
  }
}
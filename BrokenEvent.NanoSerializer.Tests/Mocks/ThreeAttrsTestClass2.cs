namespace BrokenEvent.NanoSerializer.Tests.Mocks
{
  internal class ThreeAttrsTestClass2
  {
    [NanoSerialization(Name = "A_")]
    public int A;
    public string B { get; set; }
    public NanoState C { get; set; }
  }
}
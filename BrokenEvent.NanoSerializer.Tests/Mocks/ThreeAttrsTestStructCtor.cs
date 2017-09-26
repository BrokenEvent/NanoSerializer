namespace BrokenEvent.NanoSerializer.Tests.Mocks
{
  internal struct ThreeAttrsTestStructCtor
  {
    public ThreeAttrsTestStructCtor(int a) : this()
    {
      A = a;
    }

    [NanoSerialization(ConstructorArg = 0)]
    public int A;

    public string B { get; set; }
    public NanoState C { get; set; }
  }
}
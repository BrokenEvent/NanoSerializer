namespace BrokenEvent.NanoSerializer.Tests.Mocks
{
  public class CustomConstructorTestClass
  {
    public CustomConstructorTestClass(string b, NanoState c, int a)
    {
      A = a;
      B = b;
      C = c;
    }

    [NanoSerialization(Location = NanoLocation.SubNode, ConstructorArg = 2)]
    public int A { get; private set; }
    [NanoSerialization(ConstructorArg = 0)]
    public string B { get; private set; }
    [NanoSerialization(ConstructorArg = 1)]
    public NanoState C { get; private set; }
  }
}

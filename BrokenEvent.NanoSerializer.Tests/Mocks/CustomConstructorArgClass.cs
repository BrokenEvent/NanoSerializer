namespace BrokenEvent.NanoSerializer.Tests.Mocks
{
  public class CustomConstructorArgClass
  {
    [NanoSerialization(ConstructorArg = 0)]
    public string A { get; private set; }

    [NanoSerialization(State = NanoState.Ignore)]
    public string B { get; private set; }

    public CustomConstructorArgClass() { }

    [NanoConstructor]
    public CustomConstructorArgClass(string a, [NanoArg("testArg")]string b)
    {
      A = a;
      B = b;
    }
  }
}
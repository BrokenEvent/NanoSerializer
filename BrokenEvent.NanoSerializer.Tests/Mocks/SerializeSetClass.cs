namespace BrokenEvent.NanoSerializer.Tests.Mocks
{
  internal class SerializeSetClass
  {
    private string a;
    private string b;
    [NanoSerialization(ConstructorArg = 1)]
    public readonly string c;

    public string A
    {
      get { return a; }
    }

    [NanoSerialization(ConstructorArg = 0, State = NanoState.SerializeSet)]
    public string B
    {
      get { return b; }
      set
      {
        b = value;
        a = value;
      }
    }

    public SerializeSetClass(string a, string c)
    {
      this.a = a;
      this.c = c;
    }
  }
}
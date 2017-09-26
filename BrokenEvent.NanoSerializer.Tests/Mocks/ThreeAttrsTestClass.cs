namespace BrokenEvent.NanoSerializer.Tests.Mocks
{
  internal class ThreeAttrsTestClass
  {
    [NanoSerialization(Name = "A_")]
    public int A;
    public string B { get; set; }
    public NanoState C { get; set; }

    public string D { get { return "!!!"; } }

    public static int E = 999;

    private string F { get; set; }

    public string GetPrivate()
    {
      return F;
    }

    public void SetPrivate(string value)
    {
      F = value;
    }
  }
}
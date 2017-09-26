namespace BrokenEvent.NanoSerializer.Tests.Mocks
{
  internal class CustomSerializationClass
  {
    public CustomSerializationClass(string a)
    {
      A = a;
    }
    public string A { get; }
    public NanoState B { get; set; }
  }
}
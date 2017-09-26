using BrokenEvent.NanoSerializer.Adapter;

namespace BrokenEvent.NanoSerializer.Tests.Mocks
{
  internal class NanoSerializableClass : INanoSerializable
  {
    public string A { get; set; }
    public object B { get; set; }

    void INanoSerializable.Serialize(IDataAdapter data, ISubSerializer subSerializer)
    {
      data.AddAttribute("1", A);
      subSerializer.ContinueSerialization(typeof(object), B, data.AddChild("1").AddChild("2"));
    }

    void INanoSerializable.Deserialize(IDataAdapter data, ISubDeserializer subDeserializer)
    {
      A = data.GetAttribute("1");

      object b = null;
      subDeserializer.ContinueDeserialization(typeof(object), data.GetChild("1").GetChild("2"), ref b);
      B = b;
    }
  }
}
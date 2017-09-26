using System;

using BrokenEvent.NanoSerializer.Adapter;
using BrokenEvent.NanoSerializer.Custom;

namespace BrokenEvent.NanoSerializer.Tests.Mocks
{
  internal class CustomSerializerClass : INanoSerializer
  {
    public void SerializeObject(object target, IDataAdapter data, ISubSerializer subSerializer)
    {
      CustomSerializationClass obj = (CustomSerializationClass)target;
      data.AddStringValue(obj.A, "someData", true);
      data.AddStringValue(obj.B.ToString(), "someOtherData", true);
    }

    public object DeserializeObject(IDataAdapter data, ISubDeserializer subDeserializer)
    {
      string a = data.GetStringValue("someData", true);
      CustomSerializationClass obj = new CustomSerializationClass(a);
      obj.B = (NanoState)Enum.Parse(typeof(NanoState), data.GetStringValue("someOtherData", true));
      return obj;
    }
  }
}
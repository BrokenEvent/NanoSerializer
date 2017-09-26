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
      data.AddAttribute("someData", obj.A);
      data.AddAttribute("someOtherData", obj.B.ToString());
    }

    public object DeserializeObject(IDataAdapter data, ISubDeserializer subDeserializer)
    {
      string a = data.GetAttribute("someData");
      CustomSerializationClass obj = new CustomSerializationClass(a);
      obj.B = (NanoState)Enum.Parse(typeof(NanoState), data.GetAttribute("someOtherData"));
      return obj;
    }
  }
}
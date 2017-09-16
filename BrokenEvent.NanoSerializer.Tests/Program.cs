using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace BrokenEvent.NanoSerializer.Tests
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    [XmlInclude(typeof(ModelSubclass))]
    static void Main()
    {
      Console.WriteLine("Warming up JIT: Model...");
      ModelClass target = ModelClass.BuildObjectsModel(5, 3);
      ModelClass result;

      StringBuilder stringBuilder;

      Console.WriteLine("Warming up JIT: XmlSerializer... (1)");
      XmlSerializer xmlSerializer = new XmlSerializer(typeof(ModelClass), new []{typeof(ModelSubclass)});
      stringBuilder = new StringBuilder();
      using (TextWriter writer = new StringWriter(stringBuilder))
        xmlSerializer.Serialize(writer, target);

      Console.WriteLine("Warming up JIT: XmlSerializer... (2)");
      using (TextReader reader = new StringReader(stringBuilder.ToString()))
        xmlSerializer.Deserialize(reader);

      using (MemoryStream ms = new MemoryStream())
      {
        Console.WriteLine("Warming up JIT: BinaryFormatter... (1)");
        new BinaryFormatter().Serialize(ms, target);
        ms.Position = 0;

        Console.WriteLine("Warming up JIT: BinaryFormatter...(2)");
        new BinaryFormatter().Deserialize(ms);
      }

      Console.WriteLine("Warming up JIT: JsonSerializer... (1)");
      stringBuilder = new StringBuilder();
      using (TextWriter writer = new StringWriter(stringBuilder))
        new JsonSerializer().Serialize(writer, target);

      Console.WriteLine("Warming up JIT: JsonSerializer... (2)");
      using (TextReader reader = new StringReader(stringBuilder.ToString()))
        new JsonSerializer().Deserialize(reader, typeof(ModelSubclass));

      Console.WriteLine("Warming up JIT: NanoSerializer...");
      XmlDocument xml = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)xml, target);
      stringBuilder = new StringBuilder();
      using (TextWriter writer = new StringWriter(stringBuilder))
        xml.WriteTo(new XmlTextWriter(writer));

      Console.WriteLine("Warming up JIT: NanoDeserializer...");
      xml = new XmlDocument();
      using (TextReader reader = new StringReader(stringBuilder.ToString()))
        xml.Load(reader);
      Deserializer.Deserialize<ModelSubclass>((SystemXmlAdapter)xml);

      ModelClass.ObjectsCount = 0;
      Console.Write("Building model...");
      target = ModelClass.BuildObjectsModel(13, 5);
      Console.WriteLine($"{ModelClass.ObjectsCount} objects built.");

      Console.WriteLine();
      Console.WriteLine("Serializing: XmlSerializer...");
      stringBuilder = new StringBuilder();
      Stopwatch stopwatch = Stopwatch.StartNew();
      using (TextWriter writer = new StringWriter(stringBuilder))
        xmlSerializer.Serialize(writer, target);
      stopwatch.Stop();
      Console.WriteLine($"Done. Elapsed: {stopwatch.ElapsedMilliseconds} ms. Length: {stringBuilder.Length}");

      Console.WriteLine();
      Console.WriteLine("Deserializing: XmlSerializer...");
      stopwatch.Restart();
      using (TextReader reader = new StringReader(stringBuilder.ToString()))
        result = (ModelClass)xmlSerializer.Deserialize(reader);
      stopwatch.Stop();
      Console.WriteLine($"Done. Elapsed: {stopwatch.ElapsedMilliseconds} ms. Comparison result: {ModelClass.CompareModel(target, result)}");

      using (MemoryStream ms = new MemoryStream())
      {
        Console.WriteLine();
        Console.WriteLine("Serializing: BinaryFormatter...");
        stopwatch.Restart();
        new BinaryFormatter().Serialize(ms, target);
        stopwatch.Stop();
        Console.WriteLine($"Done. Elapsed: {stopwatch.ElapsedMilliseconds} ms. Length: {ms.Length}");

        Console.WriteLine();
        Console.WriteLine("Deserializing: BinaryFormatter...");
        ms.Position = 0;
        stopwatch.Restart();
        result = (ModelClass)new BinaryFormatter().Deserialize(ms);
        stopwatch.Stop();
        Console.WriteLine($"Done. Elapsed: {stopwatch.ElapsedMilliseconds} ms. Comparison result: {ModelClass.CompareModel(target, result)}");
      }

      Console.WriteLine();
      Console.WriteLine("Serializing: JsonSerializer...");
      stringBuilder = new StringBuilder();
      stopwatch = Stopwatch.StartNew();
      using (TextWriter writer = new StringWriter(stringBuilder))
        new JsonSerializer().Serialize(writer, target);
      stopwatch.Stop();
      Console.WriteLine($"Done. Elapsed: {stopwatch.ElapsedMilliseconds} ms. Length: {stringBuilder.Length}");

      Console.WriteLine();
      Console.WriteLine("Deserializing: JsonSerializer...");
      stopwatch.Restart();
      using (TextReader reader = new StringReader(stringBuilder.ToString()))
        result = (ModelClass)new JsonSerializer().Deserialize(reader, typeof(ModelSubclass));
      stopwatch.Stop();
      Console.WriteLine($"Done. Elapsed: {stopwatch.ElapsedMilliseconds} ms. Comparison result: {ModelClass.CompareModel(target, result)}");

      Console.WriteLine();
      Console.WriteLine("Serializing: NanoSerializer...");
      xml = new XmlDocument();
      stopwatch.Restart();
      Serializer.Serialize((SystemXmlAdapter)xml, target);
      stringBuilder = new StringBuilder();
      using (TextWriter writer = new StringWriter(stringBuilder))
        xml.WriteTo(new XmlTextWriter(writer));
      stopwatch.Stop();
      Console.WriteLine($"Done. Elapsed: {stopwatch.ElapsedMilliseconds} ms. Length: {stringBuilder.Length}");

      Console.WriteLine();
      Console.WriteLine("Deserializing: NanoSerializer...");
      xml = new XmlDocument();
      stopwatch.Restart();
      using (TextReader reader = new StringReader(stringBuilder.ToString()))
        xml.Load(reader);
      result = Deserializer.Deserialize<ModelSubclass>((SystemXmlAdapter)xml);
      stopwatch.Stop();
      Console.WriteLine($"Done. Elapsed: {stopwatch.ElapsedMilliseconds} ms. Comparison result: {ModelClass.CompareModel(target, result)}");

      Console.WriteLine("Press <Enter> to exit.");
      Console.ReadLine();
    }
  }
}

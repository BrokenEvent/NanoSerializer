using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace BrokenEvent.NanoSerializer.Tests
{
  static class Program
  {
    #region Console Stuff

    [DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
    private static extern long StrFormatByteSize(long fileSize, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder buffer, int bufferSize);

    /// <summary>
    /// Converts a numeric value into a string that represents the number expressed as a size value in bytes,
    /// kilobytes, megabytes, or gigabytes, depending on the size. The WinAPI is used.
    /// </summary>
    /// <param name="filesize">The numeric value to be converted.</param>
    /// <remarks>http://stackoverflow.com/questions/281640/how-do-i-get-a-human-readable-file-size-in-bytes-abbreviation-using-net</remarks>
    /// <returns>the converted string</returns>
    public static string StrFormatByteSize(long filesize)
    {
      StringBuilder sb = new StringBuilder(11);
      StrFormatByteSize(filesize, sb, sb.Capacity);
      return sb.ToString();
    }

    private struct ConsolePos
    {
      private int x, y;

      public ConsolePos(bool unused)
      {
        x = Console.CursorLeft;
        y = Console.CursorTop;
      }

      public void Restore()
      {
        Console.SetCursorPosition(x, y);
      }
    }

    #endregion

    #region Console Helpers

    private struct BestValue
    {
      public ConsolePos Position;
      public long Value;
      public string Text;
    }

    private static ConsolePos backupPosition;
    private static BestValue bestTimeSerialization;
    private static BestValue bestTimeDeserialization;
    private static BestValue bestSizeSerialization;

    private static void UpdateBestValues(ref BestValue bestValue, long currentValue, string text)
    {
      if (bestValue.Value > currentValue || bestValue.Value == 0)
      {
        if (bestValue.Value != 0)
        {
          ConsolePos temp = new ConsolePos(true);
          bestValue.Position.Restore();
          Console.ForegroundColor = ConsoleColor.White;
          Console.Write(bestValue.Text);
          temp.Restore();
        }

        Console.ForegroundColor = ConsoleColor.Green;
        bestValue.Position = new ConsolePos(true);
        bestValue.Value = currentValue;
        bestValue.Text = text;
      }
      else
        Console.ForegroundColor = ConsoleColor.White;

      Console.Write(text);
      Console.ForegroundColor = ConsoleColor.Gray;
    }

    private static void WriteWarmUp(string name)
    {
      Console.Write("Warming up JIT: ");
      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine("{0}...", name);
      Console.ForegroundColor = ConsoleColor.Gray;
    }

    private static void WriteTestStart(string name)
    {
      Console.Write("{0,-34}: ", name);
      backupPosition = new ConsolePos(true);
      Console.ForegroundColor = ConsoleColor.DarkGray;
      Console.Write("testing...");
      Console.ForegroundColor = ConsoleColor.Gray;
    }

    private static void WriteTestFinish(Stopwatch stopwatch, long length)
    {
      backupPosition.Restore();

      UpdateBestValues(ref bestTimeSerialization, stopwatch.ElapsedMilliseconds, stopwatch.ElapsedMilliseconds + " ms");

      Console.Write(". Length: ");

      UpdateBestValues(ref bestSizeSerialization, length, StrFormatByteSize(length));
      Console.WriteLine();
    }

    private static void WriteTestFinish(Stopwatch stopwatch, bool result)
    {
      backupPosition.Restore();

      UpdateBestValues(ref bestTimeDeserialization, stopwatch.ElapsedMilliseconds, stopwatch.ElapsedMilliseconds + " ms");

      Console.Write(". Comparison: ");
      if (result)
      {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("OK");
      }
      else
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Failed");
      }
      Console.ForegroundColor = ConsoleColor.Gray;
    }

    #endregion

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      PerfTest.TestPerformance();
      Console.ReadLine();

      WriteWarmUp("Model");
      ModelClass target = ModelClass.BuildObjectsModel(5, 3);
      ModelClass result;

      StringBuilder stringBuilder;

      WriteWarmUp("XmlSerializer serialization");
      XmlSerializer xmlSerializer = new XmlSerializer(typeof(ModelClass), new []{typeof(ModelSubclass)});
      stringBuilder = new StringBuilder();
      using (TextWriter writer = new StringWriter(stringBuilder))
        xmlSerializer.Serialize(writer, target);

      WriteWarmUp("XmlSerializer deserialization");
      using (TextReader reader = new StringReader(stringBuilder.ToString()))
        xmlSerializer.Deserialize(reader);

      using (MemoryStream ms = new MemoryStream())
      {
        WriteWarmUp("BinaryFormatter serialization");
        new BinaryFormatter().Serialize(ms, target);
        ms.Position = 0;

        WriteWarmUp("BinaryFormatter deserialization");
        new BinaryFormatter().Deserialize(ms);
      }

      WriteWarmUp("JsonSerializer serialization");
      stringBuilder = new StringBuilder();
      using (TextWriter writer = new StringWriter(stringBuilder))
        new JsonSerializer().Serialize(writer, target);

      WriteWarmUp("JsonSerializer deserialization");
      using (TextReader reader = new StringReader(stringBuilder.ToString()))
        new JsonSerializer().Deserialize(reader, typeof(ModelSubclass));

      WriteWarmUp("NanoSerializer");
      XmlDocument xml = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)xml, target);
      stringBuilder = new StringBuilder();
      using (TextWriter writer = new StringWriter(stringBuilder))
        xml.WriteTo(new XmlTextWriter(writer));

      WriteWarmUp("NanoDeserializer");
      xml = new XmlDocument();
      using (TextReader reader = new StringReader(stringBuilder.ToString()))
        xml.Load(reader);
      Deserializer.Deserialize<ModelClass>((SystemXmlAdapter)xml);

      WriteWarmUp("Manual serialization");
      xml = new XmlDocument();
      xml.AppendChild(xml.CreateElement("root"));
      target.Serialize(xml.DocumentElement);

      WriteWarmUp("Manual deserialization");
      ModelClass.DeserializeCreate(xml.DocumentElement);

      ModelClass.ObjectsCount = 0;
      Console.WriteLine();
      Console.Write("Building model... ");
      target = ModelClass.BuildObjectsModel(13, 5);
      Console.ForegroundColor = ConsoleColor.White;
      Console.Write(ModelClass.ObjectsCount);
      Console.ForegroundColor = ConsoleColor.Gray;
      Console.WriteLine(" objects built.");
      Console.WriteLine();

      WriteTestStart("Serialization: XmlSerializer");
      stringBuilder = new StringBuilder();
      Stopwatch stopwatch = Stopwatch.StartNew();
      using (TextWriter writer = new StringWriter(stringBuilder))
        xmlSerializer.Serialize(writer, target);
      stopwatch.Stop();
      WriteTestFinish(stopwatch, stringBuilder.Length);

      WriteTestStart("Deserialization: XmlSerializer");
      stopwatch.Restart();
      using (TextReader reader = new StringReader(stringBuilder.ToString()))
        result = (ModelClass)xmlSerializer.Deserialize(reader);
      stopwatch.Stop();
      WriteTestFinish(stopwatch, ModelClass.CompareModel(target, result));

      using (MemoryStream ms = new MemoryStream())
      {
        WriteTestStart("Serialization: BinaryFormatter");
        stopwatch.Restart();
        new BinaryFormatter().Serialize(ms, target);
        stopwatch.Stop();
        WriteTestFinish(stopwatch, ms.Length);

        WriteTestStart("Deserialization: BinaryFormatter");
        ms.Position = 0;
        stopwatch.Restart();
        result = (ModelClass)new BinaryFormatter().Deserialize(ms);
        stopwatch.Stop();
        WriteTestFinish(stopwatch, ModelClass.CompareModel(target, result));
      }

      WriteTestStart("Serialization: JsonSerializer");
      stringBuilder = new StringBuilder();
      stopwatch = Stopwatch.StartNew();
      using (TextWriter writer = new StringWriter(stringBuilder))
        new JsonSerializer().Serialize(writer, target);
      stopwatch.Stop();
      WriteTestFinish(stopwatch, stringBuilder.Length);

      WriteTestStart("Deserialization: JsonSerializer");
      stopwatch.Restart();
      using (TextReader reader = new StringReader(stringBuilder.ToString()))
        result = (ModelClass)new JsonSerializer().Deserialize(reader, typeof(ModelSubclass));
      stopwatch.Stop();
      WriteTestFinish(stopwatch, ModelClass.CompareModel(target, result));

      WriteTestStart("Serialization: NanoSerializer");
      xml = new XmlDocument();
      stopwatch.Restart();
      Serializer.Serialize((SystemXmlAdapter)xml, target);
      stringBuilder = new StringBuilder();
      using (TextWriter writer = new StringWriter(stringBuilder))
        xml.WriteTo(new XmlTextWriter(writer));
      stopwatch.Stop();
      WriteTestFinish(stopwatch, stringBuilder.Length);

      WriteTestStart("Deserialization: NanoSerializer");
      xml = new XmlDocument();
      stopwatch.Restart();
      using (TextReader reader = new StringReader(stringBuilder.ToString()))
        xml.Load(reader);
      result = Deserializer.Deserialize<ModelClass>((SystemXmlAdapter)xml);
      stopwatch.Stop();
      WriteTestFinish(stopwatch, ModelClass.CompareModel(target, result));

      WriteTestStart("Serialization: Manual");
      xml = new XmlDocument();
      xml.AppendChild(xml.CreateElement("root"));
      stopwatch.Restart();
      target.Serialize(xml.DocumentElement);
      stringBuilder = new StringBuilder();
      using (TextWriter writer = new StringWriter(stringBuilder))
        xml.WriteTo(new XmlTextWriter(writer));
      stopwatch.Stop();
      WriteTestFinish(stopwatch, stringBuilder.Length);

      WriteTestStart("Deserialization: Manual");
      xml = new XmlDocument();
      stopwatch.Restart();
      using (TextReader reader = new StringReader(stringBuilder.ToString()))
        xml.Load(reader);
      result = ModelClass.DeserializeCreate(xml.DocumentElement);
      stopwatch.Stop();
      WriteTestFinish(stopwatch, ModelClass.CompareModel(target, result));

      Console.WriteLine();
      Console.WriteLine("Press <Enter> to exit.");
      Console.ReadLine();
    }
  }
}

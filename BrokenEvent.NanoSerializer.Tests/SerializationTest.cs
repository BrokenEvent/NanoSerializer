using System.Collections;
using System.Collections.Generic;
using System.Xml;

using NUnit.Framework;

namespace BrokenEvent.NanoSerializer.Tests
{
  [TestFixture]
  class SerializationTest
  {
    private static XmlElement GetElement(XmlElement el, string name)
    {
      foreach (XmlNode node in el.ChildNodes)
        if (node.NodeType == XmlNodeType.Element && node.Name == name)
          return (XmlElement)node;

      return null;
    }

    private static XmlElement GetElement(XmlDocument xml, string name)
    {
      return GetElement(xml.DocumentElement, name);
    }

    private static string GetXmlValue(XmlElement el, string name)
    {
      XmlElement result = GetElement(el, name);
      return result == null ? null : result.InnerText;
    }

    private static string GetXmlValue(XmlDocument xml, string name)
    {
      return GetXmlValue(xml.DocumentElement, name);
    }

    private class ThreeAttrsTestClass
    {
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

    [Test]
    public void ThreeAttrs()
    {
      ThreeAttrsTestClass a = new ThreeAttrsTestClass
      {
        A = 123,
        B = "testString",
        C = NanoState.Ignore
      };
      a.SetPrivate("private");

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(0, target.DocumentElement.ChildNodes.Count);
      Assert.AreEqual(4, target.DocumentElement.Attributes.Count);
      Assert.AreEqual("123", target.DocumentElement.GetAttribute("A"));
      Assert.AreEqual("testString", target.DocumentElement.GetAttribute("B"));
      Assert.AreEqual("Ignore", target.DocumentElement.GetAttribute("C"));

      ThreeAttrsTestClass b = Deserializer.Deserialize<ThreeAttrsTestClass>((SystemXmlAdapter)target);

      Assert.AreEqual(a.A, b.A);
      Assert.AreEqual(a.B, b.B);
      Assert.AreEqual(a.C, b.C);
      Assert.IsNull(b.GetPrivate());
    }

    [Test]
    public void ThreeAttrsPrivate()
    {
      ThreeAttrsTestClass a = new ThreeAttrsTestClass
      {
        A = 123,
        B = "testString",
        C = NanoState.Ignore
      };
      a.SetPrivate("private");

      XmlDocument target = new XmlDocument();
      SerializationSettings settings = new SerializationSettings();
      settings.SerializePrivateProperties = true;
      Serializer serializer = new Serializer(settings);
      serializer.SerializeObject((SystemXmlAdapter)target, a);

      Assert.AreEqual(0, target.DocumentElement.ChildNodes.Count);
      Assert.AreEqual(5, target.DocumentElement.Attributes.Count);
      Assert.AreEqual("123", target.DocumentElement.GetAttribute("A"));
      Assert.AreEqual("testString", target.DocumentElement.GetAttribute("B"));
      Assert.AreEqual("Ignore", target.DocumentElement.GetAttribute("C"));
      Assert.AreEqual("private", target.DocumentElement.GetAttribute("F"));

      ThreeAttrsTestClass b = Deserializer.Deserialize<ThreeAttrsTestClass>((SystemXmlAdapter)target);

      Assert.AreEqual(a.A, b.A);
      Assert.AreEqual(a.B, b.B);
      Assert.AreEqual(a.C, b.C);
      Assert.AreEqual(a.GetPrivate(), b.GetPrivate());
    }

    public struct ThreeAttrsTestStruct
    {
      public int A;
      public string B { get; set; }
      public NanoState C { get; set; }
    }

    [Test]
    public void ThreeAttrsStruct()
    {
      ThreeAttrsTestStruct a = new ThreeAttrsTestStruct
      {
        A = 123,
        B = "testString",
        C = NanoState.Ignore
      };

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(0, target.DocumentElement.ChildNodes.Count);
      Assert.AreEqual(4, target.DocumentElement.Attributes.Count);
      Assert.AreEqual("123", target.DocumentElement.GetAttribute("A"));
      Assert.AreEqual("testString", target.DocumentElement.GetAttribute("B"));
      Assert.AreEqual("Ignore", target.DocumentElement.GetAttribute("C"));

      ThreeAttrsTestStruct b = Deserializer.Deserialize<ThreeAttrsTestStruct>((SystemXmlAdapter)target);

      Assert.AreEqual(a.A, b.A);
      Assert.AreEqual(a.B, b.B);
      Assert.AreEqual(a.C, b.C);
    }

    private struct ThreeAttrsTestStructCtor
    {
      public ThreeAttrsTestStructCtor(int a): this()
      {
        A = a;
      }

      [NanoSerialization(ConstructorArg = 0)]
      public int A;

      public string B { get; set; }
      public NanoState C { get; set; }
    }

    [Test]
    public void ThreeAttrsStructCtor()
    {
      ThreeAttrsTestStructCtor a = new ThreeAttrsTestStructCtor(123)
      {
        B = "testString",
        C = NanoState.Ignore
      };

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(0, target.DocumentElement.ChildNodes.Count);
      Assert.AreEqual(4, target.DocumentElement.Attributes.Count);
      Assert.AreEqual("123", target.DocumentElement.GetAttribute("A"));
      Assert.AreEqual("testString", target.DocumentElement.GetAttribute("B"));
      Assert.AreEqual("Ignore", target.DocumentElement.GetAttribute("C"));

      ThreeAttrsTestStructCtor b = Deserializer.Deserialize<ThreeAttrsTestStructCtor>((SystemXmlAdapter)target);

      Assert.AreEqual(a.A, b.A);
      Assert.AreEqual(a.B, b.B);
      Assert.AreEqual(a.C, b.C);
    }

    private class ThreeSubnodesTestClass
    {
      [NanoSerialization(Location = NanoLocation.SubNode)]
      public int A { get; set; }
      [NanoSerialization(Location = NanoLocation.SubNode)]
      public string B { get; set; }
      [NanoSerialization(Location = NanoLocation.SubNode)]
      public NanoState C { get; set; }
      [NanoSerialization(State = NanoState.Ignore)]
      public int D { get; set; }
    }

    [Test]
    public void ThreeSubnodes()
    {
      ThreeSubnodesTestClass a = new ThreeSubnodesTestClass
      {
        A = 123,
        B = "testString",
        C = NanoState.Ignore
      };

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(3, target.DocumentElement.ChildNodes.Count);
      Assert.AreEqual("123", GetXmlValue(target, "A"));
      Assert.AreEqual("testString", GetXmlValue(target, "B"));
      Assert.AreEqual("Ignore", GetXmlValue(target, "C"));

      ThreeSubnodesTestClass b = Deserializer.Deserialize<ThreeSubnodesTestClass>((SystemXmlAdapter)target);

      Assert.AreEqual(a.A, b.A);
      Assert.AreEqual(a.B, b.B);
      Assert.AreEqual(a.C, b.C);
    }

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

    [Test]
    public void CustomConstructor()
    {
      CustomConstructorTestClass a = new CustomConstructorTestClass("ololo", NanoState.Serialize, 666);

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(1, target.DocumentElement.ChildNodes.Count);
      Assert.AreEqual("666", GetXmlValue(target, "A"));
      Assert.AreEqual("ololo", target.DocumentElement.GetAttribute("B"));
      Assert.AreEqual("Serialize", target.DocumentElement.GetAttribute("C"));

      CustomConstructorTestClass b = Deserializer.Deserialize<CustomConstructorTestClass>((SystemXmlAdapter)target);

      Assert.AreEqual(a.A, b.A);
      Assert.AreEqual(a.B, b.B);
      Assert.AreEqual(a.C, b.C);
    }

    [Test]
    public void CustomConstructorWithNull()
    {
      CustomConstructorTestClass a = new CustomConstructorTestClass(null, NanoState.Serialize, 666);

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(1, target.DocumentElement.ChildNodes.Count);
      Assert.AreEqual("666", GetXmlValue(target, "A"));
      Assert.AreEqual("Serialize", target.DocumentElement.GetAttribute("C"));

      CustomConstructorTestClass b = Deserializer.Deserialize<CustomConstructorTestClass>((SystemXmlAdapter)target);

      Assert.AreEqual(a.A, b.A);
      Assert.AreEqual(a.B, b.B);
      Assert.AreEqual(a.C, b.C);
    }

    private class ComplexTestClass: ThreeAttrsTestClass
    {
      public CustomConstructorTestClass F { get; set; }
    }

    [Test]
    public void ComplexClass()
    {
      ComplexTestClass a = new ComplexTestClass
      {
        A = 123,
        B = "testString",
        C = NanoState.Ignore,
        F = new CustomConstructorTestClass("testString2", NanoState.Serialize, 333)
      };

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(1, target.DocumentElement.ChildNodes.Count);
      Assert.AreEqual("123", target.DocumentElement.GetAttribute("A"));
      Assert.AreEqual("testString", target.DocumentElement.GetAttribute("B"));
      Assert.AreEqual("Ignore", target.DocumentElement.GetAttribute("C"));

      XmlElement fEl = GetElement(target, "F");
      Assert.NotNull(fEl);
      Assert.AreEqual(1, fEl.ChildNodes.Count);
      Assert.AreEqual(2, fEl.Attributes.Count);
      Assert.AreEqual("333", GetXmlValue(fEl, "A"));
      Assert.AreEqual("testString2", fEl.GetAttribute("B"));
      Assert.AreEqual("Serialize", fEl.GetAttribute("C"));

      ComplexTestClass b = Deserializer.Deserialize<ComplexTestClass>((SystemXmlAdapter)target);

      Assert.AreEqual(a.A, b.A);
      Assert.AreEqual(a.B, b.B);
      Assert.AreEqual(a.C, b.C);
      Assert.AreEqual(a.F.C, b.F.C);
      Assert.AreEqual(a.F.C, b.F.C);
      Assert.AreEqual(a.F.C, b.F.C);
    }

    [Test]
    public void ComplexClassNull()
    {
      ComplexTestClass a = new ComplexTestClass
      {
        A = 123,
        B = "testString",
        C = NanoState.Ignore,
      };

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(0, target.DocumentElement.ChildNodes.Count);
      Assert.AreEqual("123", target.DocumentElement.GetAttribute("A"));
      Assert.AreEqual("testString", target.DocumentElement.GetAttribute("B"));
      Assert.AreEqual("Ignore", target.DocumentElement.GetAttribute("C"));

      Assert.IsNull(GetElement(target, "D"));

      ComplexTestClass b = Deserializer.Deserialize<ComplexTestClass>((SystemXmlAdapter)target);

      Assert.AreEqual(a.A, b.A);
      Assert.AreEqual(a.B, b.B);
      Assert.AreEqual(a.C, b.C);
      Assert.AreEqual(a.D, b.D);
    }

    public class PolymorphismTestClass
    {
      public object A { get; set; }
      public object B { get; set; }
    }

    [Test]
    public void PolymorphismString()
    {
      PolymorphismTestClass a = new PolymorphismTestClass
      {
        A = "test",
      };

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(1, target.DocumentElement.ChildNodes.Count);

      XmlElement aEl = GetElement(target, "A");
      Assert.NotNull(aEl);

      Assert.AreEqual(1, aEl.Attributes.Count);
      Assert.AreEqual(1, aEl.ChildNodes.Count);
      Assert.AreEqual("System.String", aEl.GetAttribute(SerializationBase.ATTRIBUTE_TYPE));
      Assert.AreEqual("test", aEl.InnerText);

      PolymorphismTestClass b = Deserializer.Deserialize<PolymorphismTestClass>((SystemXmlAdapter)target);

      Assert.AreEqual(a.A, b.A);
    }

    [Test]
    public void PolymorphismObject()
    {
      PolymorphismTestClass a = new PolymorphismTestClass
      {
        A = new CustomConstructorTestClass("testString", NanoState.Serialize, 333),
      };

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(1, target.DocumentElement.ChildNodes.Count);

      XmlElement aEl = GetElement(target, "A");
      Assert.NotNull(aEl);

      Assert.AreEqual(1, aEl.ChildNodes.Count);
      Assert.AreEqual(3, aEl.Attributes.Count);
      Assert.AreEqual("333", GetXmlValue(aEl, "A"));
      Assert.AreEqual("testString", aEl.GetAttribute("B"));
      Assert.AreEqual("Serialize", aEl.GetAttribute("C"));

      PolymorphismTestClass b = Deserializer.Deserialize<PolymorphismTestClass>((SystemXmlAdapter)target);

      Assert.AreEqual(((CustomConstructorTestClass)a.A).A, ((CustomConstructorTestClass)b.A).A);
      Assert.AreEqual(((CustomConstructorTestClass)a.A).B, ((CustomConstructorTestClass)b.A).B);
      Assert.AreEqual(((CustomConstructorTestClass)a.A).C, ((CustomConstructorTestClass)b.A).C);
    }

    [Test]
    public void PolymorphismStruct()
    {
      PolymorphismTestClass a = new PolymorphismTestClass
      {
        A = new ThreeAttrsTestStruct { A = 1, B = "test", C = NanoState.SerializeSet }
      };

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(1, target.DocumentElement.ChildNodes.Count);

      XmlElement aEl = GetElement(target, "A");
      Assert.NotNull(aEl);

      Assert.AreEqual(0, aEl.ChildNodes.Count);
      Assert.AreEqual(4, aEl.Attributes.Count);
      Assert.AreEqual("1", aEl.GetAttribute("A"));
      Assert.AreEqual("test", aEl.GetAttribute("B"));
      Assert.AreEqual("SerializeSet", aEl.GetAttribute("C"));

      PolymorphismTestClass b = Deserializer.Deserialize<PolymorphismTestClass>((SystemXmlAdapter)target);

      Assert.AreEqual(((ThreeAttrsTestStruct)a.A).A, ((ThreeAttrsTestStruct)b.A).A);
      Assert.AreEqual(((ThreeAttrsTestStruct)a.A).B, ((ThreeAttrsTestStruct)b.A).B);
      Assert.AreEqual(((ThreeAttrsTestStruct)a.A).C, ((ThreeAttrsTestStruct)b.A).C);
    }

    [Test]
    public void CrossReference()
    {
      CustomConstructorTestClass obj = new CustomConstructorTestClass("testString", NanoState.Serialize, 333);
      PolymorphismTestClass a = new PolymorphismTestClass
      {
        A = obj,
        B = obj,
      };

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(2, target.DocumentElement.ChildNodes.Count);

      XmlElement aEl = GetElement(target, "A");
      Assert.NotNull(aEl);

      Assert.AreEqual(1, aEl.ChildNodes.Count);
      Assert.AreEqual(3, aEl.Attributes.Count);
      Assert.AreEqual("333", GetXmlValue(aEl, "A"));
      Assert.AreEqual("testString", aEl.GetAttribute("B"));
      Assert.AreEqual("Serialize", aEl.GetAttribute("C"));

      XmlElement bEl = GetElement(target, "B");
      Assert.NotNull(bEl);
      Assert.AreEqual(0, bEl.ChildNodes.Count);
      Assert.AreEqual(1, bEl.Attributes.Count);
      Assert.AreEqual("1", bEl.GetAttribute(SerializationBase.ATTRIBUTE_OBJID));

      PolymorphismTestClass b = Deserializer.Deserialize<PolymorphismTestClass>((SystemXmlAdapter)target);

      Assert.AreEqual(((CustomConstructorTestClass)a.A).A, ((CustomConstructorTestClass)b.A).A);
      Assert.AreEqual(((CustomConstructorTestClass)a.A).B, ((CustomConstructorTestClass)b.A).B);
      Assert.AreEqual(((CustomConstructorTestClass)a.A).C, ((CustomConstructorTestClass)b.A).C);
      Assert.AreEqual(b.A, b.B);
    }

    [Test]
    public void SelfReference()
    {
      PolymorphismTestClass a = new PolymorphismTestClass();
      a.A = a;

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(1, target.DocumentElement.ChildNodes.Count);

      XmlElement aEl = GetElement(target, "A");
      Assert.NotNull(aEl);

      Assert.AreEqual(0, aEl.ChildNodes.Count);
      Assert.AreEqual(1, aEl.Attributes.Count);
      Assert.AreEqual("0", aEl.GetAttribute(SerializationBase.ATTRIBUTE_OBJID));

      PolymorphismTestClass b = Deserializer.Deserialize<PolymorphismTestClass>((SystemXmlAdapter)target);

      Assert.AreEqual(b.A, b);
    }

    private class ArrayClass
    {
      public byte[] Bytes;
    }

    [Test]
    public void Array()
    {
      ArrayClass a = new ArrayClass()
      {
        Bytes = new byte[] { 1, 2 },
      };

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(1, target.DocumentElement.ChildNodes.Count);

      ArrayClass b = Deserializer.Deserialize<ArrayClass>((SystemXmlAdapter)target);

      Assert.AreEqual(a.Bytes.Length, b.Bytes.Length);
      for (int i = 0; i < a.Bytes.Length; i++)
        Assert.AreEqual(a.Bytes[i], b.Bytes[i]);
    }

    [Test]
    public void ArrayNull()
    {
      ArrayClass a = new ArrayClass();

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(0, target.DocumentElement.ChildNodes.Count);

      ArrayClass b = Deserializer.Deserialize<ArrayClass>((SystemXmlAdapter)target);

      Assert.IsNull(b.Bytes);
    }

    private class IListClass
    {
      public IList<string> Strings;
    }

    [Test]
    public void List()
    {
      IListClass a = new IListClass()
      {
        Strings = new List<string> { "test1", "test2" },
      };

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(1, target.DocumentElement.ChildNodes.Count);

      IListClass b = Deserializer.Deserialize<IListClass>((SystemXmlAdapter)target);

      Assert.AreEqual(a.Strings.Count, b.Strings.Count);
      for (int i = 0; i < a.Strings.Count; i++)
        Assert.AreEqual(a.Strings[i], b.Strings[i]);
    }

    private class ReadOnlyListClass
    {
      public ReadOnlyListClass()
      {
        Strings = new List<string>();
      }

      public IList<string> Strings { get; }
    }

    [Test]
    public void ReadOnlyList()
    {
      ReadOnlyListClass a = new ReadOnlyListClass();
      a.Strings.Add("test1");
      a.Strings.Add("test2");

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(1, target.DocumentElement.ChildNodes.Count);

      ReadOnlyListClass b = Deserializer.Deserialize<ReadOnlyListClass>((SystemXmlAdapter)target);

      Assert.AreEqual(a.Strings.Count, b.Strings.Count);
      for (int i = 0; i < a.Strings.Count; i++)
        Assert.AreEqual(a.Strings[i], b.Strings[i]);
    }

    private class SquareArrayClass
    {
      public string[,] Bytes;
    }

    [Test]
    public void SquareArray()
    {
      SquareArrayClass a = new SquareArrayClass()
      {
        Bytes = new string[,] { {"1", "2"}, {"3", "4"} },
      };

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(1, target.DocumentElement.ChildNodes.Count);

      SquareArrayClass b = Deserializer.Deserialize<SquareArrayClass>((SystemXmlAdapter)target);

      Assert.AreEqual(a.Bytes.GetLength(0), b.Bytes.GetLength(0));
      for (int x = 0; x < a.Bytes.GetLength(0); x++)
        for (int y = 0; y < a.Bytes.GetLength(1); y++)
          Assert.AreEqual(a.Bytes[x, y], b.Bytes[x, y]);
    }

    private class TriangleArrayClass
    {
      public string[][] Bytes;
    }

    [Test]
    public void TriangleArray()
    {
      TriangleArrayClass a = new TriangleArrayClass()
      {
        Bytes = new string[2][]
      };

      a.Bytes[0] = new string[] { "1", "2" };
      a.Bytes[1] = new string[] { "1", "2", "3" };

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(1, target.DocumentElement.ChildNodes.Count);

      TriangleArrayClass b = Deserializer.Deserialize<TriangleArrayClass>((SystemXmlAdapter)target);

      Assert.AreEqual(a.Bytes.GetLength(0), b.Bytes.GetLength(0));
      for (int x = 0; x < a.Bytes.Length; x++)
        for (int y = 0; y < a.Bytes[x].Length; y++)
          Assert.AreEqual(a.Bytes[x][y], b.Bytes[x][y]);
    }
 
    private class QueueClass
    {
      public Queue<string> Strings;
    }

    [Test]
    public void Queue()
    {
      Queue<string> q = new Queue<string>();
      q.Enqueue("test1");
      q.Enqueue("test2");
      QueueClass a = new QueueClass()
      {
        Strings = q,
      };

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(1, target.DocumentElement.ChildNodes.Count);

      QueueClass b = Deserializer.Deserialize<QueueClass>((SystemXmlAdapter)target);

      while (true)
      {
        bool emptyA = a.Strings.Count == 0;
        bool emptyB = b.Strings.Count == 0;

        Assert.AreEqual(emptyA, emptyB);
        if (emptyA)
          break;

        Assert.AreEqual(a.Strings.Dequeue(), b.Strings.Dequeue());
      }
    }

    private class NonGenericQueueClass
    {
      public Queue Strings;
    }

    [Test]
    public void NonGenericQueue()
    {
      Queue q = new Queue();
      q.Enqueue("test1");
      q.Enqueue("test2");
      NonGenericQueueClass a = new NonGenericQueueClass()
      {
        Strings = q,
      };

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(1, target.DocumentElement.ChildNodes.Count);

      NonGenericQueueClass b = Deserializer.Deserialize<NonGenericQueueClass>((SystemXmlAdapter)target);

      while (true)
      {
        bool emptyA = a.Strings.Count == 0;
        bool emptyB = b.Strings.Count == 0;

        Assert.AreEqual(emptyA, emptyB);
        if (emptyA)
          break;

        Assert.AreEqual(a.Strings.Dequeue(), b.Strings.Dequeue());
      }
    }

    private class StackClass
    {
      public Stack<string> Strings;
    }

    [Test]
    public void Stack()
    {
      Stack<string> q = new Stack<string>();
      q.Push("test1");
      q.Push("test2");
      StackClass a = new StackClass()
      {
        Strings = q,
      };

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(1, target.DocumentElement.ChildNodes.Count);

      StackClass b = Deserializer.Deserialize<StackClass>((SystemXmlAdapter)target);

      while (true)
      {
        bool emptyA = a.Strings.Count == 0;
        bool emptyB = b.Strings.Count == 0;

        Assert.AreEqual(emptyA, emptyB);
        if (emptyA)
          break;

        Assert.AreEqual(a.Strings.Pop(), b.Strings.Pop());
      }
    }

    private class NonGenericStackClass
    {
      public Stack Strings;
    }

    [Test]
    public void NonGenericStack()
    {
      Stack q = new Stack();
      q.Push("test1");
      q.Push("test2");
      NonGenericStackClass a = new NonGenericStackClass()
      {
        Strings = q,
      };

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(1, target.DocumentElement.ChildNodes.Count);

      NonGenericStackClass b = Deserializer.Deserialize<NonGenericStackClass>((SystemXmlAdapter)target);

      while (true)
      {
        bool emptyA = a.Strings.Count == 0;
        bool emptyB = b.Strings.Count == 0;

        Assert.AreEqual(emptyA, emptyB);
        if (emptyA)
          break;

        Assert.AreEqual(a.Strings.Pop(), b.Strings.Pop());
      }
    }

    private class SetClass
    {
      public ISet<string> Strings;
    }

    [Test]
    public void Set()
    { 
      SetClass a = new SetClass
      {
        Strings = new HashSet<string>{"test1", "test2"},
      };

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(1, target.DocumentElement.ChildNodes.Count);

      SetClass b = Deserializer.Deserialize<SetClass>((SystemXmlAdapter)target);

      Assert.IsTrue(a.Strings.SetEquals(b.Strings));
    }

    private class LinkedListClass
    {
      public LinkedList<string> Strings;
    }

    [Test]
    public void LinkedList()
    {
      LinkedList<string> strings = new LinkedList<string>();
      strings.AddLast("test1");
      strings.AddLast("test2");
      LinkedListClass a = new LinkedListClass
      {
        Strings = strings,
      };

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(1, target.DocumentElement.ChildNodes.Count);

      LinkedListClass b = Deserializer.Deserialize<LinkedListClass>((SystemXmlAdapter)target);

      IEnumerator<string> aEnum = a.Strings.GetEnumerator();
      IEnumerator<string> bEnum = b.Strings.GetEnumerator();

      while (true)
      {
        bool aDone = !aEnum.MoveNext();
        bool bDone = !bEnum.MoveNext();
        if (aDone && bDone)
          break;
        Assert.AreEqual(aDone, bDone);
        Assert.AreEqual(aEnum.Current, bEnum.Current);
      }

      aEnum.Dispose();
      bEnum.Dispose();
    }

    private class DictionaryClass
    {
      public Dictionary<string, string> Strings;
    }

    [Test]
    public void Dictionary()
    {
      DictionaryClass a = new DictionaryClass
      {
        Strings = new Dictionary<string, string>
        {
          { "1", "test1"},
          { "2", "test2"},
        }
      };

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(1, target.DocumentElement.ChildNodes.Count);

      DictionaryClass b = Deserializer.Deserialize<DictionaryClass>((SystemXmlAdapter)target);

      Assert.AreEqual(a.Strings.Count, b.Strings.Count);
      foreach (KeyValuePair<string, string> pair in a.Strings)
      {
        Assert.AreEqual(pair.Value, b.Strings[pair.Key]);
      }
    }

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

    [Test]
    public void CustomConstructorArg()
    {
      CustomConstructorArgClass a = new CustomConstructorArgClass("test1", "test2");

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(0, target.DocumentElement.ChildNodes.Count);
      Assert.AreEqual("test1", target.DocumentElement.GetAttribute("A"));
      Assert.IsEmpty(target.DocumentElement.GetAttribute("B"));

      Deserializer deserializer = new Deserializer();
      deserializer.ConstructorArgs.Add("testArg", "test2");
      CustomConstructorArgClass b = deserializer.DeserializeObject<CustomConstructorArgClass>((SystemXmlAdapter)target);

      Assert.AreEqual(a.A, b.A);
      Assert.AreEqual(a.B, b.B);
    }

    [Test]
    public void CustomConstructorNoArg()
    {
      CustomConstructorArgClass a = new CustomConstructorArgClass("test1", "test2");

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(0, target.DocumentElement.ChildNodes.Count);
      Assert.AreEqual("test1", target.DocumentElement.GetAttribute("A"));
      Assert.IsEmpty(target.DocumentElement.GetAttribute("B"));

      CustomConstructorArgClass b = Deserializer.Deserialize<CustomConstructorArgClass>((SystemXmlAdapter)target);

      Assert.AreEqual(a.A, b.A);
      Assert.IsNull(b.B);
    }

    private class SerializeSetClass
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

    [Test]
    public void SerializeSet()
    {
      SerializeSetClass a = new SerializeSetClass("test__", "test1");
      a.B = "test2";

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(0, target.DocumentElement.ChildNodes.Count);
      Assert.IsEmpty(target.DocumentElement.GetAttribute("A"));
      Assert.AreEqual("test2", target.DocumentElement.GetAttribute("B"));
      Assert.AreEqual("test1", target.DocumentElement.GetAttribute("c"));

      SerializeSetClass b = Deserializer.Deserialize<SerializeSetClass>((SystemXmlAdapter)target);

      Assert.AreEqual(a.A, b.A);
      Assert.AreEqual(a.B, b.B);
      Assert.AreEqual(a.c, b.c);
    }

    private class NanoSerializableClass: INanoSerializable
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

    [Test]
    public void NanoSerializable()
    {
      NanoSerializableClass a = new NanoSerializableClass
      {
        A = "test",
        B = new ThreeAttrsTestClass { A = 3, B = "test2", C = NanoState.Ignore }
      };

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      NanoSerializableClass b = Deserializer.Deserialize<NanoSerializableClass>((SystemXmlAdapter)target);

      Assert.AreEqual(a.A, b.A);
      Assert.AreEqual(((ThreeAttrsTestClass)a.B).A, ((ThreeAttrsTestClass)b.B).A);
      Assert.AreEqual(((ThreeAttrsTestClass)a.B).B, ((ThreeAttrsTestClass)b.B).B);
      Assert.AreEqual(((ThreeAttrsTestClass)a.B).C, ((ThreeAttrsTestClass)b.B).C);
    }

    private class PrimitiveArrayClass
    {
      public int[] Ints;
      public float[] Floats;
      public int[,] Ints2;
    }

    [Test]
    public void PrimitiveArray()
    {
      PrimitiveArrayClass a = new PrimitiveArrayClass
      {
        Ints = new[] { 1, 2, 3, 4, 5, 6 },
        Floats = new[] { 1.5f, 2.5f, 3.5f },
        Ints2 = new[,] { { 1, 2 }, { 3, 4 } }
      };

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(3, target.DocumentElement.ChildNodes.Count);

      Assert.AreEqual(1, GetElement(target, "Ints").ChildNodes.Count);
      Assert.AreEqual(1, GetElement(target, "Floats").ChildNodes.Count);

      PrimitiveArrayClass b = Deserializer.Deserialize<PrimitiveArrayClass>((SystemXmlAdapter)target);

      for (int i = 0; i < a.Ints.Length; i++)
        Assert.AreEqual(a.Ints[i], b.Ints[i]);
      for (int i = 0; i < a.Floats.Length; i++)
        Assert.AreEqual(a.Floats[i], b.Floats[i]);
      for (int x = 0; x < a.Ints2.GetLength(0); x++)
        for (int y = 0; y < a.Ints2.GetLength(1); y++)
          Assert.AreEqual(a.Ints2[x, y], b.Ints2[x, y]);
    }

    private class PrimitiveListClass
    {
      public List<int> Ints { get; } = new List<int>();
    }

    [Test]
    public void PrimitiveList()
    {
      PrimitiveListClass a = new PrimitiveListClass();
      a.Ints.Add(1);
      a.Ints.Add(3);
      a.Ints.Add(4);
      a.Ints.Add(5);
      a.Ints.Add(6);

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(1, target.DocumentElement.ChildNodes.Count);
      Assert.AreEqual(1, GetElement(target, "Ints").ChildNodes.Count);

      PrimitiveListClass b = Deserializer.Deserialize<PrimitiveListClass>((SystemXmlAdapter)target);

      for (int i = 0; i < a.Ints.Count; i++)
        Assert.AreEqual(a.Ints[i], b.Ints[i]);
    }
  }
}
using System.Collections;
using System.Collections.Generic;
using System.Xml;

using BrokenEvent.NanoSerializer.Custom;
using BrokenEvent.NanoSerializer.Tests.Mocks;

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
      Assert.AreEqual("123", target.DocumentElement.GetAttribute("A_"));
      Assert.AreEqual("testString", target.DocumentElement.GetAttribute("B"));
      Assert.AreEqual("Ignore", target.DocumentElement.GetAttribute("C"));

      ThreeAttrsTestClass b = Deserializer.Deserialize<ThreeAttrsTestClass>((SystemXmlAdapter)target);

      Assert.AreEqual(a.A, b.A);
      Assert.AreEqual(a.B, b.B);
      Assert.AreEqual(a.C, b.C);
      Assert.IsNull(b.GetPrivate());

      ThreeAttrsTestClass c = new ThreeAttrsTestClass();
      new Deserializer().FillObject(c, (SystemXmlAdapter)target);

      Assert.AreEqual(a.A, c.A);
      Assert.AreEqual(a.B, c.B);
      Assert.AreEqual(a.C, c.C);
      Assert.IsNull(c.GetPrivate());
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
      Assert.AreEqual("123", target.DocumentElement.GetAttribute("A_"));
      Assert.AreEqual("testString", target.DocumentElement.GetAttribute("B"));
      Assert.AreEqual("Ignore", target.DocumentElement.GetAttribute("C"));
      Assert.AreEqual("private", target.DocumentElement.GetAttribute("F"));

      ThreeAttrsTestClass b = Deserializer.Deserialize<ThreeAttrsTestClass>((SystemXmlAdapter)target);

      Assert.AreEqual(a.A, b.A);
      Assert.AreEqual(a.B, b.B);
      Assert.AreEqual(a.C, b.C);
      Assert.AreEqual(a.GetPrivate(), b.GetPrivate());

      ThreeAttrsTestClass c = new ThreeAttrsTestClass();
      new Deserializer().FillObject(c, (SystemXmlAdapter)target);

      Assert.AreEqual(a.A, c.A);
      Assert.AreEqual(a.B, c.B);
      Assert.AreEqual(a.C, c.C);
      Assert.AreEqual(a.GetPrivate(), c.GetPrivate());
    }

    [Test]
    public void EnumAsValue()
    {
      ThreeAttrsTestClass a = new ThreeAttrsTestClass
      {
        A = 123,
        B = "testString",
        C = NanoState.Ignore
      };

      XmlDocument target = new XmlDocument();
      SerializationSettings settings = new SerializationSettings();
      settings.EnumsAsValue = true;
      new Serializer(settings).SerializeObject((SystemXmlAdapter)target, a);

      Assert.AreEqual(0, target.DocumentElement.ChildNodes.Count);
      Assert.AreEqual(4, target.DocumentElement.Attributes.Count);
      Assert.AreEqual("123", target.DocumentElement.GetAttribute("A_"));
      Assert.AreEqual("testString", target.DocumentElement.GetAttribute("B"));
      Assert.AreEqual("2", target.DocumentElement.GetAttribute("C"));

      ThreeAttrsTestClass b = Deserializer.Deserialize<ThreeAttrsTestClass>((SystemXmlAdapter)target);

      Assert.AreEqual(a.A, b.A);
      Assert.AreEqual(a.B, b.B);
      Assert.AreEqual(a.C, b.C);
      Assert.IsNull(b.GetPrivate());

      ThreeAttrsTestClass c = new ThreeAttrsTestClass();
      new Deserializer().FillObject(c, (SystemXmlAdapter)target);

      Assert.AreEqual(a.A, c.A);
      Assert.AreEqual(a.B, c.B);
      Assert.AreEqual(a.C, c.C);
      Assert.IsNull(c.GetPrivate());
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
      Assert.AreEqual("testString", GetXmlValue(target, "B_"));
      Assert.AreEqual("Ignore", GetXmlValue(target, "C"));

      ThreeSubnodesTestClass b = Deserializer.Deserialize<ThreeSubnodesTestClass>((SystemXmlAdapter)target);

      Assert.AreEqual(a.A, b.A);
      Assert.AreEqual(a.B, b.B);
      Assert.AreEqual(a.C, b.C);

      ThreeSubnodesTestClass c = new ThreeSubnodesTestClass();
      new Deserializer().FillObject(c, (SystemXmlAdapter)target);

      Assert.AreEqual(a.A, c.A);
      Assert.AreEqual(a.B, c.B);
      Assert.AreEqual(a.C, c.C);
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
      Assert.AreEqual("123", target.DocumentElement.GetAttribute("A_"));
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

      ComplexTestClass c = new ComplexTestClass();
      new Deserializer().FillObject(c, (SystemXmlAdapter)target);

      Assert.AreEqual(a.A, c.A);
      Assert.AreEqual(a.B, c.B);
      Assert.AreEqual(a.C, c.C);
      Assert.AreEqual(a.F.C, c.F.C);
      Assert.AreEqual(a.F.C, c.F.C);
      Assert.AreEqual(a.F.C, c.F.C);
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
      Assert.AreEqual("123", target.DocumentElement.GetAttribute("A_"));
      Assert.AreEqual("testString", target.DocumentElement.GetAttribute("B"));
      Assert.AreEqual("Ignore", target.DocumentElement.GetAttribute("C"));

      Assert.IsNull(GetElement(target, "D"));

      ComplexTestClass b = Deserializer.Deserialize<ComplexTestClass>((SystemXmlAdapter)target);

      Assert.AreEqual(a.A, b.A);
      Assert.AreEqual(a.B, b.B);
      Assert.AreEqual(a.C, b.C);
      Assert.AreEqual(a.D, b.D);
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
      Assert.AreEqual("2", bEl.GetAttribute(SerializationBase.ATTRIBUTE_OBJID));

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
      Assert.AreEqual("1", aEl.GetAttribute(SerializationBase.ATTRIBUTE_OBJID));

      PolymorphismTestClass b = Deserializer.Deserialize<PolymorphismTestClass>((SystemXmlAdapter)target);

      Assert.AreEqual(b.A, b);
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

      PrimitiveListClass c = new PrimitiveListClass();
      new Deserializer().FillObject(c, (SystemXmlAdapter)target);

      for (int i = 0; i < a.Ints.Count; i++)
        Assert.AreEqual(a.Ints[i], c.Ints[i]);
    }

    [Test]
    public void CustomSerialization()
    {
      CustomSerializationClass a = new CustomSerializationClass("1111");
      a.B = NanoState.SerializeSet;
      CustomStore.RegisterCustomSerializer<CustomSerializationClass>(new CustomSerializerClass());

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.IsEmpty(target.DocumentElement.GetAttribute("a"));
      Assert.IsEmpty(target.DocumentElement.GetAttribute("b"));

      CustomSerializationClass b = Deserializer.Deserialize<CustomSerializationClass>((SystemXmlAdapter)target);

      Assert.AreEqual(a.A, b.A);
      Assert.AreEqual(a.B, b.B);
    }

    [Test]
    public void CustomSettings()
    {
      ThreeAttrsTestClass2 a = new ThreeAttrsTestClass2
      {
        A = 123,
        B = "testString",
        C = NanoState.Ignore
      };

      CustomStore.RegisterCustomSettings(typeof(ThreeAttrsTestClass2), nameof(ThreeAttrsTestClass2.A), serializationName: "name", location: NanoLocation.SubNode);
      CustomStore.RegisterCustomSettings(typeof(ThreeAttrsTestClass2), nameof(ThreeAttrsTestClass2.B), NanoState.Ignore);
      CustomStore.RegisterCustomSettings(typeof(ThreeAttrsTestClass2), nameof(ThreeAttrsTestClass2.C), serializationName: "value", location: NanoLocation.SubNode);

      XmlDocument target = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)target, a);

      Assert.AreEqual(2, target.DocumentElement.ChildNodes.Count);
      Assert.AreEqual(1, target.DocumentElement.Attributes.Count);
      Assert.AreEqual("123", GetXmlValue(target, "name"));
      Assert.IsNull(GetXmlValue(target, "B"));
      Assert.AreEqual("Ignore", GetXmlValue(target, "value"));

      ThreeAttrsTestClass2 b = Deserializer.Deserialize<ThreeAttrsTestClass2>((SystemXmlAdapter)target);

      Assert.AreEqual(a.A, b.A);
      Assert.IsNull(b.B);
      Assert.AreEqual(a.C, b.C);

      ThreeAttrsTestClass2 c = new ThreeAttrsTestClass2();
      new Deserializer().FillObject(c, (SystemXmlAdapter)target);

      Assert.AreEqual(a.A, c.A);
      Assert.IsNull(c.B);
      Assert.AreEqual(a.C, c.C);
    }
  }
}
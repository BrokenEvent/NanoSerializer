using System.Collections;
using System.Collections.Generic;

using BrokenEvent.NanoSerializer.Tests.Mocks;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

namespace BrokenEvent.NanoSerializer.Tests
{
  [TestFixture]
  class JsonTest
  {
    private static void AssertChildren(int expectedCount, JObject obj)
    {
      Assert.AreEqual(obj.Count, obj.Count);
    }

    private static void AssertChild(string expected, string name, JObject obj)
    {
      JToken token = obj[name];
      Assert.NotNull(token);

      Assert.AreEqual(expected, token.Value<string>());
    }

    private static void AssertAttribute(string expected, string name, JObject obj)
    {
      JToken token = obj["@" + name];
      Assert.NotNull(token);

      Assert.AreEqual(expected, token.Value<string>());
    }

    private static void AssertNoAttribute(string name, JObject obj)
    {
      JToken token = obj["@" + name];
      Assert.IsNull(token);
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

      JObject target = new JObject();
      Serializer.Serialize(new NewtonsoftJsonAdapter(target), a);

      AssertChildren(4, target);
      AssertAttribute("123", "A_", target);
      AssertAttribute("testString", "B", target);
      AssertAttribute("Ignore", "C", target);

      ThreeAttrsTestClass b = Deserializer.Deserialize<ThreeAttrsTestClass>((NewtonsoftJsonAdapter)target);

      Assert.AreEqual(a.A, b.A);
      Assert.AreEqual(a.B, b.B);
      Assert.AreEqual(a.C, b.C);
      Assert.IsNull(b.GetPrivate());

      ThreeAttrsTestClass c = new ThreeAttrsTestClass();
      new Deserializer().FillObject(c, (NewtonsoftJsonAdapter)target);

      Assert.AreEqual(a.A, c.A);
      Assert.AreEqual(a.B, c.B);
      Assert.AreEqual(a.C, c.C);
      Assert.IsNull(c.GetPrivate());
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

      JObject target = new JObject();
      Serializer.Serialize((NewtonsoftJsonAdapter)target, a);

      AssertChildren(4, target);
      AssertChild("123", "A", target);
      AssertChild("testString", "B_", target);
      AssertChild("Ignore", "C", target);

      ThreeSubnodesTestClass b = Deserializer.Deserialize<ThreeSubnodesTestClass>((NewtonsoftJsonAdapter)target);

      Assert.AreEqual(a.A, b.A);
      Assert.AreEqual(a.B, b.B);
      Assert.AreEqual(a.C, b.C);

      ThreeSubnodesTestClass c = new ThreeSubnodesTestClass();
      new Deserializer().FillObject(c, (NewtonsoftJsonAdapter)target);

      Assert.AreEqual(a.A, c.A);
      Assert.AreEqual(a.B, c.B);
      Assert.AreEqual(a.C, c.C);
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

      JObject target = new JObject();
      Serializer.Serialize((NewtonsoftJsonAdapter)target, a);

      AssertChildren(5, target);
      AssertAttribute("123", "A_", target);
      AssertAttribute("testString", "B", target);
      AssertAttribute("Ignore", "C", target);

      JObject fEl = (JObject)target["F"];
      Assert.NotNull(fEl);
      AssertChildren(3, fEl);
      AssertChild("333", "A", fEl);
      AssertAttribute("testString2", "B", fEl);
      AssertAttribute("Serialize", "C", fEl);

      ComplexTestClass b = Deserializer.Deserialize<ComplexTestClass>((NewtonsoftJsonAdapter)target);

      Assert.AreEqual(a.A, b.A);
      Assert.AreEqual(a.B, b.B);
      Assert.AreEqual(a.C, b.C);
      Assert.AreEqual(a.F.C, b.F.C);
      Assert.AreEqual(a.F.C, b.F.C);
      Assert.AreEqual(a.F.C, b.F.C);

      ComplexTestClass c = new ComplexTestClass();
      new Deserializer().FillObject(c, (NewtonsoftJsonAdapter)target);

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

      JObject target = new JObject();
      Serializer.Serialize((NewtonsoftJsonAdapter)target, a);

      AssertChildren(4, target);
      AssertAttribute("123", "A_", target);
      AssertAttribute("testString", "B", target);
      AssertAttribute("Ignore", "C", target);

      Assert.IsNull((JObject)target["F"]);

      ComplexTestClass b = Deserializer.Deserialize<ComplexTestClass>((NewtonsoftJsonAdapter)target);

      Assert.AreEqual(a.A, b.A);
      Assert.AreEqual(a.B, b.B);
      Assert.AreEqual(a.C, b.C);
      Assert.AreEqual(a.D, b.D);
    }

    [Test]
    public void ComplexClassNullValue()
    {
      ComplexTestClass a = new ComplexTestClass
      {
        A = 123,
        B = "testString",
        C = NanoState.Ignore,
      };

      JObject target = new JObject();
      SerializationSettings settings = new SerializationSettings();
      settings.SerializeNull = true;
      new Serializer(settings).SerializeObject((NewtonsoftJsonAdapter)target, a);

      AssertChildren(4, target);
      AssertAttribute("123", "A_", target);
      AssertAttribute("testString", "B", target);
      AssertAttribute("Ignore", "C", target);

      Assert.AreEqual(JTokenType.Null, target["F"].Type);

      ComplexTestClass b = Deserializer.Deserialize<ComplexTestClass>((NewtonsoftJsonAdapter)target);

      Assert.AreEqual(a.A, b.A);
      Assert.AreEqual(a.B, b.B);
      Assert.AreEqual(a.C, b.C);
      Assert.AreEqual(a.D, b.D);
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

      JObject target = new JObject();
      Serializer.Serialize((NewtonsoftJsonAdapter)target, a);

      AssertChildren(3, target);

      JObject aEl = (JObject)target["A"];
      Assert.NotNull(aEl);

      AssertChildren(4, aEl);
      AssertChild("333", "A", aEl);
      AssertAttribute("testString", "B", aEl);
      AssertAttribute("Serialize", "C", aEl);

      JObject bEl = (JObject)target["B"];
      Assert.NotNull(bEl);

      AssertChildren(1, aEl);
      AssertAttribute("1", SerializationBase.ATTRIBUTE_OBJID, bEl);

      PolymorphismTestClass b = Deserializer.Deserialize<PolymorphismTestClass>((NewtonsoftJsonAdapter)target);

      Assert.AreEqual(((CustomConstructorTestClass)a.A).A, ((CustomConstructorTestClass)b.A).A);
      Assert.AreEqual(((CustomConstructorTestClass)a.A).B, ((CustomConstructorTestClass)b.A).B);
      Assert.AreEqual(((CustomConstructorTestClass)a.A).C, ((CustomConstructorTestClass)b.A).C);
      Assert.AreEqual(b.A, b.B);
    }

    [Test]
    public void Array()
    {
      ArrayClass a = new ArrayClass
      {
        Bytes = new byte[] { 1, 2 },
      };

      JObject target = new JObject();
      Serializer.Serialize((NewtonsoftJsonAdapter)target, a);

      AssertChildren(2, target);

      ArrayClass b = Deserializer.Deserialize<ArrayClass>((NewtonsoftJsonAdapter)target);

      Assert.AreEqual(a.Bytes.Length, b.Bytes.Length);
      for (int i = 0; i < a.Bytes.Length; i++)
        Assert.AreEqual(a.Bytes[i], b.Bytes[i]);
    }

    [Test]
    public void StringArray()
    {
      StringArrayClass a = new StringArrayClass
      {
        Strings = new string[] { "test1", "test2" },
      };

      JObject target = new JObject();
      Serializer.Serialize((NewtonsoftJsonAdapter)target, a);

      AssertChildren(3, target);

      StringArrayClass b = Deserializer.Deserialize<StringArrayClass>((NewtonsoftJsonAdapter)target);

      Assert.AreEqual(a.Strings.Length, b.Strings.Length);
      for (int i = 0; i < a.Strings.Length; i++)
        Assert.AreEqual(a.Strings[i], b.Strings[i]);
    }

    [Test]
    public void ArrayNull()
    {
      ArrayClass a = new ArrayClass();

      JObject target = new JObject();
      Serializer.Serialize((NewtonsoftJsonAdapter)target, a);
      AssertChildren(1, target);

      ArrayClass b = Deserializer.Deserialize<ArrayClass>((NewtonsoftJsonAdapter)target);

      Assert.IsNull(b.Bytes);
    }

    [Test]
    public void ArrayNullValue()
    {
      ArrayClass a = new ArrayClass();

      JObject target = new JObject();
      SerializationSettings settings = new SerializationSettings();
      settings.SerializeNull = true;
      new Serializer(settings).SerializeObject((NewtonsoftJsonAdapter)target, a);

      AssertChildren(2, target);
      Assert.AreEqual(JTokenType.Null, target["Bytes"].Type);

      ArrayClass b = Deserializer.Deserialize<ArrayClass>((NewtonsoftJsonAdapter)target);

      Assert.IsNull(b.Bytes);
    }

    [Test]
    public void List()
    {
      IListClass a = new IListClass()
      {
        Strings = new List<string> { "test1", "test2" },
      };

      JObject target = new JObject();
      Serializer.Serialize((NewtonsoftJsonAdapter)target, a);

      AssertChildren(2, target);

      IListClass b = Deserializer.Deserialize<IListClass>((NewtonsoftJsonAdapter)target);

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

      JObject target = new JObject();
      Serializer.Serialize((NewtonsoftJsonAdapter)target, a);

      AssertChildren(3, target);

      ReadOnlyListClass b = Deserializer.Deserialize<ReadOnlyListClass>((NewtonsoftJsonAdapter)target);

      Assert.AreEqual(a.Strings.Count, b.Strings.Count);
      for (int i = 0; i < a.Strings.Count; i++)
        Assert.AreEqual(a.Strings[i], b.Strings[i]);
    }

    [Test]
    public void SquareArray()
    {
      SquareArrayClass a = new SquareArrayClass
      {
        Bytes = new string[,] { {"1", "2"}, {"3", "4"} },
      };

      JObject target = new JObject();
      Serializer.Serialize((NewtonsoftJsonAdapter)target, a);

      AssertChildren(3, target);

      SquareArrayClass b = Deserializer.Deserialize<SquareArrayClass>((NewtonsoftJsonAdapter)target);

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

      JObject target = new JObject();
      Serializer.Serialize((NewtonsoftJsonAdapter)target, a);

      AssertChildren(3, target);

      TriangleArrayClass b = Deserializer.Deserialize<TriangleArrayClass>((NewtonsoftJsonAdapter)target);

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

      JObject target = new JObject();
      Serializer.Serialize((NewtonsoftJsonAdapter)target, a);

      AssertChildren(3, target);

      QueueClass b = Deserializer.Deserialize<QueueClass>((NewtonsoftJsonAdapter)target);

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

      JObject target = new JObject();
      Serializer.Serialize((NewtonsoftJsonAdapter)target, a);

      AssertChildren(3, target);

      NonGenericQueueClass b = Deserializer.Deserialize<NonGenericQueueClass>((NewtonsoftJsonAdapter)target);

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

      JObject target = new JObject();
      Serializer.Serialize((NewtonsoftJsonAdapter)target, a);

      AssertChildren(3, target);

      StackClass b = Deserializer.Deserialize<StackClass>((NewtonsoftJsonAdapter)target);

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

      JObject target = new JObject();
      Serializer.Serialize((NewtonsoftJsonAdapter)target, a);

      AssertChildren(3, target);

      NonGenericStackClass b = Deserializer.Deserialize<NonGenericStackClass>((NewtonsoftJsonAdapter)target);

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

      JObject target = new JObject();
      Serializer.Serialize((NewtonsoftJsonAdapter)target, a);

      AssertChildren(3, target);

      SetClass b = Deserializer.Deserialize<SetClass>((NewtonsoftJsonAdapter)target);

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

      JObject target = new JObject();
      Serializer.Serialize((NewtonsoftJsonAdapter)target, a);

      AssertChildren(3, target);

      LinkedListClass b = Deserializer.Deserialize<LinkedListClass>((NewtonsoftJsonAdapter)target);

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

      JObject target = new JObject();
      Serializer.Serialize((NewtonsoftJsonAdapter)target, a);

      AssertChildren(3, target);

      DictionaryClass b = Deserializer.Deserialize<DictionaryClass>((NewtonsoftJsonAdapter)target);

      Assert.AreEqual(a.Strings.Count, b.Strings.Count);
      foreach (KeyValuePair<string, string> pair in a.Strings)
        Assert.AreEqual(pair.Value, b.Strings[pair.Key]);
    }

    [Test]
    public void CustomConstructorArg()
    {
      CustomConstructorArgClass a = new CustomConstructorArgClass("test1", "test2");

      JObject target = new JObject();
      Serializer.Serialize((NewtonsoftJsonAdapter)target, a);

      AssertChildren(3, target);
      AssertAttribute("test1", "A", target);
      AssertNoAttribute("B", target);

      Deserializer deserializer = new Deserializer();
      deserializer.ConstructorArgs.Add("testArg", "test2");
      CustomConstructorArgClass b = deserializer.DeserializeObject<CustomConstructorArgClass>((NewtonsoftJsonAdapter)target);

      Assert.AreEqual(a.A, b.A);
      Assert.AreEqual(a.B, b.B);
    }

    [Test]
    public void CustomConstructorNoArg()
    {
      CustomConstructorArgClass a = new CustomConstructorArgClass("test1", "test2");

      JObject target = new JObject();
      Serializer.Serialize((NewtonsoftJsonAdapter)target, a);

      AssertChildren(3, target);
      AssertAttribute("test1", "A", target);
      AssertNoAttribute("B", target);

      CustomConstructorArgClass b = Deserializer.Deserialize<CustomConstructorArgClass>((NewtonsoftJsonAdapter)target);

      Assert.AreEqual(a.A, b.A);
      Assert.IsNull(b.B);
    }

    [Test]
    public void SerializeSet()
    {
      SerializeSetClass a = new SerializeSetClass("test__", "test1");
      a.B = "test2";

      JObject target = new JObject();
      Serializer.Serialize((NewtonsoftJsonAdapter)target, a);

      AssertChildren(3, target);
      AssertNoAttribute("A", target);
      AssertAttribute("test2", "B", target);
      AssertAttribute("test1", "c", target);

      SerializeSetClass b = Deserializer.Deserialize<SerializeSetClass>((NewtonsoftJsonAdapter)target);

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

      JObject target = new JObject();
      Serializer.Serialize((NewtonsoftJsonAdapter)target, a);

      NanoSerializableClass b = Deserializer.Deserialize<NanoSerializableClass>((NewtonsoftJsonAdapter)target);

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

      JObject target = new JObject();
      Serializer.Serialize((NewtonsoftJsonAdapter)target, a);

      AssertChildren(3, target);

      AssertChildren(1, (JObject)target.GetValue("Ints"));
      AssertChildren(1, (JObject)target.GetValue("Floats"));

      PrimitiveArrayClass b = Deserializer.Deserialize<PrimitiveArrayClass>((NewtonsoftJsonAdapter)target);

      for (int i = 0; i < a.Ints.Length; i++)
        Assert.AreEqual(a.Ints[i], b.Ints[i]);
      for (int i = 0; i < a.Floats.Length; i++)
        Assert.AreEqual(a.Floats[i], b.Floats[i]);
      for (int x = 0; x < a.Ints2.GetLength(0); x++)
        for (int y = 0; y < a.Ints2.GetLength(1); y++)
          Assert.AreEqual(a.Ints2[x, y], b.Ints2[x, y]);
    }
  }
}

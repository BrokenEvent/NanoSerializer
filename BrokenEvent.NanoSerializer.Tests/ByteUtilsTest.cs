using System;

using NUnit.Framework;

namespace BrokenEvent.NanoSerializer.Tests
{
  [TestFixture]
  public class ByteUtilsTest
  {
    [Test]
    public void Sizeofs()
    {
      Assert.AreEqual(sizeof(bool), ByteUtils.GetSizeOf(typeof(bool)));
      Assert.AreEqual(sizeof(byte), ByteUtils.GetSizeOf(typeof(byte)));
      Assert.AreEqual(sizeof(sbyte), ByteUtils.GetSizeOf(typeof(sbyte)));
      Assert.AreEqual(sizeof(char), ByteUtils.GetSizeOf(typeof(char)));
      Assert.AreEqual(sizeof(double), ByteUtils.GetSizeOf(typeof(double)));
      Assert.AreEqual(sizeof(float), ByteUtils.GetSizeOf(typeof(float)));
      Assert.AreEqual(sizeof(int), ByteUtils.GetSizeOf(typeof(int)));
      Assert.AreEqual(sizeof(uint), ByteUtils.GetSizeOf(typeof(uint)));
      Assert.AreEqual(sizeof(long), ByteUtils.GetSizeOf(typeof(long)));
      Assert.AreEqual(sizeof(ulong), ByteUtils.GetSizeOf(typeof(ulong)));
      Assert.AreEqual(sizeof(short), ByteUtils.GetSizeOf(typeof(short)));
      Assert.AreEqual(sizeof(ushort), ByteUtils.GetSizeOf(typeof(ushort)));
    }

    private static void GenericTest<TType>(TType[] value)
    {
      int size = ByteUtils.GetSizeOf(typeof(TType));
      byte[] buffer = new byte[value.Length * size];

      Action<byte[], int, object> writer = ByteUtils.GetBinaryWriter(typeof(TType));

      for (int i = 0; i < value.Length; i++)
        writer(buffer, i * size, value[i]);

      Func<byte[], int, object> reader = ByteUtils.GetBinaryReader(typeof(TType));
      for (int i = 0; i < value.Length; i++)
        Assert.AreEqual(value[i], reader(buffer, i * size), typeof(TType).Name);
    }

    [Test]
    public void BoolReadWrite()
    {
      GenericTest(new bool[] { true, false, false, true, true });
    }

    [Test]
    public void ByteReadWrite()
    {
      GenericTest(new byte[] { 24, 35, 127, 254 });
    }

    [Test]
    public void SByteReadWrite()
    {
      GenericTest(new sbyte[] { 24, 35, 127, -120 });
    }

    [Test]
    public void CharReadWrite()
    {
      GenericTest(new char[] { 't', 'e', 's', 't', (char)249 });
    }

    [Test]
    public void DoubleReadWrite()
    {
      GenericTest(new double[] { 0.5458758787, 0.245421214, 0.0000001, 0.0002121, 21212.4878 });
    }

    [Test]
    public void FloatReadWrite()
    {
      GenericTest(new float[] { 0.5458758787f, 0.245421214f, 0.0000001f, 0.0002121f, 21212.4878f });
    }

    [Test]
    public void IntReadWrite()
    {
      GenericTest(new int[] { 8787, -8787, 1, 0 });
    }

    [Test]
    public void UIntReadWrite()
    {
      GenericTest(new uint[] { (uint)int.MaxValue + 1, 0, 5787, 87979879 });
    }

    [Test]
    public void LongReadWrite()
    {
      GenericTest(new long[] { 8787987987987, -78788787454 });
    }

    [Test]
    public void ULongReadWrite()
    {
      GenericTest(new ulong[] { 54545545554545, 87876676565787, 12102121012121212, 0 });
    }

    [Test]
    public void ShortReadWrite()
    {
      GenericTest(new short[] { -32000, 32000, 0, 4587 });
    }

    [Test]
    public void UShortReadWrite()
    {
      GenericTest(new ushort[] { 32000, 64000, 48787, 0 });
    }
  }
}

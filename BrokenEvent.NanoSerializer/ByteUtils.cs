using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BrokenEvent.NanoSerializer
{
  internal static class ByteUtils
  {
    // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/built-in-types-table
    private static Dictionary<Type, Action<byte[], int, object>> byteWriters = new Dictionary<Type, Action<byte[], int, object>>
    {
      { typeof(bool), (buffer, index, value) => new BytesUnion{BoolValue = (bool)value}.WriteBuffer(buffer, index, sizeof(bool)) },
      { typeof(byte), (buffer, index, value) => buffer[index] = (byte)value },
      { typeof(sbyte), (buffer, index, value) => new BytesUnion{SByteValue = (sbyte)value}.WriteBuffer(buffer, index, sizeof(sbyte)) },
      { typeof(char), (buffer, index, value) => new BytesUnion{CharValue = (char)value}.WriteBuffer(buffer, index, sizeof(char)) },
      { typeof(double), (buffer, index, value) => new BytesUnion{DoubleValue = (double)value}.WriteBuffer(buffer, index, sizeof(double)) },
      { typeof(float), (buffer, index, value) => new BytesUnion{FloatValue = (float)value}.WriteBuffer(buffer, index, sizeof(float)) },
      { typeof(int), (buffer, index, value) => new BytesUnion{IntValue = (int)value}.WriteBuffer(buffer, index, sizeof(int)) },
      { typeof(uint), (buffer, index, value) => new BytesUnion{UIntValue = (uint)value}.WriteBuffer(buffer, index, sizeof(uint)) },
      { typeof(long), (buffer, index, value) => new BytesUnion{LongValue = (long)value}.WriteBuffer(buffer, index, sizeof(long)) },
      { typeof(ulong), (buffer, index, value) => new BytesUnion{ULongValue = (ulong)value}.WriteBuffer(buffer, index, sizeof(ulong)) },
      { typeof(short), (buffer, index, value) => new BytesUnion{ShortValue = (short)value}.WriteBuffer(buffer, index, sizeof(short)) },
      { typeof(ushort), (buffer, index, value) => new BytesUnion{UShortValue = (ushort)value}.WriteBuffer(buffer, index, sizeof(ushort)) },
    };

    private static Dictionary<Type, Func<byte[], int, object>> byteReaders = new Dictionary<Type, Func<byte[], int, object>>
    {
      { typeof(bool), (buffer, index) => { BytesUnion bytes = new BytesUnion(); bytes.ReadBuffer(buffer, index, sizeof(bool)); return bytes.BoolValue; } },
      { typeof(byte), (buffer, index) => { BytesUnion bytes = new BytesUnion(); bytes.ReadBuffer(buffer, index, sizeof(byte)); return bytes.ByteValue; } },
      { typeof(sbyte), (buffer, index) => { BytesUnion bytes = new BytesUnion(); bytes.ReadBuffer(buffer, index, sizeof(sbyte)); return bytes.SByteValue; } },
      { typeof(char), (buffer, index) => { BytesUnion bytes = new BytesUnion(); bytes.ReadBuffer(buffer, index, sizeof(char)); return bytes.CharValue; } },
      { typeof(double), (buffer, index) => { BytesUnion bytes = new BytesUnion(); bytes.ReadBuffer(buffer, index, sizeof(double)); return bytes.DoubleValue; } },
      { typeof(float), (buffer, index) => { BytesUnion bytes = new BytesUnion(); bytes.ReadBuffer(buffer, index, sizeof(float)); return bytes.FloatValue; } },
      { typeof(int), (buffer, index) => { BytesUnion bytes = new BytesUnion(); bytes.ReadBuffer(buffer, index, sizeof(int)); return bytes.IntValue; } },
      { typeof(uint), (buffer, index) => { BytesUnion bytes = new BytesUnion(); bytes.ReadBuffer(buffer, index, sizeof(uint)); return bytes.UIntValue; } },
      { typeof(long), (buffer, index) => { BytesUnion bytes = new BytesUnion(); bytes.ReadBuffer(buffer, index, sizeof(long)); return bytes.LongValue; } },
      { typeof(ulong), (buffer, index) => { BytesUnion bytes = new BytesUnion(); bytes.ReadBuffer(buffer, index, sizeof(ulong)); return bytes.ULongValue; } },
      { typeof(short), (buffer, index) => { BytesUnion bytes = new BytesUnion(); bytes.ReadBuffer(buffer, index, sizeof(short)); return bytes.ShortValue; } },
      { typeof(ushort), (buffer, index) => { BytesUnion bytes = new BytesUnion(); bytes.ReadBuffer(buffer, index, sizeof(ushort)); return bytes.UShortValue; } },
    };

    private static Dictionary<Type, int> sizeofs = new Dictionary<Type, int>
    {
      { typeof(bool), sizeof(bool) },
      { typeof(byte), sizeof(byte) },
      { typeof(sbyte), sizeof(sbyte) },
      { typeof(char), sizeof(char) },
      { typeof(double), sizeof(double) },
      { typeof(float), sizeof(float) },
      { typeof(int), sizeof(int) },
      { typeof(uint), sizeof(uint) },
      { typeof(long), sizeof(long) },
      { typeof(ulong), sizeof(ulong) },
      { typeof(short), sizeof(short) },
      { typeof(ushort), sizeof(ushort) },
    };

    public static Action<byte[], int, object> GetBinaryWriter(Type type)
    {
      return byteWriters[type];
    }

    public static int GetSizeOf(Type type)
    {
      return sizeofs[type];
    }

    public static Func<byte[], int, object> GetBinaryReader(Type type)
    {
      return byteReaders[type];
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct BytesUnion
    {
      [FieldOffset(0)]
      public byte ByteValue;
      [FieldOffset(0)]
      public sbyte SByteValue;
      [FieldOffset(0)]
      public bool BoolValue;
      [FieldOffset(0)]
      public char CharValue;
      [FieldOffset(0)]
      public short ShortValue;
      [FieldOffset(0)]
      public ushort UShortValue;
      [FieldOffset(0)]
      public long LongValue;
      [FieldOffset(0)]
      public ulong ULongValue;
      [FieldOffset(0)]
      public int IntValue;
      [FieldOffset(0)]
      public uint UIntValue;
      [FieldOffset(0)]
      public float FloatValue;
      [FieldOffset(0)]
      public double DoubleValue;

      [FieldOffset(0)]
      private byte Byte0;
      [FieldOffset(1)]
      private byte Byte1;
      [FieldOffset(2)]
      private byte Byte2;
      [FieldOffset(3)]
      private byte Byte3;
      [FieldOffset(4)]
      private byte Byte4;
      [FieldOffset(5)]
      private byte Byte5;
      [FieldOffset(6)]
      private byte Byte6;
      [FieldOffset(7)]
      private byte Byte7;

      public void ReadBuffer(byte[] buffer, int index, int count)
      {
        Byte0 = buffer[index++];
        if (count > 1)
          Byte1 = buffer[index++];
        if (count > 2)
          Byte2 = buffer[index++];
        if (count > 3)
          Byte3 = buffer[index++];
        if (count > 4)
          Byte4 = buffer[index++];
        if (count > 5)
          Byte5 = buffer[index++];
        if (count > 6)
          Byte6 = buffer[index++];
        if (count > 7)
          Byte7 = buffer[index++];
      }

      public void WriteBuffer(byte[] buffer, int index, int count)
      {
        buffer[index++] = Byte0;
        if (count > 1)
          buffer[index++] = Byte1;
        if (count > 2)
          buffer[index++] = Byte2;
        if (count > 3)
          buffer[index++] = Byte3;
        if (count > 4)
          buffer[index++] = Byte4;
        if (count > 5)
          buffer[index++] = Byte5;
        if (count > 6)
          buffer[index++] = Byte6;
        if (count > 7)
          buffer[index] = Byte7;
      }
    }

    public static int GetBytesInBase64(string base64)
    {
      int result = base64.Length * 3 / 4;
      if (base64.EndsWith("=="))
        result -= 2;
      else if (base64.EndsWith("="))
        result -= 1;

      return result;
    }
  }
}

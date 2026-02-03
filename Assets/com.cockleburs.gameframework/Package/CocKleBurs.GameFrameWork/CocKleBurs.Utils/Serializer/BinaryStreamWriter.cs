using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;


namespace CockleBurs.GameFramework.Utility
{
// 序列化接口
public interface IBinarySerializable
{
    void Serialize(ref BinaryStreamWriter writer);
    void Deserialize(ref BinaryStreamReader reader);
}

// 高性能二进制写入器
public ref struct BinaryStreamWriter
{
    private byte[] _buffer;
    private int _position;
    private ArrayPool<byte> _pool;

    public int Position => _position;
    public int Capacity => _buffer.Length;

    public BinaryStreamWriter(int initialCapacity = 128, ArrayPool<byte> pool = null)
    {
        _pool = pool ?? ArrayPool<byte>.Shared;
        _buffer = _pool.Rent(initialCapacity);
        _position = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write<T>(T value) where T : unmanaged
    {
        int size = Unsafe.SizeOf<T>();
        EnsureCapacity(size);
        
        Unsafe.WriteUnaligned(ref _buffer[_position], value);
        _position += size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            Write(0);
            return;
        }

        int byteCount = System.Text.Encoding.UTF8.GetByteCount(value);
        Write(byteCount);
        EnsureCapacity(byteCount);
        
        System.Text.Encoding.UTF8.GetBytes(value, 0, value.Length, _buffer, _position);
        _position += byteCount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteDictionary<TKey, TValue>(Dictionary<TKey, TValue> dict) 
        where TKey : unmanaged 
        where TValue : unmanaged
    {
        if (dict == null)
        {
            Write(0);
            return;
        }

        Write(dict.Count);
        foreach (var kvp in dict)
        {
            Write(kvp.Key);
            Write(kvp.Value);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnityDictionary<TKey, TValue>(Dictionary<TKey, TValue> dict) 
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : IBinarySerializable, new()
    {
        if (dict == null)
        {
            Write(0);
            return;
        }

        Write(dict.Count);
        foreach (var kvp in dict)
        {
            Write(kvp.Key);
            kvp.Value.Serialize(ref this);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureCapacity(int additional)
    {
        if (_position + additional <= _buffer.Length) return;
        
        int newCapacity = Math.Max(_buffer.Length * 2, _position + additional);
        byte[] newBuffer = _pool.Rent(newCapacity);
        Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _position);
        _pool.Return(_buffer);
        _buffer = newBuffer;
    }

    public byte[] ToArrayAndDispose()
    {
        byte[] result = new byte[_position];
        Buffer.BlockCopy(_buffer, 0, result, 0, _position);
        _pool.Return(_buffer);
        _buffer = null;
        return result;
    }
}

// 高性能二进制读取器
public ref struct BinaryStreamReader
{
    private ReadOnlySpan<byte> _span;
    private int _position;

    public int Position => _position;

    public BinaryStreamReader(byte[] buffer)
    {
        _span = buffer;
        _position = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Read<T>() where T : unmanaged
    {
        int size = Unsafe.SizeOf<T>();
        if (_position + size > _span.Length)
            throw new EndOfStreamException();

        T value = Unsafe.ReadUnaligned<T>(ref Unsafe.AsRef(in _span[_position]));
        _position += size;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadString()
    {
        int byteCount = Read<int>();
        if (byteCount == 0) return string.Empty;
        
        if (_position + byteCount > _span.Length)
            throw new EndOfStreamException();

        string result = System.Text.Encoding.UTF8.GetString(
            _span.Slice(_position, byteCount));
        
        _position += byteCount;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>() 
        where TKey : unmanaged 
        where TValue : unmanaged
    {
        int count = Read<int>();
        if (count == 0) return null;

        var dict = new Dictionary<TKey, TValue>(count);
        for (int i = 0; i < count; i++)
        {
            TKey key = Read<TKey>();
            TValue value = Read<TValue>();
            dict.Add(key, value);
        }
        return dict;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Dictionary<TKey, TValue> ReadUnityDictionary<TKey, TValue>() 
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : IBinarySerializable, new()
    {
        int count = Read<int>();
        if (count == 0) return null;

        var dict = new Dictionary<TKey, TValue>(count);
        for (int i = 0; i < count; i++)
        {
            TKey key = Read<TKey>();
            TValue value = new TValue();
            value.Deserialize(ref this);
            dict.Add(key, value);
        }
        return dict;
    }
}

// 示例使用
public class PlayerData : IBinarySerializable
{
    public int Health;
    public Vector3 Position;
    public Dictionary<int, ItemData> Inventory = new();

    public void Serialize(ref BinaryStreamWriter writer)
    {
        writer.Write(Health);
        writer.Write(Position);
        writer.WriteUnityDictionary(Inventory);
    }

    public void Deserialize(ref BinaryStreamReader reader)
    {
        Health = reader.Read<int>();
        Position = reader.Read<Vector3>();
        Inventory = reader.ReadUnityDictionary<int, ItemData>();
    }
}

[Serializable]
public struct ItemData : IBinarySerializable
{
    public int ID;
    public float Durability;

    public void Serialize(ref BinaryStreamWriter writer)
    {
        writer.Write(ID);
        writer.Write(Durability);
    }

    public void Deserialize(ref BinaryStreamReader reader)
    {
        ID = reader.Read<int>();
        Durability = reader.Read<float>();
    }
}
}
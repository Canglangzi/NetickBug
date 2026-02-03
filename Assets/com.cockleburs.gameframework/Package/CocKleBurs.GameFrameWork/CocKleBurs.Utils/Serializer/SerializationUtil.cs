using System;
using System.IO;
using Unity.Collections;
using UnityEngine;


namespace CockleBurs.GameFramework.Utility
{
public static class SerializationUtil
{
    public static NativeArray<byte> SerializeToNativeArray<T>(T obj)
    {
        byte[] byteArray;
        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(memoryStream))
            {
                if (obj is int intValue)
                {
                    writer.Write(intValue);
                }
                else if (obj is float floatValue)
                {
                    writer.Write(floatValue);
                }
                else if (obj is bool boolValue)
                {
                    writer.Write(boolValue);
                }
                else if (obj is Vector2 vec2)
                {
                    writer.Write(vec2.x);
                    writer.Write(vec2.y);
                }
                else if (obj is Vector3 vec3)
                {
                    writer.Write(vec3.x);
                    writer.Write(vec3.y);
                    writer.Write(vec3.z);
                }
                else if (obj is Quaternion quat)
                {
                    writer.Write(quat.eulerAngles.x);
                    writer.Write(quat.eulerAngles.y);
                    writer.Write(quat.eulerAngles.z);
                }
                else
                {
                    throw new NotSupportedException("Type not supported for serialization");
                }
            }
            byteArray = memoryStream.ToArray();
        }

        NativeArray<byte> nativeArray = new NativeArray<byte>(byteArray.Length, Allocator.Temp);
        nativeArray.CopyFrom(byteArray);

        return nativeArray;
    }

    public static T DeserializeFromNativeArray<T>(NativeArray<byte> nativeArray) where T : struct
    {
        if (nativeArray.Length == 0)
            throw new ArgumentException("NativeArray is empty.", nameof(nativeArray));

        byte[] byteArray = nativeArray.ToArray();

        using (MemoryStream memoryStream = new MemoryStream(byteArray))
        {
            using (BinaryReader reader = new BinaryReader(memoryStream))
            {
                if (typeof(T) == typeof(int))
                {
                    return (T)(object)reader.ReadInt32();
                }
                else if (typeof(T) == typeof(float))
                {
                    return (T)(object)reader.ReadSingle();
                }
                else if (typeof(T) == typeof(bool))
                {
                    return (T)(object)reader.ReadBoolean();
                }
                else if (typeof(T) == typeof(Vector2))
                {
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    return (T)(object)new Vector2(x, y);
                }
                else if (typeof(T) == typeof(Vector3))
                {
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    float z = reader.ReadSingle();
                    return (T)(object)new Vector3(x, y, z);
                }
                else if (typeof(T) == typeof(Quaternion))
                {
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    float z = reader.ReadSingle();
                    return (T)(object)Quaternion.Euler(x, y, z);
                }
                else
                {
                    throw new NotSupportedException("Type not supported for deserialization");
                }
            }
        }
    }
}

}
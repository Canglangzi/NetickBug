using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;


namespace CockleBurs.GameFramework.Utility
{
// 核心序列化接口
public interface IBurstSerializable
{
    void Serialize(ref NativeList<byte> output);
    void Deserialize(NativeArray<byte> input, ref int position);
}
//
[BurstCompile]
// 核心序列化工具类
public unsafe  static class BurstSerializer
{
    // 通用序列化方法
    [BurstCompile]
    public static unsafe void Serialize<T>(in T data, ref NativeList<byte> output)
        where T : unmanaged, IBurstSerializable
    {
        data.Serialize(ref output);
    }

    // 通用反序列化方法
    [BurstCompile]
    public static unsafe T Deserialize<T>(NativeArray<byte> input)
        where T : unmanaged, IBurstSerializable
    {
        T result = default;
        int position = 0;
        result.Deserialize(input, ref position);
        return result;
    }

    // ===== 基本类型序列化方法 =====
    
    [BurstCompile]
    public static unsafe void WriteBytes(void* data, int size, ref NativeList<byte> output)
    {
        int start = output.Length;
        output.ResizeUninitialized(start + size);
        byte* src = (byte*)data;
        byte* dst = (byte*)output.GetUnsafePtr() + start;
        UnsafeUtility.MemCpy(dst, src, size);
    }



    // 简单类型的序列化辅助方法
    [BurstCompile] public static void Write(int value, ref NativeList<byte> output) => WriteBytes(&value, sizeof(int), ref output);
    [BurstCompile] public static void Write(float value, ref NativeList<byte> output) => WriteBytes(&value, sizeof(float), ref output);
    [BurstCompile] public static void Write(double value, ref NativeList<byte> output) => WriteBytes(&value, sizeof(double), ref output);
    [BurstCompile] public static void Write(bool value, ref NativeList<byte> output) => WriteBytes(&value, sizeof(bool), ref output);
    [BurstCompile] public static void Write(byte value, ref NativeList<byte> output) => WriteBytes(&value, sizeof(byte), ref output);
    
    // 简单类型的反序列化辅助方法
    [BurstCompile]
    public static void ReadBytes(void* destData, int size, byte* srcInput, int inputLength, ref int position)
    {
        // 检查边界
        if (position + size > inputLength)
        {
            position = inputLength; // 跳过无效数据
            return;
        }
        
        // 执行内存复制
        byte* srcPtr = srcInput + position;
        byte* dstPtr = (byte*)destData;
        UnsafeUtility.MemCpy(dstPtr, srcPtr, size);
        
        // 更新位置
        position += size;
    }

    // 简单类型的反序列化辅助方法（使用指针）
    [BurstCompile] 
    public static unsafe int ReadInt(byte* input, int inputLength, ref int position)
    {
        int value;
        ReadBytes(&value, sizeof(int), input, inputLength, ref position);
        return value;
    }
    
    [BurstCompile] 
    public static unsafe float ReadFloat(byte* input, int inputLength, ref int position)
    {
        float value;
        ReadBytes(&value, sizeof(float), input, inputLength, ref position);
        return value;
    }
    
    [BurstCompile] 
    public static unsafe bool ReadBool(byte* input, int inputLength, ref int position)
    {
        bool value;
        ReadBytes(&value, sizeof(bool), input, inputLength, ref position);
        return value;
    }
    [BurstCompile]
    public static unsafe void ReadFixedString32(
        byte* input, 
        int inputLength, 
        ref int position,
        FixedString32Bytes* output) // 添加指针参数输出结果
    {
        int length = ReadInt(input, inputLength, ref position);
        length = math.min(length, FixedString32Bytes.UTF8MaxLengthInBytes);
        if (length > 0)
        {
            ReadBytes(output->GetUnsafePtr(), length, input, inputLength, ref position);
            output->Length = (ushort)length;
        }
        else
        {
            *output = default; // 设置默认值
        }
    }

    [BurstCompile]
    public static unsafe void ReadFixedString64(
        byte* input, 
        int inputLength, 
        ref int position,
        FixedString64Bytes* output) // 添加指针参数输出结果
    {
        int length = ReadInt(input, inputLength, ref position);
        length = math.min(length, FixedString64Bytes.UTF8MaxLengthInBytes);
        if (length > 0)
        {
            ReadBytes(output->GetUnsafePtr(), length, input, inputLength, ref position);
            output->Length = (ushort)length;
        }
        else
        {
            *output = default; // 设置默认值
        }
    }
//
    [BurstCompile]
    public static unsafe void ReadFixedString128(
        byte* input, 
        int inputLength, 
        ref int position,
        FixedString128Bytes* output) // 添加指针参数输出结果
    {
        int length = ReadInt(input, inputLength, ref position);
        length = math.min(length, FixedString128Bytes.UTF8MaxLengthInBytes);
        if (length > 0)
        {
            ReadBytes(output->GetUnsafePtr(), length, input, inputLength, ref position);
            output->Length = (ushort)length;
        }
        else
        {
            *output = default; // 设置默认值
        }
    }
}
}
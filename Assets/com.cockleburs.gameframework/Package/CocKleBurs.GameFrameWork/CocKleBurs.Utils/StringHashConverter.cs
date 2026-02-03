using System;
using System.Runtime.CompilerServices;


namespace CockleBurs.GameFramework.Utility
{
public static class StringHashConverter
{
    // 默认哈希种子值
    private const uint Seed = 0x811C9DC5u;

    // 乘法常数 (FNV-1a算法)
    private const uint Prime = 0x1000193u;

    /// <summary>
    /// 将字符串转换为稳定的 uint 哈希值
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint StringToStableUInt(string input)
    {
        if (string.IsNullOrEmpty(input))
            return 0;

        // 使用内存安全的方式访问字符串
        return StringToStableUInt(input.AsSpan());
    }

    /// <summary>
    /// 高性能版本（使用 ReadOnlySpan<char>）
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint StringToStableUInt(ReadOnlySpan<char> input)
    {
        if (input.IsEmpty)
            return 0;

        // 使用 FNV-1a 哈希算法变种
        uint hash = Seed;

        // 使用指针操作提升性能
        unsafe
        {
            fixed (char* ptr = input)
            {
                char* current = ptr;
                char* end = ptr + input.Length;

                while (current < end)
                {
                    // 处理UTF-16代理对
                    if (char.IsHighSurrogate(*current) &&
                        (current + 1 < end) &&
                        char.IsLowSurrogate(*(current + 1)))
                    {
                        // 处理完整Unicode字符
                        int codePoint = char.ConvertToUtf32(*current, *(current + 1));
                        hash = (hash ^ (uint)codePoint) * Prime;
                        current += 2;
                    }
                    else
                    {
                        // 处理单个字符
                        hash = (hash ^ *current) * Prime;
                        current++;
                    }
                }
            }
        }

        return hash;
    }
}
}
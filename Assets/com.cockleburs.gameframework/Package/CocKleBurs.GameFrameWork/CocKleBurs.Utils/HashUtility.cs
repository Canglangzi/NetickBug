using System;


namespace CockleBurs.GameFramework.Utility
{
public static class HashUtility
{
    // FNV-1a 64位哈希算法常数
    private const ulong OffsetBasis64 = 14695981039346656037;
    private const ulong Prime64 = 1099511628211;

    // FNV-1a 32位哈希算法常数
    private const uint OffsetBasis32 = 2166136261;
    private const uint Prime32 = 16777619;

    // 64位哈希函数
    public static long HashStringWithFNV1A64(string text)
    {
        if (text == null)
            throw new ArgumentNullException(nameof(text));

        ulong result = OffsetBasis64;

        foreach (var c in text)
        {
            result ^= (byte)(c & 255); // 处理低8位
            result *= Prime64;         // 乘以FNV素数

            result ^= (byte)(c >> 8);  // 处理高8位
            result *= Prime64;         // 乘以FNV素数
        }

        return (long)result; // 返回结果
    }

    // 32位哈希函数
    public static uint HashStringWithFNV1A32(string input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        uint hash = OffsetBasis32;

        foreach (char c in input)
        {
            hash ^= (byte)c; // XOR 当前字符的底部字节
            hash *= Prime32; // 乘以FNV素数
        }

        return hash; // 返回结果
    }
}

}
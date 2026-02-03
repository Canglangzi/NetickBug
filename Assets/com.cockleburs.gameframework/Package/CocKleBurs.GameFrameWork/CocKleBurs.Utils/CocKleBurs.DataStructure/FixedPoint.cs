using System;
using Netick;


namespace CockleBurs.GameFramework.Utility
{
[Serializable] [Networked]
public struct FixedPoint : IEquatable<FixedPoint>, IComparable<FixedPoint>
{
    public const int SCALE_BITS = 16; // 小数位位数
    public const int SCALE_FACTOR = 1 << SCALE_BITS; // 缩放因子 65536
    private const long MAX_VALUE = int.MaxValue;
    private const long MIN_VALUE = int.MinValue;

   [Networked] public long RawValue { get; } // 存储实际整数值

    // 构造函数
    public FixedPoint(float value) => RawValue = (long)(value * SCALE_FACTOR);
    public FixedPoint(int value) => RawValue = value * SCALE_FACTOR;
    private FixedPoint(long rawValue) => RawValue = rawValue;

    // 基本运算符重载
    public static FixedPoint operator +(FixedPoint a, FixedPoint b) 
        => new FixedPoint(a.RawValue + b.RawValue);
    
    public static FixedPoint operator -(FixedPoint a, FixedPoint b) 
        => new FixedPoint(a.RawValue - b.RawValue);
    
    public static FixedPoint operator *(FixedPoint a, FixedPoint b)
    {
        // 64位中间值防止溢出
        long product = a.RawValue * b.RawValue;
        return new FixedPoint(product >> SCALE_BITS); // 重新缩放
    }
    
    public static FixedPoint operator /(FixedPoint a, FixedPoint b)
    {
        if (b.RawValue == 0) throw new DivideByZeroException();
        // 被除数左移以保留精度
        long dividend = a.RawValue << SCALE_BITS;
        return new FixedPoint(dividend / b.RawValue);
    }

    // 比较运算符
    public static bool operator >(FixedPoint a, FixedPoint b) => a.RawValue > b.RawValue;
    public static bool operator <(FixedPoint a, FixedPoint b) => a.RawValue < b.RawValue;
    public int CompareTo(FixedPoint other) => RawValue.CompareTo(other.RawValue);
    public bool Equals(FixedPoint other) => RawValue == other.RawValue;

    // 类型转换
    public float ToFloat() => (float)RawValue / SCALE_FACTOR;
    public int ToInt() => (int)(RawValue >> SCALE_BITS); // 直接截断
    
    // 四舍五入到整数
    public int RoundToInt() 
        => (int)((RawValue + (SCALE_FACTOR / 2)) >> SCALE_BITS);

    // 常用数学函数
    public static FixedPoint Abs(FixedPoint value)
        => new FixedPoint(Math.Abs(value.RawValue));
    
    public static FixedPoint Sqrt(FixedPoint value)
    {
        if (value.RawValue < 0) throw new ArgumentException("负数不能开平方");
        // 使用巴比伦算法（快速整数平方根）
        long raw = value.RawValue << SCALE_BITS; // 左移保持精度
        long root = (long)Math.Sqrt(raw);
        return new FixedPoint(root);
    }

    // 字符串输出（调试用）
    public override string ToString() => ToFloat().ToString("F4");
}
}
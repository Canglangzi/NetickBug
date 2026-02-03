using System;


namespace CockleBurs.GameFramework.Extension
{
public interface IBitmaskConvertible
{
    /// <summary>
    /// 将对象状态打包为整型位掩码
    /// </summary>
    int ToBitmask();
    
    /// <summary>
    /// 从位掩码还原对象状态
    /// </summary>
    void FromBitmask(int bitmask);
}

public static class BitmaskExtensions
{
    /// <summary>
    /// 检查位掩码中特定位是否设置
    /// </summary>
    public static bool IsSet(this int bitmask, int flag)
    {
        return (bitmask & flag) != 0;
    }

    /// <summary>
    /// 设置位掩码中的特定位
    /// </summary>
    public static int SetFlag(this int bitmask, int flag, bool value)
    {
        return value ? (bitmask | flag) : (bitmask & ~flag);
    }
}
}
using System;


namespace CockleBurs.GameFramework.Utility
{
public enum EMyEnum : byte
{
    False,
    True,
    Maybe
}

[Serializable]
public class TEnumAsByte<T> where T : Enum
{
    private byte value;

    public TEnumAsByte(T initialValue)
    {
        value = Convert.ToByte(initialValue);
    }

    public T Value
    {
        get => (T)(object)value;
        set => this.value = Convert.ToByte(value);
    }

    public static implicit operator TEnumAsByte<T>(T enumValue) => new TEnumAsByte<T>(enumValue);
    public static implicit operator T(TEnumAsByte<T> enumAsByte) => enumAsByte.Value;

    public override string ToString()
    {
        return Value.ToString();
    }
}
}
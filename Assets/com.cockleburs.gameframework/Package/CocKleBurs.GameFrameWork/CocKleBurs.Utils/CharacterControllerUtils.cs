using System.Collections.Generic;
using UnityEngine;


namespace CockleBurs.GameFramework.Utility
{
public static class CCUtils
{
    /// <summary>
    /// Modulo operation.
    /// </summary>
    /// <param name="x">dividend</param>
    /// <param name="m">divisor</param>
    /// <returns></returns>
    public static int Modulo(int x, int m)
    {
        return (x % m + m) % m;
    }

    /// <summary>
    /// Shuffles a list.
    /// </summary>
    public static void Shuffle<T>(this IList<T> list)
    {
        int n     = list.Count;
        while (n > 1)
        {
            n--;
            int k   = UnityEngine.Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static Vector2 ClampAngles(Vector2 angles)
    {
        return new Vector2(ClampAngle(angles.x, -360, 360), ClampAngle(angles.y, -80, 80));
    }

    public static Vector2 ClampAngles(float yaw, float pitch)
    {
        return new Vector2(ClampAngle(yaw, -360, 360), ClampAngle(pitch, -80, 80));
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}

}
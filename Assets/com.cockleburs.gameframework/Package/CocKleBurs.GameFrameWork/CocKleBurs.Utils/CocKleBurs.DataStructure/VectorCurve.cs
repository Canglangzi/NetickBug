using UnityEngine;


namespace CockleBurs.GameFramework.Utility
{
[System.Serializable]
public class VectorCurve
{
    // 三个独立的 AnimationCurve 控制 x、y、z
    public AnimationCurve curveX;
    public AnimationCurve curveY;
    public AnimationCurve curveZ;

    // 在指定时间点计算 Vector3
    public Vector3 Evaluate(float time)
    {
        float x = curveX.Evaluate(time);
        float y = curveY.Evaluate(time);
        float z = curveZ.Evaluate(time);
        return new Vector3(x, y, z);
    }
}
}
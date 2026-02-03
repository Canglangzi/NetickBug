using UnityEngine;


namespace CockleBurs.GameFramework.Utility
{
[System.Serializable]
public struct SecondOrderDynamic
{
    // 动力学的基本参数
    public Vector3 velocity;       // 速度
    public Vector3 position;       // 当前位置
    public Vector3 targetPosition; // 目标位置

    public float damping;          // 阻尼系数
    public float springConstant;   // 弹簧常数
    public float mass;             // 质量

    private Vector3 previousVelocity; // 用于平滑计算
    private float smoothTime;     // 平滑时间
    private float maxDeltaTime;   // 最大的 deltaTime
    private Vector3 currentAcceleration; // 当前加速度

    // 构造函数
    public SecondOrderDynamic(Vector3 initialPosition, float damping, float springConstant, float mass, float smoothTime, float maxDeltaTime)
    {
        this.position = initialPosition;
        this.velocity = Vector3.zero;
        this.targetPosition = initialPosition;
        this.damping = damping;
        this.springConstant = springConstant;
        this.mass = mass;
        this.smoothTime = smoothTime;
        this.maxDeltaTime = maxDeltaTime;
        this.previousVelocity = Vector3.zero;
        this.currentAcceleration = Vector3.zero;
    }

    // 设置目标位置
    public void SetTargetPosition(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
    }

    // 更新动态系统状态
    public void Update(float deltaTime)
    {
        // 限制 deltaTime 以避免过大的步长
        deltaTime = Mathf.Min(deltaTime, maxDeltaTime);

        // 计算加速度
        currentAcceleration = (-springConstant * (position - targetPosition) - damping * velocity) / mass;

        // 使用运动方程更新速度和位置
        velocity += currentAcceleration * deltaTime;      // 更新速度
        position += velocity * deltaTime;                  // 更新位置

        // 使用 SmoothDamp 平滑位置
        position = Vector3.SmoothDamp(position, targetPosition, ref previousVelocity, smoothTime);
    }

    // 获取当前速度
    public Vector3 GetVelocity()
    {
        return velocity;
    }

    // 获取当前加速度
    public Vector3 GetAcceleration()
    {
        return currentAcceleration;
    }

    // 获取当前位置
    public Vector3 GetPosition()
    {
        return position;
    }
}

}
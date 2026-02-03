using UnityEngine;


namespace CockleBurs.GameFramework.Extension
{
// 在某个工具类中添加：
public static class NetworkCharacterControllerExtensions
{
    public static bool IsVelocityZero(this CharacterController controller)
    {
        return controller.velocity.sqrMagnitude < 0.1f;
    }
}
}
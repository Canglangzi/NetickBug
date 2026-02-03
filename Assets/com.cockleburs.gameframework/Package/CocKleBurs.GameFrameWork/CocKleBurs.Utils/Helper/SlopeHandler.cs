using UnityEngine;


namespace CockleBurs.GameFramework.Utility
{
public static class SlopeHandler
{
    public static bool OnMaxedAngleSlope(CharacterController controller)
    {
        if (controller.isGrounded && Physics.Raycast(controller.transform.position, Vector3.down, out RaycastHit hit, controller.height))
        {
            Vector3 slopeDirection = hit.normal;
            return Vector3.Angle(slopeDirection, Vector3.up) > controller.slopeLimit;
        }
        return false;
    }

    public static Vector3 SlopeDirection(Transform orientation, CharacterController controller)
    {
        if (Physics.Raycast(orientation.position, Vector3.down, out RaycastHit slopeHit, (controller.height / 2) + 0.1f))
            return Vector3.ProjectOnPlane(orientation.forward, slopeHit.normal);

        return orientation.forward;
    }

    public static float SlopeAngle(CharacterController controller)
    {
        if (Physics.Raycast(controller.transform.position, Vector3.down, out RaycastHit slopeHit))
            return (Vector3.Angle(Vector3.down, slopeHit.normal) - 180) * -1;

        return 0;
    }
}

}
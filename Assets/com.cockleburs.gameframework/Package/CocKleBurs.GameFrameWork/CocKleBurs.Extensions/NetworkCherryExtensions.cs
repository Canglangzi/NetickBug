
namespace CockleBurs.GameFramework.Extension
{
// using Netick;
// using UnityEngine;
// using Netick.Unity;
//
// public static class NetworkCherryExtensions
// {
//     /// <summary>
//     /// Transfers ownership of this NetworkBehaviour to another player.
//     /// </summary>
//     /// <param name="behaviour">The NetworkBehaviour to transfer.</param>
//     /// <param name="newOwner">The new owner's NetworkConnection.</param>
//     /// <param name="notifyClient">Whether to notify clients about the ownership change.</param>
//     public static void GiveOwnership(this NetworkCherry behaviour, NetworkPlayer newOwner, bool notifyClient = true)
//     {
//         if (behaviour.IsServerInitialized)
//         {
//             behaviour.Object.InputSource = newOwner;
//             
//             if (notifyClient)
//             {
//                 behaviour.OnOwnershipClient(behaviour.InputSource as NetworkConnection);
//             }
//             
//             if (behaviour.NetworkPlayer == behaviour.Object.InputSource)
//             {
//                 behaviour.StartControl();
//             }
//             else
//             {
//                 behaviour.StopControl();
//             }
//         }
//     }
//
//     /// <summary>
//     /// Checks if the local player owns this NetworkBehaviour.
//     /// </summary>
//     /// <param name="behaviour">The NetworkBehaviour to check.</param>
//     /// <returns>True if the local player owns this object.</returns>
//     public static bool IsLocallyOwned(this NetworkCherry behaviour)
//     {
//         return behaviour.LocalConnection == behaviour.Object.InputSource;
//     }
//
//     /// <summary>
//     /// Checks if the specified connection owns this NetworkBehaviour.
//     /// </summary>
//     /// <param name="behaviour">The NetworkBehaviour to check.</param>
//     /// <param name="connection">The connection to check ownership against.</param>
//     /// <returns>True if the specified connection owns this object.</returns>
//     public static bool IsOwnedBy(this NetworkCherry behaviour, NetworkConnection connection)
//     {
//         return behaviour.Object.InputSource == connection;
//     }
//     
//     /// <summary>
//     /// Resets ownership of this NetworkBehaviour back to the server.
//     /// </summary>
//     /// <param name="behaviour">The NetworkBehaviour to reset ownership of.</param>
//     public static void ResetOwnership(this NetworkCherry behaviour)
//     {
//         if (behaviour.IsServerInitialized)
//         {
//             behaviour.GiveOwnership(null);
//         }
//     }
//
//     /// <summary>
//     /// Temporarily takes control of this NetworkBehaviour (server-side only).
//     /// </summary>
//     /// <param name="behaviour">The NetworkBehaviour to take control of.</param>
//     public static void TakeTemporaryControl(this NetworkCherry behaviour)
//     {
//         if (behaviour.IsServerInitialized)
//         {
//             behaviour.StartControl();
//         }
//     }
//
//     /// <summary>
//     /// Releases temporary control of this NetworkBehaviour (server-side only).
//     /// </summary>
//     /// <param name="behaviour">The NetworkBehaviour to release control of.</param>
//     public static void ReleaseTemporaryControl(this NetworkCherry behaviour)
//     {
//         if (behaviour.IsServerInitialized)
//         {
//             behaviour.StopControl();
//         }
//     }
// }
}
using System;
using Netick.Unity;
using System.Threading.Tasks;
using CockleBurs.GameFramework.Utility;
using UnityEngine;
using UnityEngine.Serialization;


namespace CockleBurs.GameFramework.Core
{
public interface INetworkSpawn
{
    void OnNetworkSpawn();
}
public interface INetworkDestroy
{
    void OnNetworkDestroy();
}
public enum ObjectReconnectBehavior
{
    /// <summary>对象不参与重连流程</summary>
    SkipReconnection,  
    Destroy,
    /// <summary>保留对象并不转移控制权</summary>
    RetainWithoutOwnership,
    
    /// <summary>完全接管对象所有权并恢复控制</summary>
    TakeOwnership
}
[System.Serializable]

public class ReplicatedObject : NetworkObject
{
  int LastPlayerID; 
   int PlayerID;
    [SerializeReference] public ObjectReconnectBehavior objectReconnectBehavior = ObjectReconnectBehavior.SkipReconnection;
    

    private void PlayerEventsOnOnClientCherryInputSourceLef(ReplicatedObject Client)
    {
        if (Client.objectReconnectBehavior == ObjectReconnectBehavior.Destroy)
        {
            
        }
    }

    public virtual void Destroy()
    {
    //    world.PlayerEvents.OnClientCherryInputSourceLef -= PlayerEventsOnOnClientCherryInputSourceLef;
    }
    public virtual void CallOnNetworkSpawn()
    {
        int length = NetworkedBehaviours.Length;
        for (int i = 0; i < length; i++)
        {
            var behaviour = NetworkedBehaviours[i];
            if (behaviour is INetworkSpawn networkSpawnBehaviour)
            {
                networkSpawnBehaviour.OnNetworkSpawn();
            }
        }
    }

    public virtual void CallNetworkDestroy()
    {
        int length = NetworkedBehaviours.Length;
        for (int i = 0; i < length; i++)
        {
            var behaviour = NetworkedBehaviours[i];
            if (behaviour is INetworkDestroy networkDestroyBehaviour)
            {
                networkDestroyBehaviour.OnNetworkDestroy();
            }
        }
    }
    // <summary>
    /// 关联的玩家持久化ID（服务器端维护）
    /// </summary>
    [SerializeField] private string associatedPlayerId;

    /// <summary>
    /// 服务器端：设置对象关联的玩家ID
    /// </summary>
    [Server]
    public void SetAssociatedPlayerId(string playerId)
    {
        associatedPlayerId = playerId;
        // 可选：同步到客户端（如果需要客户端知道归属）
        // RpcSyncAssociatedPlayerId(playerId);
    }

    /// <summary>
    /// 获取关联的玩家ID（服务器端使用）
    /// </summary>
    public string GetAssociatedPlayerId() => associatedPlayerId;
  
    // [ClientRpc]
    // private void RpcSyncAssociatedPlayerId(string playerId)
    // {
    //     associatedPlayerId = playerId;
    // }
}
}
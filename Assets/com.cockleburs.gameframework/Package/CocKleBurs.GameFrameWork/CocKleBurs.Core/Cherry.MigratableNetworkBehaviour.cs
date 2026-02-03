using Netick;

namespace CockleBurs.GameFramework.Core
{
    public partial class NetworkCherry 
    { 

        /// <summary>
        /// NetworkAwake的密封实现，处理迁移相关的事件订阅
        /// </summary>
        public override void NetworkAwake()
        {
            BeginNetworkPlay();
            
        }

        /// <summary>
        /// NetworkStart的密封实现
        /// </summary>
        public sealed override void NetworkStart()
        {
            StartNetworkLogic();
        }

        /// <summary>
        /// NetworkUpdate的密封实现，迁移期间不更新
        /// </summary>
        public sealed override void NetworkUpdate()
        {
                NetTick();
        }

        /// <summary>
        /// NetworkFixedUpdate的密封实现，迁移期间不更新
        /// </summary>
        public sealed override void NetworkFixedUpdate()
        {
                NetFixedTick();
        }

        /// <summary>
        /// NetworkRender的密封实现，迁移期间不渲染
        /// </summary>
        public sealed override void NetworkRender()
        {
                NetRender();
        }

        /// <summary>
        /// NetworkDestroy的密封实现，处理迁移相关的事件取消订阅
        /// </summary>
        public sealed override void NetworkDestroy()
        {
            EndNetworkPlay();
            
        }

        /// <summary>
        /// 内部玩家离开事件处理，检查是否是输入源玩家离开
        /// </summary>
        private void OnPlayerLeftInternal(bool isDeadHost, NetworkPlayerId networkPlayerId, string puid)
        {
            // 调用派生类的玩家离开处理
            OnPlayerLeftSession(isDeadHost, networkPlayerId, puid);
            
        }
        
        /// <summary>
        /// <see cref="NetworkAwake"/>的替代方法。始终调用
        /// </summary>
        protected virtual void BeginNetworkPlay() { }

        /// <summary>
        /// <see cref="NetworkAwake"/>的替代方法。迁移后不调用
        /// </summary>
        protected virtual void OnSpawnedAwake() { }

        /// <summary>
        /// <see cref="NetworkAwake"/>的替代方法。仅在迁移后调用
        /// </summary>
        protected virtual void OnRestoredAwake() { }

        /// <summary>
        /// <see cref="NetworkStart"/>的替代方法。始终调用
        /// </summary>
        protected virtual void StartNetworkLogic() { }

        /// <summary>
        /// <see cref="NetworkStart"/>的替代方法。迁移后不调用
        /// </summary>
        protected virtual void OnSpawned() { }

        /// <summary>
        /// <see cref="NetworkStart"/>的替代方法。仅在迁移后调用
        /// </summary>
        protected virtual void OnRestored() { }

        /// <summary>
        /// <see cref="NetworkUpdate"/>的替代方法。迁移期间不调用
        /// </summary>
        protected virtual void NetTick() { }

        /// <summary>
        /// <see cref="NetworkFixedUpdate"/>的替代方法。迁移期间不调用
        /// </summary>
        protected virtual void NetFixedTick() { }

        /// <summary>
        /// <see cref="NetworkRender"/>的替代方法。迁移期间不调用
        /// </summary>
        protected virtual void NetRender() { }

        /// <summary>
        /// <see cref="NetworkDestroy"/>的替代方法。始终调用
        /// </summary>
        protected virtual void EndNetworkPlay() { }

        /// <summary>
        /// <see cref="NetworkDestroy"/>的替代方法。仅在非迁移期间调用
        /// </summary>
        protected virtual void OnDespawned() { }

        /// <summary>
        /// <see cref="NetworkDestroy"/>的替代方法。仅在迁移期间调用
        /// </summary>
        protected virtual void OnPreserved() { }

        /// <summary>
        /// 当玩家加入时调用
        /// </summary>
        protected virtual void OnPlayerJoinedSession(bool isNewPlayer, NetworkPlayerId networkPlayerId, string puid) { }

        /// <summary>
        /// 当玩家离开时调用
        /// </summary>
        protected virtual void OnPlayerLeftSession(bool isDeadHost, NetworkPlayerId networkPlayerId, string puid) { }

        /// <summary>
        /// 当此对象的输入源玩家离开时调用
        /// </summary>
        protected virtual void OnInputSourceLeft(bool isDeadHost, string puid) { }
    }
}
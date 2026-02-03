using System;
using System.Collections.Generic;
using System.Linq;
using Netick;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace CockleBurs.GameFramework.Core
{
    [System.Serializable]
    public enum NetworkActionType
    {
        ButtonClicked = 0,
        ToggleChanged = 1,
        SliderChanged = 2,
        ObjectDragged = 3,
        ProgressChanged = 4,
        TextInput = 5,
        DropdownSelected = 6,
        CustomAction = 7
    }

    [System.Serializable, Networked]
    public struct NetworkAction
    {
        public NetworkActionType Type { get; set; }
        [Networked]   public int Identifier { get; set; }     // UI元素唯一标识符
        [Networked]   public int IntData { get; set; }       // 整型数据（切换状态、进度值等）
        [Networked] public float FloatData { get; set; }  // 浮点数据
        public NetworkString32 StringData { get; set; } // 字符串数据
        [Networked]     public Vector3 VectorData { get; set; }// 位置、方向等
        [Networked]    public float Timestamp { get; set; }   // 时间戳

        public NetworkPlayerId RpcContext;
    }

    public class NetworkActionSystem : SandboxSingleton<NetworkActionSystem>
    {
        public static NetworkActionSystem Instance;
        
        [SerializeField] private bool _enableLogging = true;
        [SerializeField] private int _maxHistorySize = 100; // 历史记录最大容量
        
        // 注册表：存储UI元素与回调的映射
        [SerializeReference]
        private Dictionary<int, Action<NetworkAction, RpcContext>> _actionHandlers = new Dictionary<int, Action<NetworkAction, RpcContext>>();
        
        // 历史记录：存储最近的操作
        private readonly Queue<NetworkAction> _actionHistory = new Queue<NetworkAction>();
        
        // 事件：当接收到网络操作时触发
        public UnityEvent<NetworkAction> actionReceived = new UnityEvent<NetworkAction>();
        
        // 事件：当历史记录发生变化时触发
        public UnityEvent<List<NetworkAction>> historyUpdated = new UnityEvent<List<NetworkAction>>();
        

        /// <summary>
        /// 注册操作处理器
        /// </summary>
        public void RegisterHandler(int id, Action<NetworkAction, RpcContext> handler)
        {
            if (_actionHandlers.ContainsKey(id))
            {
                if (_enableLogging) Debug.LogWarning($"Handler for ID {id} already registered. Overwriting.");
                _actionHandlers[id] = handler;
            }
            else
            {
                _actionHandlers.Add(id, handler);
            }
        }
        
        /// <summary>
        /// 注销操作处理器
        /// </summary>
        public void UnregisterHandler(int id)
        {
            if (_actionHandlers.ContainsKey(id))
            {
                _actionHandlers.Remove(id);
            }
        }
        
        /// <summary>
        /// 获取操作处理器
        /// </summary>
        public Action<NetworkAction, RpcContext> GetHandler(int id)
        {
            if (_actionHandlers.TryGetValue(id, out var handler))
            {
                return handler;
            }
            
            if (_enableLogging) Debug.LogWarning($"No handler found for ID {id}");
            return null;
        }
        
        /// <summary>
        /// 检查是否有处理器
        /// </summary>
        public bool HasHandler(int id)
        {
            return _actionHandlers.ContainsKey(id);
        }

        /// <summary>
        /// 获取所有处理器ID
        /// </summary>
        public IEnumerable<int> GetAllHandlerIds()
        {
            return _actionHandlers.Keys;
        }
        
        /// <summary>
        /// 添加处理器（可链式调用）
        /// </summary>
        public void AddHandler(int id, Action<NetworkAction, RpcContext> additionalHandler)
        {
            if (!_actionHandlers.TryGetValue(id, out var existingHandler))
            {
                // 如果没有现有处理程序，直接注册新的
                RegisterHandler(id, additionalHandler);
                return;
            }
        
            // 创建新的处理程序链
            Action<NetworkAction, RpcContext> newHandler = (action, context) =>
            {
                // 调用原始处理程序
                existingHandler?.Invoke(action, context);
            
                // 调用新增处理程序
                additionalHandler(action, context);
            };
        
            // 更新注册
            RegisterHandler(id, newHandler);
        }
        
        /// <summary>
        /// 获取处理器的调用列表
        /// </summary>
        public Delegate[] GetHandlerInvocationList(int id)
        {
            var handler = GetHandler(id);
            return handler?.GetInvocationList();
        }
        
        [Rpc(source: RpcPeers.Everyone, target: RpcPeers.Owner, isReliable: true, localInvoke: false)]
        public void SendNetworkUIAction(NetworkAction action, RpcContext ctx = default)
        {
            
    
            ReceiveNetworkAction(action, ctx);
        }

        /// <summary>
        /// 发送网络操作
        /// </summary>
        public void SendNetworkAction(int id, NetworkActionType type, int intData = 0, float floatData = 0, 
                                    string stringData = "", Vector3 vectorData = default)
        {
            NetworkAction action = new NetworkAction
            {
                Identifier = id,
                Type = type,
                IntData = intData,
                FloatData = floatData,
                StringData = stringData,
                VectorData = vectorData,
                Timestamp = Time.time,
                
            };
            
            SendNetworkUIAction(action);
        }
        
        /// <summary>
        /// 接收网络操作
        /// </summary>
        public void ReceiveNetworkAction(NetworkAction action, RpcContext ctx)
        {
            if (action.Timestamp == 0)
            {
               
                action = new NetworkAction
                {
                    Type = action.Type,
                    Identifier = action.Identifier,
                    IntData = action.IntData,
                    FloatData = action.FloatData,
                    StringData = action.StringData,
                    VectorData = action.VectorData,
                    Timestamp = Time.time,
                    RpcContext = ctx.Source
                };
                if (ctx.Source == -1)
                    action.RpcContext = Sandbox.LocalPlayer.PlayerId; 
            }
            
            // 添加到历史记录
            AddToHistory(action);
            
            // 触发全局事件
            actionReceived?.Invoke(action);
            
            // 查找并调用特定处理程序
            if (_actionHandlers.TryGetValue(action.Identifier, out Action<NetworkAction, RpcContext> handler))
            {
                handler.Invoke(action, ctx);
            }
            else
            {
                if (_enableLogging) Debug.LogWarning($"No handler found for ID {action.Identifier}");
            }
        }
        
        #region History Management 历史记录管理
        
        /// <summary>
        /// 添加操作到历史记录
        /// </summary>
        private void AddToHistory(NetworkAction action)
        {
            _actionHistory.Enqueue(action);
            
            // 如果超过最大容量，移除最旧的操作
            if (_actionHistory.Count > _maxHistorySize)
            {
                _actionHistory.Dequeue();
            }
            
            // 触发历史记录更新事件
            historyUpdated?.Invoke(GetHistory());
        }
        
        /// <summary>
        /// 获取完整的历史记录（按时间顺序）
        /// </summary>
        public List<NetworkAction> GetHistory()
        {
            return _actionHistory.ToList();
        }
        
        /// <summary>
        /// 获取最近的历史记录
        /// </summary>
        /// <param name="count">获取的记录数量</param>
        public List<NetworkAction> GetRecentHistory(int count)
        {
            if (count <= 0) return new List<NetworkAction>();
            
            return _actionHistory
                .Reverse()
                .Take(Mathf.Min(count, _actionHistory.Count))
                .Reverse()
                .ToList();
        }
        
        /// <summary>
        /// 获取指定类型的历史记录
        /// </summary>
        public List<NetworkAction> GetHistoryByType(NetworkActionType type)
        {
            return _actionHistory
                .Where(action => action.Type == type)
                .ToList();
        }
        
        /// <summary>
        /// 获取指定标识符的历史记录
        /// </summary>
        public List<NetworkAction> GetHistoryById(int id)
        {
            return _actionHistory
                .Where(action => action.Identifier == id)
                .ToList();
        }
        
        /// <summary>
        /// 获取指定时间段内的历史记录
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        public List<NetworkAction> GetHistoryByTimeRange(float startTime, float endTime)
        {
            return _actionHistory
                .Where(action => action.Timestamp >= startTime && action.Timestamp <= endTime)
                .ToList();
        }
        
        /// <summary>
        /// 清除历史记录
        /// </summary>
        public void ClearHistory()
        {
            _actionHistory.Clear();
            historyUpdated?.Invoke(new List<NetworkAction>());
        }
        
        /// <summary>
        /// 获取历史记录数量
        /// </summary>
        public int GetHistoryCount()
        {
            return _actionHistory.Count;
        }
        
        /// <summary>
        /// 设置历史记录最大容量
        /// </summary>
        public void SetMaxHistorySize(int size)
        {
            if (size < 1)
            {
                Debug.LogWarning("History size must be at least 1");
                return;
            }
            
            _maxHistorySize = size;
            
            // 如果当前历史记录超过新的大小，移除最旧的操作
            while (_actionHistory.Count > _maxHistorySize)
            {
                _actionHistory.Dequeue();
            }
            
            historyUpdated?.Invoke(GetHistory());
        }
        
        /// <summary>
        /// 获取历史记录的最大容量
        /// </summary>
        public int GetMaxHistorySize()
        {
            return _maxHistorySize;
        }
        
        /// <summary>
        /// 查找最近发生的特定操作
        /// </summary>
        /// <param name="id">操作标识符</param>
        /// <returns>找到的操作，如果没有则返回null</returns>
        public NetworkAction? FindRecentActionById(int id)
        {
            return _actionHistory
                .Reverse()
                .FirstOrDefault(action => action.Identifier == id);
        }
        
        /// <summary>
        /// 检查指定操作是否在历史记录中
        /// </summary>
        public bool ContainsInHistory(NetworkAction action)
        {
            return _actionHistory.Contains(action);
        }
        
        /// <summary>
        /// 获取历史记录的统计信息
        /// </summary>
        public Dictionary<NetworkActionType, int> GetHistoryStatistics()
        {
            var stats = new Dictionary<NetworkActionType, int>();
            
            foreach (var action in _actionHistory)
            {
                if (stats.ContainsKey(action.Type))
                {
                    stats[action.Type]++;
                }
                else
                {
                    stats[action.Type] = 1;
                }
            }
            
            return stats;
        }
        
        /// <summary>
        /// 导出历史记录为可读字符串
        /// </summary>
        public string ExportHistoryAsString()
        {
            var history = GetHistory();
            var result = $"Network Action History (Count: {history.Count})\n";
            result += "=================================\n";
            
            foreach (var action in history)
            {
                result += $"[{action.Timestamp:F2}] ID: {action.Identifier}, Type: {action.Type}\n";
                result += $"  Int: {action.IntData}, Float: {action.FloatData}, String: {action.StringData}\n";
                result += $"  Vector: {action.VectorData}\n";
            }
            
            return result;
        }
        
        #endregion
    }
}
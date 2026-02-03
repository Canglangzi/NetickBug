// using System;
// using System.Collections.Generic;
// using Netick.Unity;
// using UnityEngine;
// using Object = UnityEngine.Object;
//
//
// namespace CockleBurs.GameFramework.Core
// {
// // 添加的对象池持久化模式枚举
// public enum PoolPersistMode
// {
//     AutoRebuild,    // 场景卸载后自动重建
//     DontDestroy     // 跨场景持久化
// }
//
// public struct GameObjectPool : IDisposable 
// {
//     // 事件委托定义
//     public delegate void PoolEventDelegate(GameObject obj);
//     
//     // 池事件
//     public event PoolEventDelegate OnObjectSpawned;
//     public event PoolEventDelegate OnObjectDespawned;
//
//     private GameObject _prefab;
//     private Queue<GameObject> _inactiveObjects;
//     private Queue<GameObject> _activeObjects;
//     private Dictionary<GameObject, IPoolable[]> _cachedPoolables;
//     private int _maxSize;
//     private Transform _poolParent;
//     private int _spawnedCount;
//     private bool _isDisposed; // 销毁状态标志
//     private PoolPersistMode _persistMode; // 新增的持久化模式字段
//     
//     public string PoolName { get; private set; }
//     public PoolPersistMode PersistMode => _persistMode; // 公开只读属性
//
//     public World World;
//     public GameObjectPool(World world,GameObject prefab, int maxSize = 0, 
//                           string poolName = "Pool", Transform parent = null,
//                           PoolPersistMode persistMode = PoolPersistMode.AutoRebuild) : this()
//     {
//         if (prefab == null)
//         {
//             throw new System.ArgumentNullException("prefab cannot be null");
//         }
//
//         World = world;
//         _prefab = prefab;
//         _maxSize = maxSize;
//         _isDisposed = false;
//         _persistMode = persistMode; // 设置持久化模式
//         PoolName = $"{poolName}[{prefab.name}]";
//         
//         // 创建父物体
//         if (parent != null)
//         {
//             _poolParent = parent;
//         }
//         else
//         {
//             var poolParentGo = new GameObject($"{prefab.name}_Pool");
//             _poolParent = poolParentGo.transform;
//             
//             // 持久化模式处理
//             if (persistMode == PoolPersistMode.DontDestroy && Application.isPlaying)
//             {
//                 // 标记为跨场景不销毁
//                 Object.DontDestroyOnLoad(poolParentGo);
//             }
//         }
//
//         _inactiveObjects = new Queue<GameObject>(maxSize);
//         _activeObjects = new Queue<GameObject>(maxSize);
//         _cachedPoolables = new Dictionary<GameObject, IPoolable[]>();
//         _spawnedCount = 0;
//
//         // 预创建对象并缓存组件
//         for (int i = 0; i < maxSize; i++)
//         {
//             GameObject obj = CreateNewPoolObject();
//             _inactiveObjects.Enqueue(obj);
//         }
//         
//         // 订阅场景操作事件
//         if (world != null && world.Events != null)
//         {
//             world.Events.OnSceneOperationBegan += EventsOnOnSceneOperationBegan;
//             world.Events.OnSceneOperationDone += EventsOnOnSceneOperationDone;
//         }
//     }
//
//     private void EventsOnOnSceneOperationDone(NetworkSandbox sandbox, NetworkSceneOperation sceneoperation)
//     {
//         if (_persistMode != PoolPersistMode.AutoRebuild) return;
//         RebuildPool();
//     }
//
//     // 场景操作开始事件处理 - 实现重建逻辑
//     private void EventsOnOnSceneOperationBegan(NetworkSandbox sandbox, NetworkSceneOperation sceneOperation)
//     {
//         // 只处理AutoRebuild模式
//         if (_persistMode != PoolPersistMode.AutoRebuild) return;
//         ReturnAllActiveObjects();
//     }
//
//     public void Dispose()
//     {
//         if (_isDisposed) return;
//         
//         // 取消事件订阅
//         if (World != null && World.Events != null)
//         {
//             World.Events.OnSceneOperationBegan -= EventsOnOnSceneOperationBegan;
//             World.Events.OnSceneOperationDone -= EventsOnOnSceneOperationDone;
//         }
//         
//         // 清理所有对象
//         ClearPool(true);
//         
//         // 只有AutoRebuild模式才销毁父物体
//         if (_persistMode == PoolPersistMode.AutoRebuild && 
//             _poolParent != null && 
//             _poolParent.name.EndsWith("_Pool"))
//         {
//             Object.DestroyImmediate(_poolParent.gameObject);
//             // if (Application.isPlaying)
//             // {
//             //     World.Destroy(_poolParent.gameObject);
//             // }
//             // else
//             // {
//             //     Object.DestroyImmediate(_poolParent.gameObject);
//             // }
//         }
//         else
//         {
//             // DontDestroy模式：只重置父物体但不销毁
//             if (_poolParent != null)
//             {
//                 _poolParent.position = Vector3.zero;
//                 _poolParent.rotation = Quaternion.identity;
//             }
//         }
//         
//         // 清理事件订阅
//         OnObjectSpawned = null;
//         OnObjectDespawned = null;
//         
//         // 清空集合
//         _inactiveObjects.Clear();
//         _activeObjects.Clear();
//         _cachedPoolables.Clear();
//         
//         _isDisposed = true;
//     }
//
//     // 检查对象池是否已销毁
//     private void CheckDisposed()
//     {
//         if (_isDisposed)
//         {
//             throw new System.ObjectDisposedException($"GameObjectPool '{PoolName}' has been disposed");
//         }
//     }
//
//     public int ActiveCount => _activeObjects.Count;
//     public int InactiveCount => _inactiveObjects.Count;
//     public int TotalCount => ActiveCount + InactiveCount;
//
//     public GameObject GetPooledObject(Vector3 position, Quaternion rotation)
//     {
//         CheckDisposed(); // 检查是否已销毁
//         
//         GameObject obj = null;
//
//         // 1. 尝试从空闲池获取对象
//         if (_inactiveObjects.Count > 0)
//         {
//             obj = _inactiveObjects.Dequeue();
//         }
//         // 2. 尝试创建新对象（如果允许）
//         else if (CanCreateNewObject())
//         {
//             obj = CreateNewPoolObject();
//         }
//         // 3. 尝试回收最早的对象（如果允许）
//         else if (ShouldRecycleOldest())
//         {
//             obj = RecycleOldestActiveObject();
//         }
//
//         if (obj != null)
//         {
//             PrepareObject(obj, position, rotation);
//             _activeObjects.Enqueue(obj);
//         }
//
//         return obj;
//     }
//
//     public void ReturnToPool(GameObject obj)
//     {
//         CheckDisposed(); // 检查是否已销毁
//         
//         if (obj == null) return;
//         
//         // 确保对象属于此池
//         if (!_cachedPoolables.ContainsKey(obj))
//         {
//             Debug.LogWarning($"尝试归还不属于此池的对象: {obj.name} (池: {PoolName})");
//             return;
//         }
//
//         ResetObjectState(obj);
//         
//         // 池满时销毁而非回收
//         if (_maxSize > 0 && _inactiveObjects.Count >= _maxSize)
//         {
//             DestroyPoolObject(obj);
//             return;
//         }
//         
//         _inactiveObjects.Enqueue(obj);
//         
//         // 触发回收事件
//         OnObjectDespawned?.Invoke(obj);
//     }
//
//     // 清空整个对象池
//     public void ClearPool(bool includeActive = false)
//     {
//         CheckDisposed(); // 检查是否已销毁
//         
//         // 清除非活跃对象
//         while (_inactiveObjects.Count > 0)
//         {
//             var obj = _inactiveObjects.Dequeue();
//             DestroyPoolObject(obj);
//         }
//
//         if (includeActive)
//         {
//             // 清空活跃对象
//             while (_activeObjects.Count > 0)
//             {
//                 var obj = _activeObjects.Dequeue();
//                 DestroyPoolObject(obj);
//             }
//         }
//     }
//
//     private bool CanCreateNewObject()
//     {
//         return _maxSize == 0 || TotalCount < _maxSize;
//     }
//
//     private bool ShouldRecycleOldest()
//     {
//         return _maxSize > 0 && _activeObjects.Count > 0;
//     }
//
//     private GameObject RecycleOldestActiveObject()
//     {
//         if (_activeObjects.Count == 0) return null;
//         
//         var oldest = _activeObjects.Dequeue();
//         ResetObjectState(oldest);
//         return oldest;
//     }
//
//     private GameObject CreateNewPoolObject()
//     {
//         GameObject newObj = World.Instantiate(_prefab, _poolParent);
//         _spawnedCount++;
//         newObj.name = $"{_prefab.name}_{_spawnedCount:000}";
//         
//         // 缓存IPoolable组件
//         CachePoolableComponents(newObj);
//         
//         ResetObjectState(newObj);
//         return newObj;
//     }
//
//     private void CachePoolableComponents(GameObject obj)
//     {
//         var poolables = obj.GetComponents<IPoolable>();
//         _cachedPoolables[obj] = poolables;
//     }
//
//     private void DestroyPoolObject(GameObject obj)
//     {
//         // 清理缓存
//         if (_cachedPoolables.ContainsKey(obj))
//         {
//             _cachedPoolables.Remove(obj);
//         }
//         
//         if (obj != null)
//         {
//             Object.DestroyImmediate(obj);
//             // if (Application.isPlaying)
//             // {
//             // //    World.Destroy(obj,true);
//             // // .Destroy(obj,true)
//             // }
//             // else
//             // {
//             //     Object.DestroyImmediate(obj);
//             // }
//         }
//     }
//
//     private void ResetObjectState(GameObject obj)
//     {
//         // 物理状态重置
//         if (obj.TryGetComponent<Rigidbody>(out var rb))
//         {
//             rb.linearVelocity = Vector3.zero;
//             rb.angularVelocity = Vector3.zero;
//         }
//
//         // 对象禁用和复位
//         obj.SetActive(false);
//         obj.transform.SetParent(_poolParent);
//         obj.transform.localPosition = Vector3.zero;
//         obj.transform.localRotation = Quaternion.identity;
//         obj.transform.localScale = Vector3.one;
//
//         // 使用缓存的IPoolable组件
//         if (_cachedPoolables.TryGetValue(obj, out var poolables))
//         {
//             foreach (var poolable in poolables)
//             {
//                 poolable.OnDespawn();
//             }
//         }
//     }
//
//     private void PrepareObject(GameObject obj, Vector3 position, Quaternion rotation)
//     {
//         // 物理位置设置
//         if (obj.TryGetComponent<Rigidbody>(out var rb))
//         {
//             rb.position = position;
//             rb.rotation = rotation;
//         }
//         else
//         {
//             obj.transform.position = position;
//             obj.transform.rotation = rotation;
//         }
//
//         obj.SetActive(true);
//         
//         // 使用缓存的IPoolable组件
//         if (_cachedPoolables.TryGetValue(obj, out var poolables))
//         {
//             foreach (var poolable in poolables)
//             {
//                 poolable.OnSpawn();
//             }
//         }
//         
//         // 触发取出事件
//         OnObjectSpawned?.Invoke(obj);
//     }
//
//     // 重建对象池（用于AutoRebuild模式）
//     public void RebuildPool()
//     {
//         CheckDisposed();
//         
//         if (_persistMode != PoolPersistMode.AutoRebuild)
//         {
//             Debug.LogWarning($"尝试重建非AutoRebuild模式的对象池: {PoolName}");
//             return;
//         }
//
//         // 确保父物体存在
//         if (_poolParent == null)
//         {
//             var poolParentGo = new GameObject($"{_prefab.name}_RebuiltPool");
//             _poolParent = poolParentGo.transform;
//         }
//
//         // 清除现有对象
//         ClearPool(true);
//         
//         // 重新初始化对象池
//         _inactiveObjects = new Queue<GameObject>(_maxSize);
//         _activeObjects = new Queue<GameObject>(_maxSize);
//         _cachedPoolables = new Dictionary<GameObject, IPoolable[]>();
//         _spawnedCount = 0;
//         
//         for (int i = 0; i < _maxSize; i++)
//         {
//             GameObject obj = CreateNewPoolObject();
//             _inactiveObjects.Enqueue(obj);
//         }
//         
//         Debug.Log($"对象池重建完成: {PoolName} (大小: {_maxSize})");
//     }
//
//     // 处理场景切换
//     public void HandleSceneChange()
//     {
//         CheckDisposed();
//         
//         switch (_persistMode)
//         {
//             case PoolPersistMode.AutoRebuild:
//                 // 对于AutoRebuild模式，返回所有活动对象并重置
//                 ReturnAllActiveObjects();
//                 break;
//                 
//             case PoolPersistMode.DontDestroy:
//                 // DontDestroy模式：只需更新父物体状态
//                 if (_poolParent != null)
//                 {
//                     _poolParent.SetParent(null);
//                 }
//                 break;
//         }
//     }
//
//     private void ReturnAllActiveObjects()
//     {
//         var tempList = new List<GameObject>(_activeObjects);
//         foreach (var obj in tempList)
//         {
//             if (obj != null && obj.activeSelf)
//             {
//                 ReturnToPool(obj);
//             }
//         }
//         _activeObjects.Clear();
//     }
// }
//
// // 对象池行为接口
// public interface IPoolable
// {
//     void OnSpawn();    // 对象被取出时调用
//     void OnDespawn();  // 对象被回收时调用
// }
// }
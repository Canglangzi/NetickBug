using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Netick;
using Netick.Unity;
using UnityEngine;

namespace CockleBurs.GameFramework.Core
{
    /// <summary>
    /// 操作工具类，提供各种通用操作和错误处理
    /// </summary>
    public static class OperationUtility
    {
        #region 错误处理和重试
        /// <summary>
        /// 尝试执行操作并捕获异常
        /// </summary>
        /// <param name="action">要执行的操作</param>
        /// <param name="operationName">操作名称（用于日志）</param>
        /// <param name="silent">是否静默处理异常（不记录错误）</param>
        /// <returns>是否成功执行</returns>
        public static bool TryRun(Action action, string operationName = null, bool silent = false)
        {
            try
            {
                action?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                if (!silent)
                {
                    Debug.LogError($"操作失败: {operationName ?? "未命名操作"}, 错误: {ex.Message}");
                }
                return false;
            }
        }

        /// <summary>
        /// 尝试执行操作并返回结果
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="func">要执行的函数</param>
        /// <param name="defaultValue">默认值</param>
        /// <param name="operationName">操作名称</param>
        /// <param name="silent">是否静默处理异常</param>
        /// <returns>函数结果或默认值</returns>
        public static T TryRun<T>(Func<T> func, T defaultValue = default, string operationName = null, bool silent = false)
        {
            try
            {
                return func != null ? func.Invoke() : defaultValue;
            }
            catch (Exception ex)
            {
                if (!silent)
                {
                    Debug.LogError($"操作失败: {operationName ?? "未命名操作"}, 错误: {ex.Message}");
                }
                return defaultValue;
            }
        }

        /// <summary>
        /// 异步尝试执行操作
        /// </summary>
        public static async Task<bool> TryRunAsync(Func<Task> asyncAction, string operationName = null, bool silent = false)
        {
            try
            {
                if (asyncAction != null)
                {
                    await asyncAction.Invoke();
                }
                return true;
            }
            catch (Exception ex)
            {
                if (!silent)
                {
                    Debug.LogError($"异步操作失败: {operationName ?? "未命名操作"}, 错误: {ex.Message}");
                }
                return false;
            }
        }

        /// <summary>
        /// 异步尝试执行操作并返回结果
        /// </summary>
        public static async Task<T> TryRunAsync<T>(Func<Task<T>> asyncFunc, T defaultValue = default, string operationName = null, bool silent = false)
        {
            try
            {
                return asyncFunc != null ? await asyncFunc.Invoke() : defaultValue;
            }
            catch (Exception ex)
            {
                if (!silent)
                {
                    Debug.LogError($"异步操作失败: {operationName ?? "未命名操作"}, 错误: {ex.Message}");
                }
                return defaultValue;
            }
        }

        /// <summary>
        /// 重试操作直到成功或达到最大重试次数
        /// </summary>
        /// <param name="action">要重试的操作</param>
        /// <param name="operationName">操作名称</param>
        /// <param name="maxRetries">最大重试次数</param>
        /// <param name="retryDelay">重试延迟（秒）</param>
        /// <param name="onRetry">每次重试时的回调</param>
        /// <returns>是否最终成功</returns>
        public static bool RetryUntil(Action action, string operationName = null, int maxRetries = 3, float retryDelay = 1f, Action<int> onRetry = null)
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                if (TryRun(action, $"{operationName} (尝试 {attempt}/{maxRetries})", silent: attempt < maxRetries))
                {
                    return true;
                }

                if (attempt < maxRetries)
                {
                    onRetry?.Invoke(attempt);
                    
                    if (retryDelay > 0)
                    {
                        var delayTask = Task.Delay(TimeSpan.FromSeconds(retryDelay));
                        delayTask.Wait();
                    }
                }
            }

            Debug.LogError($"操作失败: {operationName}，已达到最大重试次数 {maxRetries}");
            return false;
        }

        /// <summary>
        /// 异步重试操作直到成功
        /// </summary>
        public static async Task<bool> RetryUntilAsync(Func<Task<bool>> asyncFunc, string operationName = null, int maxRetries = 3, float retryDelay = 1f)
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                bool success = await TryRunAsync(
                    asyncFunc, 
                    defaultValue: false, 
                    operationName: $"{operationName} (尝试 {attempt}/{maxRetries})",
                    silent: attempt < maxRetries
                );

                if (success) return true;

                if (attempt < maxRetries && retryDelay > 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(retryDelay));
                }
            }

            Debug.LogError($"异步操作失败: {operationName}，已达到最大重试次数 {maxRetries}");
            return false;
        }
        #endregion

        #region 协程操作
        /// <summary>
        /// 启动安全的协程
        /// </summary>
        public static Coroutine StartSafeCoroutine(this MonoBehaviour mono, IEnumerator coroutine, Action onComplete = null, Action<Exception> onError = null)
        {
            if (mono == null || coroutine == null) return null;
            
            return mono.StartCoroutine(SafeCoroutineWrapper(coroutine, onComplete, onError));
        }

        private static IEnumerator SafeCoroutineWrapper(IEnumerator coroutine, Action onComplete, Action<Exception> onError)
        {
            while (true)
            {
                try
                {
                    if (!coroutine.MoveNext())
                    {
                        onComplete?.Invoke();
                        yield break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"协程执行错误: {ex.Message}");
                    onError?.Invoke(ex);
                    yield break;
                }

                yield return coroutine.Current;
            }
        }

        /// <summary>
        /// 停止安全的协程
        /// </summary>
        public static void StopSafeCoroutine(this MonoBehaviour mono, Coroutine coroutine)
        {
            if (mono != null && coroutine != null)
            {
                mono.StopCoroutine(coroutine);
            }
        }
        #endregion

        #region 对象操作
        /// <summary>
        /// 安全销毁GameObject
        /// </summary>
        public static void SafeDestroy(GameObject gameObject)
        {
            if (gameObject == null) return;
            
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(gameObject);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        /// <summary>
        /// 安全销毁组件
        /// </summary>
        public static void SafeDestroy(Component component)
        {
            if (component == null) return;
            SafeDestroy(component.gameObject);
        }

        /// <summary>
        /// 获取或添加组件
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject == null) return null;
            
            T component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }
            return component;
        }

        /// <summary>
        /// 查找子对象（包括非激活的）
        /// </summary>
        public static Transform FindDeepChild(this Transform parent, string childName, bool includeInactive = true)
        {
            if (parent == null || string.IsNullOrEmpty(childName)) return null;
            
            // 先检查当前层级
            if (parent.name == childName) return parent;
            
            // 递归查找子对象
            foreach (Transform child in parent)
            {
                if (!includeInactive && !child.gameObject.activeSelf) continue;
                
                Transform result = child.FindDeepChild(childName, includeInactive);
                if (result != null) return result;
            }
            
            return null;
        }
        #endregion

        #region 网络和迁移相关
        /// <summary>
        /// 安全地等待网络操作
        /// </summary>
        public static async Task<bool> WaitForNetworkCondition(Func<bool> condition, float timeoutSeconds = 10f, float checkInterval = 0.1f)
        {
            float startTime = Time.time;
            
            while (Time.time - startTime < timeoutSeconds)
            {
                if (condition?.Invoke() == true)
                {
                    return true;
                }
                
                await Task.Delay(TimeSpan.FromSeconds(checkInterval));
            }
            
            return false;
        }

        /// <summary>
        /// 执行主机迁移操作
        /// </summary>
        public static async Task<bool> ExecuteHostMigration(Func<Task<bool>> migrationAction, Action<float> onProgress = null)
        {
            try
            {
                Debug.Log("开始主机迁移操作...");
                
                // 步骤1: 验证网络状态
                onProgress?.Invoke(0.1f);
                if (!NetworkUtility.IsNetworkReady())
                {
                    Debug.LogError("网络未准备好，无法进行主机迁移");
                    return false;
                }

                // 步骤2: 执行迁移操作
                onProgress?.Invoke(0.3f);
                bool success = await TryRunAsync(migrationAction, operationName: "主机迁移操作");
                
                if (!success)
                {
                    Debug.LogError("主机迁移操作失败");
                    return false;
                }

                // 步骤3: 验证迁移结果
                onProgress?.Invoke(0.7f);
                await Task.Delay(TimeSpan.FromSeconds(1)); // 等待网络稳定
                
                if (!NetworkUtility.IsNetworkReady())
                {
                    Debug.LogError("迁移后网络状态异常");
                    return false;
                }

                onProgress?.Invoke(1f);
                Debug.Log("主机迁移操作完成");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"主机迁移过程中发生错误: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 安全地处理网络对象引用
        /// </summary>
        public static NetworkObjectRef SafeGetNetworkObjectRef(NetworkObject networkObject)
        {
            if (networkObject != null)
            {
                return new NetworkObjectRef(networkObject);
            }
            return new NetworkObjectRef();
        }

        /// <summary>
        /// 安全地设置网络属性
        /// </summary>
        public static bool SafeSetNetworkedProperty(object instance, string propertyName, object value)
        {
            return TryRun(() =>
            {
                if (instance == null) return;
                
                var property = instance.GetType().GetProperty(propertyName, 
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                
                if (property != null && property.CanWrite)
                {
                    property.SetValue(instance, value);
                }
                else
                {
                    var field = instance.GetType().GetField(propertyName,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    
                    if (field != null)
                    {
                        field.SetValue(instance, value);
                    }
                }
            }, operationName: $"设置网络属性 {propertyName}");
        }
        #endregion

        #region 日志和调试
        /// <summary>
        /// 记录迁移操作的调试信息
        /// </summary>
        public static void LogMigrationOperation(string operation, object data = null, bool isError = false)
        {
            string message = $"[主机迁移] {operation}";
            
            if (data != null)
            {
                message += $"\n数据: {JsonUtility.ToJson(data, true)}";
            }
            
            if (isError)
            {
                Debug.LogError(message);
            }
            else
            {
                Debug.Log(message);
            }
        }
        
        #endregion

        #region 实用工具方法
        /// <summary>
        /// 安全地执行延迟操作
        /// </summary>
        public static Coroutine ExecuteDelayed(this MonoBehaviour mono, Action action, float delay, bool unscaledTime = false)
        {
            return mono.StartSafeCoroutine(DelayedCoroutine(action, delay, unscaledTime));
        }

        private static IEnumerator DelayedCoroutine(Action action, float delay, bool unscaledTime)
        {
            if (unscaledTime)
            {
                yield return new WaitForSecondsRealtime(delay);
            }
            else
            {
                yield return new WaitForSeconds(delay);
            }
            
            TryRun(action, "延迟操作");
        }

        /// <summary>
        /// 生成唯一的ID
        /// </summary>
        public static string GenerateUniqueId(string prefix = "")
        {
            return $"{prefix}{Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8)}";
        }

        /// <summary>
        /// 安全地转换类型
        /// </summary>
        public static T SafeCast<T>(object value, T defaultValue = default)
        {
            try
            {
                if (value is T result)
                {
                    return result;
                }
                
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 检查对象是否为默认值
        /// </summary>
        public static bool IsDefaultValue<T>(T value)
        {
            return EqualityComparer<T>.Default.Equals(value, default);
        }

        /// <summary>
        /// 深拷贝对象
        /// </summary>
        public static T DeepCopy<T>(T obj)
        {
            if (obj == null) return default;
            
            try
            {
                // 对于Unity对象，使用JsonUtility
                if (obj is UnityEngine.Object)
                {
                    string json = JsonUtility.ToJson(obj);
                    return JsonUtility.FromJson<T>(json);
                }
                
                // 对于可序列化对象
                var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                using (var stream = new System.IO.MemoryStream())
                {
                    formatter.Serialize(stream, obj);
                    stream.Seek(0, System.IO.SeekOrigin.Begin);
                    return (T)formatter.Deserialize(stream);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"深拷贝失败: {ex.Message}");
                return default;
            }
        }
        #endregion
    }

    /// <summary>
    /// 网络工具类
    /// </summary>
    public static class NetworkUtility
    {
        /// <summary>
        /// 检查网络是否就绪
        /// </summary>
        public static bool IsNetworkReady()
        {
            try
            {
                // 这里根据您的网络框架实现检查逻辑
                return Network.IsRunning;
            }
            catch
            {
                return false;
            }
        }
        
    }
}
using System.Collections.Generic;
using Netick;
using Netick.Unity;
using UnityEngine;
using UnityEngine.Events;


namespace CockleBurs.GameFramework.Core
{
public abstract class SandboxSingleton<T> : NetworkCherry where T : SandboxSingleton<T>
{
    private static readonly Dictionary<NetworkSandbox, T> Instances = new Dictionary<NetworkSandbox, T>();
    
    /// <summary>
    /// 获取当前沙盒的单例实例
    /// </summary>
    public T Instance
    {
        get
        {
            // 获取当前沙盒
            if (Sandbox == null) return null;
            
            return GetInstanceForSandbox(Sandbox);
        }
    }
    
    /// <summary>
    /// 获取指定沙盒的单例实例
    /// </summary>
    public static T GetInstanceForSandbox(NetworkSandbox sandbox)
    {
        if (sandbox == null)
        {
            Debug.LogError("NetworkSandbox cannot be null!");
            return null;
        }

        // 先从缓存中获取
        if (Instances.TryGetValue(sandbox, out T cachedInstance))
        {
            if (cachedInstance != null)
                return cachedInstance;
        
            // 如果缓存中的实例为null，从字典中移除
            Instances.Remove(sandbox);
        }

        T instance = null;

        // 优先查找挂载在sandbox GameObject上的组件
        instance = sandbox.GetComponent<T>();
        if (instance != null)
        {
            Instances[sandbox] = instance;
            return instance;
        }

        // 在整个sandbox中查找
        instance = sandbox.FindObjectOfType<T>();
        if (instance != null)
        {
            Instances[sandbox] = instance;
            return instance;
        }

        // 如果仍然没找到，记录警告但不返回null，因为调用者可能处理null
        Debug.LogWarning($"No instance of {typeof(T).Name} found in sandbox {sandbox.name}");
        return null;
    }
    
    /// <summary>
    /// 创建新的单例实例
    /// </summary>
    public static T CreateInstanceForSandbox(NetworkSandbox sandbox)
    {
        if (sandbox == null)
        {
            Debug.LogError($"[SandboxSingleton<{typeof(T).Name}>] Cannot create instance: sandbox is null.");
            return null;
        }
        
        // 如果已有实例，直接返回
        var existing = GetInstanceForSandbox(sandbox);
        if (existing != null) return existing;
        
        Debug.Log($"[SandboxSingleton<{typeof(T).Name}>] Creating new instance for sandbox: {sandbox.Name}");
        
        var go = new GameObject($"{typeof(T).Name}_Singleton");
        var instance = go.AddComponent<T>();
        
        Instances[sandbox] = instance;
        return instance;
    }

    protected override void BeginNetworkPlay()
    {
        SingletonAwake();
        SingletonStart();
    }
    protected virtual void SingletonStart()
    {
           
    }
    protected override void EndNetworkPlay()
    {
        OnDestroy();
        SingletonDestroy();
    }
    protected virtual void  SingletonDestroy()
    {
           
    }
    protected virtual void SingletonAwake()
    {
        // 获取当前对象所属沙盒
        var sandbox = Sandbox as NetworkSandbox;
        if (sandbox == null)
        {
            Debug.LogError($"[SandboxSingleton<{typeof(T).Name}>] No sandbox found for {typeof(T).Name}");
            return;
        }
        
        // 检查是否已有该沙盒的实例
        if (Instances.TryGetValue(sandbox, out var existing) && existing != this)
        {
            Debug.LogWarning($"[SandboxSingleton<{typeof(T).Name}>] Multiple instances detected in sandbox: {sandbox.Name}. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        
        // 注册为当前沙盒的单例
        Instances[sandbox] = this as T;
    }
    
    protected virtual void OnDestroy()
    {
        // 获取当前对象所属沙盒
        var sandbox = Sandbox as NetworkSandbox;
        if (sandbox == null) return;
        
        // 从字典中移除
        if (Instances.TryGetValue(sandbox, out var instance) && instance == this)
        {
            Instances.Remove(sandbox);
        }
    }
    
    /// <summary>
    /// 当沙盒被销毁时清理实例
    /// </summary>
    public static void CleanupForSandbox(NetworkSandbox sandbox)
    {
        if (sandbox != null && Instances.ContainsKey(sandbox))
        {
            Instances.Remove(sandbox);
        }
    }
}
}
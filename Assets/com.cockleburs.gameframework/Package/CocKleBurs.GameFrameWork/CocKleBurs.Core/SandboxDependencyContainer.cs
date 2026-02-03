using System;
using System.Collections.Generic;
using System.Reflection;
using Netick.Unity;
using UnityEngine;


namespace CockleBurs.GameFramework.Core
{
/// <summary>
/// 服务生命周期类型
/// </summary>
public enum ServiceLifetime
{
    /// <summary> 单例，全局唯一 </summary>
    Singleton,
    
    /// <summary> 作用域内单例（默认每个沙盒一个实例） </summary>
    Scoped,
    
    /// <summary> 瞬态，每次请求都创建新实例 </summary>
    Transient
}

public class SandboxDependencyContainer : SandboxSingleton<SandboxDependencyContainer>
{
    // 服务注册表
    private readonly Dictionary<Type, ServiceDescriptor> _services = new Dictionary<Type, ServiceDescriptor>();
    
    // 单例实例存储
    private readonly Dictionary<Type, object> _singletonInstances = new Dictionary<Type, object>();
    
    // 作用域实例存储（每个沙盒独立）
    private readonly Dictionary<Type, object> _scopedInstances = new Dictionary<Type, object>();
    

    protected override void SingletonDestroy()
    {
        DisposeServices(_singletonInstances.Values);
        DisposeServices(_scopedInstances.Values);
        
        _services.Clear();
        _singletonInstances.Clear();
        _scopedInstances.Clear();
        
        Debug.Log($"[DI] Dependency container destroyed for sandbox: {Sandbox.Name}");
    }

    private void DisposeServices(IEnumerable<object> services)
    {
        foreach (var service in services)
        {
            if (service is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    /// <summary>
    /// 注册服务到依赖容器
    /// </summary>
    public void Register<TService, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TImplementation : TService
    {
        Register(typeof(TService), typeof(TImplementation), lifetime);
    }

    /// <summary>
    /// 注册服务实例到依赖容器
    /// </summary>
    public void RegisterInstance<TService>(TService instance, ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        var serviceType = typeof(TService);
        if (_services.ContainsKey(serviceType))
        {
            Debug.LogWarning($"[DI] Service {serviceType.Name} already registered. Overwriting.");
        }

        _services[serviceType] = new ServiceDescriptor(serviceType, instance.GetType(), lifetime);
        
        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                _singletonInstances[serviceType] = instance;
                break;
            case ServiceLifetime.Scoped:
                _scopedInstances[serviceType] = instance;
                break;
            case ServiceLifetime.Transient:
                throw new InvalidOperationException("Cannot register transient service as instance");
        }
        
        Debug.Log($"[DI] Registered instance: {serviceType.Name} => {instance.GetType().Name} ({lifetime})");
    }

    private void Register(Type serviceType, Type implementationType, ServiceLifetime lifetime)
    {
        if (_services.ContainsKey(serviceType))
        {
            Debug.LogWarning($"[DI] Service {serviceType.Name} already registered. Overwriting.");
        }

        _services[serviceType] = new ServiceDescriptor(serviceType, implementationType, lifetime);
        Debug.Log($"[DI] Registered: {serviceType.Name} => {implementationType.Name} ({lifetime})");
    }

    /// <summary>
    /// 解析服务实例
    /// </summary>
    public TService Resolve<TService>()
    {
        return (TService)Resolve(typeof(TService));
    }
   public void InjectDependencies(object instance)
{
    if (instance == null)
    {
        Debug.LogWarning("[DI] Cannot inject dependencies into null instance");
        return;
    }

    var type = instance.GetType();
    Debug.Log($"[DI] Starting dependency injection for {type.Name}");
    
    // 1. 方法注入（优先）
    bool methodInjected = InjectViaMethod(instance, type);
    
    // 2. 如果没有方法注入，尝试属性注入
    if (!methodInjected)
    {
        Debug.Log($"[DI] No method injection found for {type.Name}, trying property injection");
        InjectViaProperties(instance, type);
    }
    else
    {
        Debug.Log($"[DI] Method injection completed for {type.Name}");
    }
    
    // 3. 最后尝试字段注入
    if (!methodInjected)
    {
        Debug.Log($"[DI] Trying field injection for {type.Name}");
        InjectViaFields(instance, type);
    }
    
    Debug.Log($"[DI] Dependency injection completed for {type.Name}");
}

private bool InjectViaMethod(object instance, Type type)
{
    bool injected = false;
    
    Debug.Log($"[DI] Scanning for injectable methods in {type.Name}");
    
    // 查找所有标记为[Inject]的方法
    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    Debug.Log($"[DI] Found {methods.Length} methods in {type.Name}");
    
    foreach (var method in methods)
    {
        var injectAttr = method.GetCustomAttribute<InjectAttribute>();
        if (injectAttr != null)
        {
            Debug.Log($"[DI] Found injectable method: {type.Name}.{method.Name}");
            
            try
            {
                var parameters = method.GetParameters();
                Debug.Log($"[DI] Method has {parameters.Length} parameters");
                
                var parameterValues = new object[parameters.Length];
                
                for (int i = 0; i < parameters.Length; i++)
                {
                    var paramType = parameters[i].ParameterType;
                    Debug.Log($"[DI] Resolving parameter #{i+1}: {paramType.Name}");
                    
                    try
                    {
                        parameterValues[i] = Resolve(paramType);
                        Debug.Log($"[DI] Successfully resolved parameter {paramType.Name}");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[DI] Failed to resolve parameter {paramType.Name} for method {method.Name}: {ex.Message}");
                        throw;
                    }
                }
                
                method.Invoke(instance, parameterValues);
                Debug.Log($"[DI] Successfully injected method: {type.Name}.{method.Name}");
                injected = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DI] Error injecting method {type.Name}.{method.Name}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.LogError($"[DI] Inner exception: {ex.InnerException.Message}");
                }
            }
        }
    }
    
    Debug.Log($"[DI] Method injection scan completed for {type.Name}. Found {injected} injectable methods.");
    return injected;
}

private void InjectViaProperties(object instance, Type type)
{
    Debug.Log($"[DI] Scanning for injectable properties in {type.Name}");
    
    var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    Debug.Log($"[DI] Found {properties.Length} properties in {type.Name}");
    
    int injectedCount = 0;
    
    foreach (var property in properties)
    {
        var injectAttr = property.GetCustomAttribute<InjectAttribute>();
        if (injectAttr != null)
        {
            Debug.Log($"[DI] Found injectable property: {type.Name}.{property.Name}");
            
            if (!property.CanWrite)
            {
                Debug.LogError($"[DI] Property {property.Name} is marked for injection but has no setter");
                continue;
            }
            
            try
            {
                var propertyType = property.PropertyType;
                Debug.Log($"[DI] Resolving property dependency: {propertyType.Name}");
                
                var propertyValue = Resolve(propertyType);
                property.SetValue(instance, propertyValue);
                
                Debug.Log($"[DI] Successfully injected property: {type.Name}.{property.Name}");
                injectedCount++;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DI] Error injecting property {type.Name}.{property.Name}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.LogError($"[DI] Inner exception: {ex.InnerException.Message}");
                }
            }
        }
    }
    
    Debug.Log($"[DI] Property injection completed for {type.Name}. Injected {injectedCount} properties.");
}

private void InjectViaFields(object instance, Type type)
{
    Debug.Log($"[DI] Scanning for injectable fields in {type.Name}");
    
    var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    Debug.Log($"[DI] Found {fields.Length} fields in {type.Name}");
    
    int injectedCount = 0;
    
    foreach (var field in fields)
    {
        var injectAttr = field.GetCustomAttribute<InjectAttribute>();
        if (injectAttr != null)
        {
            Debug.Log($"[DI] Found injectable field: {type.Name}.{field.Name}");
            
            try
            {
                var fieldType = field.FieldType;
                Debug.Log($"[DI] Resolving field dependency: {fieldType.Name}");
                
                var fieldValue = Resolve(fieldType);
                field.SetValue(instance, fieldValue);
                
                Debug.Log($"[DI] Successfully injected field: {type.Name}.{field.Name}");
                injectedCount++;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DI] Error injecting field {type.Name}.{field.Name}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.LogError($"[DI] Inner exception: {ex.InnerException.Message}");
                }
            }
        }
    }
    
    Debug.Log($"[DI] Field injection completed for {type.Name}. Injected {injectedCount} fields.");
}
    private object Resolve(Type serviceType)
    {
        // 1. 检查是否已注册
        if (!_services.TryGetValue(serviceType, out var descriptor))
        {
            // 尝试自动注册具体类型
            if (serviceType.IsClass && !serviceType.IsAbstract)
            {
                Debug.Log($"[DI] Auto-registering concrete type: {serviceType.Name}");
                Register(serviceType, serviceType, ServiceLifetime.Transient);
                descriptor = _services[serviceType];
            }
            else
            {
                throw new InvalidOperationException($"[DI] Service not registered: {serviceType.Name}");
            }
        }

        // 2. 根据生命周期处理
        switch (descriptor.Lifetime)
        {
            case ServiceLifetime.Singleton:
                return GetOrCreateSingleton(serviceType, descriptor);
            
            case ServiceLifetime.Scoped:
                return GetOrCreateScoped(serviceType, descriptor);
            
            case ServiceLifetime.Transient:
                return CreateTransient(serviceType, descriptor);
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private object GetOrCreateSingleton(Type serviceType, ServiceDescriptor descriptor)
    {
        // 检查是否已有实例
        if (_singletonInstances.TryGetValue(serviceType, out var instance))
        {
            return instance;
        }

        // 创建新实例
        instance = CreateInstance(descriptor.ImplementationType);
        _singletonInstances[serviceType] = instance;
        Debug.Log($"[DI] Created singleton: {serviceType.Name}");
        return instance;
    }

    private object GetOrCreateScoped(Type serviceType, ServiceDescriptor descriptor)
    {
        // 检查是否已有实例
        if (_scopedInstances.TryGetValue(serviceType, out var instance))
        {
            return instance;
        }

        // 创建新实例
        instance = CreateInstance(descriptor.ImplementationType);
        _scopedInstances[serviceType] = instance;
        Debug.Log($"[DI] Created scoped: {serviceType.Name}");
        return instance;
    }

    private object CreateTransient(Type serviceType, ServiceDescriptor descriptor)
    {
        var instance = CreateInstance(descriptor.ImplementationType);
        Debug.Log($"[DI] Created transient: {serviceType.Name}");
        return instance;
    }

    private object CreateInstance(Type implementationType)
    {
        // 获取可注入的构造函数
        var constructor = GetInjectableConstructor(implementationType);
        var parameters = constructor.GetParameters();
        var parameterInstances = new object[parameters.Length];

        // 递归解析依赖
        for (int i = 0; i < parameters.Length; i++)
        {
            parameterInstances[i] = Resolve(parameters[i].ParameterType);
        }

        // 创建实例
        return Activator.CreateInstance(implementationType, parameterInstances);
    }

    private ConstructorInfo GetInjectableConstructor(Type type)
    {
        var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        
        if (constructors.Length == 0)
        {
            throw new InvalidOperationException($"[DI] No public constructor found for {type.Name}");
        }
        
        // 优先选择标记了[Inject]特性的构造函数
        foreach (var constructor in constructors)
        {
            if (constructor.GetCustomAttribute<InjectAttribute>() != null)
            {
                return constructor;
            }
        }
        
        // 否则使用参数最多的构造函数
        ConstructorInfo selected = null;
        foreach (var constructor in constructors)
        {
            if (selected == null || constructor.GetParameters().Length > selected.GetParameters().Length)
            {
                selected = constructor;
            }
        }
        
        return selected;
    }

    /// <summary>
    /// 清理当前作用域的实例
    /// </summary>
    public void ClearScope()
    {
        DisposeServices(_scopedInstances.Values);
        _scopedInstances.Clear();
        Debug.Log($"[DI] Scope cleared for sandbox: {Sandbox.Name}");
    }

    /// <summary>
    /// 清理容器的所有注册
    /// </summary>
    public void ClearAll()
    {
        DisposeServices(_singletonInstances.Values);
        DisposeServices(_scopedInstances.Values);
        
        _services.Clear();
        _singletonInstances.Clear();
        _scopedInstances.Clear();
        
        Debug.Log($"[DI] Container fully cleared for sandbox: {Sandbox.Name}");
    }
}
[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method)]
public class InjectAttribute : Attribute { }
internal struct ServiceDescriptor
{
    public Type ServiceType { get; }
    public Type ImplementationType { get; }
    public ServiceLifetime Lifetime { get; }

    public ServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime)
    {
        ServiceType = serviceType;
        ImplementationType = implementationType;
        Lifetime = lifetime;
    }
}

}
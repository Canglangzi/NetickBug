using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Netick;
using UnityEngine;

namespace CockleBurs.GameFramework.Core
{
    /// <summary>
    /// 泛型属性缓存系统，支持缓存反射获取的成员信息
    /// </summary>
    public static class PropertyCache<TAttribute> where TAttribute : Attribute
    {
        #region 内部缓存结构
        private static class TypeCache<T>
        {
            private static PropertyInfo[] _cachedProperties;
            private static FieldInfo[] _cachedFields;
            private static Dictionary<string, MemberInfo> _cachedMembers;
            private static bool _initialized;

            static TypeCache()
            {
                Initialize();
            }

            private static void Initialize()
            {
                if (_initialized) return;

                var type = typeof(T);
                
                // 缓存属性
                _cachedProperties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(p => p.GetCustomAttribute<TAttribute>() != null)
                    .ToArray();
                
                // 缓存字段
                _cachedFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(f => f.GetCustomAttribute<TAttribute>() != null)
                    .ToArray();
                
                // 缓存所有成员
                _cachedMembers = new Dictionary<string, MemberInfo>();
                
                foreach (var prop in _cachedProperties)
                {
                    _cachedMembers[prop.Name] = prop;
                }
                
                foreach (var field in _cachedFields)
                {
                    _cachedMembers[field.Name] = field;
                }
                
                _initialized = true;
            }

            public static PropertyInfo[] GetProperties() => _cachedProperties;
            public static FieldInfo[] GetFields() => _cachedFields;
            public static MemberInfo GetMember(string name) => _cachedMembers.TryGetValue(name, out var member) ? member : null;
            public static bool HasMember(string name) => _cachedMembers.ContainsKey(name);
        }
        #endregion

        #region 静态缓存字典
        private static readonly Dictionary<Type, PropertyInfo[]> _propertyCache = new Dictionary<Type, PropertyInfo[]>();
        private static readonly Dictionary<Type, FieldInfo[]> _fieldCache = new Dictionary<Type, FieldInfo[]>();
        private static readonly Dictionary<Type, Dictionary<string, MemberInfo>> _memberCache = new Dictionary<Type, Dictionary<string, MemberInfo>>();
        private static readonly Dictionary<Type, bool> _typeInitialized = new Dictionary<Type, bool>();
        private static readonly object _lock = new object();
        #endregion

        #region 公共方法
        /// <summary>
        /// 获取指定类型中带有指定特性的属性
        /// </summary>
        public static PropertyInfo[] GetProperties(Type type)
        {
            if (type == null) return Array.Empty<PropertyInfo>();
            
            lock (_lock)
            {
                if (!_propertyCache.TryGetValue(type, out var properties))
                {
                    properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(p => p.GetCustomAttribute<TAttribute>() != null)
                        .ToArray();
                    
                    _propertyCache[type] = properties;
                }
                
                return properties;
            }
        }

        /// <summary>
        /// 获取指定类型中带有指定特性的字段
        /// </summary>
        public static FieldInfo[] GetFields(Type type)
        {
            if (type == null) return Array.Empty<FieldInfo>();
            
            lock (_lock)
            {
                if (!_fieldCache.TryGetValue(type, out var fields))
                {
                    fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(f => f.GetCustomAttribute<TAttribute>() != null)
                        .ToArray();
                    
                    _fieldCache[type] = fields;
                }
                
                return fields;
            }
        }

        /// <summary>
        /// 获取指定类型中所有带有指定特性的成员（属性和字段）
        /// </summary>
        public static MemberInfo[] GetMembers(Type type)
        {
            var properties = GetProperties(type);
            var fields = GetFields(type);
            
            var members = new MemberInfo[properties.Length + fields.Length];
            Array.Copy(properties, members, properties.Length);
            Array.Copy(fields, 0, members, properties.Length, fields.Length);
            
            return members;
        }

        /// <summary>
        /// 获取指定成员的指定特性
        /// </summary>
        public static TAttribute GetAttribute(MemberInfo member)
        {
            return member?.GetCustomAttribute<TAttribute>();
        }

        /// <summary>
        /// 检查指定类型是否包含带有指定特性的成员
        /// </summary>
        public static bool HasMembers(Type type)
        {
            return GetMembers(type).Length > 0;
        }

        /// <summary>
        /// 获取成员值
        /// </summary>
        public static object GetMemberValue(object instance, string memberName)
        {
            if (instance == null || string.IsNullOrEmpty(memberName)) return null;
            
            var type = instance.GetType();
            InitializeTypeCache(type);
            
            if (_memberCache.TryGetValue(type, out var members) && 
                members.TryGetValue(memberName, out var member))
            {
                try
                {
                    if (member is PropertyInfo property)
                        return property.GetValue(instance);
                    
                    if (member is FieldInfo field)
                        return field.GetValue(instance);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"获取成员值失败: {memberName}, 错误: {ex.Message}");
                }
            }
            
            return null;
        }

        /// <summary>
        /// 设置成员值
        /// </summary>
        public static bool SetMemberValue(object instance, string memberName, object value)
        {
            if (instance == null || string.IsNullOrEmpty(memberName)) return false;
            
            var type = instance.GetType();
            InitializeTypeCache(type);
            
            if (_memberCache.TryGetValue(type, out var members) && 
                members.TryGetValue(memberName, out var member))
            {
                try
                {
                    if (member is PropertyInfo property)
                    {
                        property.SetValue(instance, value);
                        return true;
                    }
                    
                    if (member is FieldInfo field)
                    {
                        field.SetValue(instance, value);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"设置成员值失败: {memberName}, 错误: {ex.Message}");
                }
            }
            
            return false;
        }

        /// <summary>
        /// 获取成员的完整键名（类型全名.成员名）
        /// </summary>
        public static string GetFullKey(Type type, string memberName)
        {
            return $"{type.FullName}.{memberName}";
        }

        /// <summary>
        /// 清空指定类型的缓存
        /// </summary>
        public static void ClearCache(Type type)
        {
            lock (_lock)
            {
                _propertyCache.Remove(type);
                _fieldCache.Remove(type);
                _memberCache.Remove(type);
                _typeInitialized.Remove(type);
            }
        }

        /// <summary>
        /// 清空所有缓存
        /// </summary>
        public static void ClearAllCache()
        {
            lock (_lock)
            {
                _propertyCache.Clear();
                _fieldCache.Clear();
                _memberCache.Clear();
                _typeInitialized.Clear();
            }
        }
        #endregion

        #region 私有方法
        private static void InitializeTypeCache(Type type)
        {
            lock (_lock)
            {
                if (_typeInitialized.ContainsKey(type)) return;
                
                var properties = GetProperties(type);
                var fields = GetFields(type);
                
                var members = new Dictionary<string, MemberInfo>();
                
                foreach (var prop in properties)
                {
                    members[prop.Name] = prop;
                }
                
                foreach (var field in fields)
                {
                    members[field.Name] = field;
                }
                
                _memberCache[type] = members;
                _typeInitialized[type] = true;
            }
        }
        #endregion

        #region 高级功能
        /// <summary>
        /// 获取所有网络成员的当前值
        /// </summary>
        public static Dictionary<string, object> GetAllMemberValues(object instance)
        {
            var result = new Dictionary<string, object>();
            
            if (instance == null) return result;
            
            var type = instance.GetType();
            var members = GetMembers(type);
            
            foreach (var member in members)
            {
                try
                {
                    object value = null;
                    
                    if (member is PropertyInfo property)
                        value = property.GetValue(instance);
                    else if (member is FieldInfo field)
                        value = field.GetValue(instance);
                    
                    result[member.Name] = value;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"获取成员值失败: {member.Name}, 错误: {ex.Message}");
                }
            }
            
            return result;
        }

        /// <summary>
        /// 设置所有网络成员的值
        /// </summary>
        public static void SetAllMemberValues(object instance, Dictionary<string, object> values)
        {
            if (instance == null || values == null) return;
            
            var type = instance.GetType();
            InitializeTypeCache(type);
            
            if (!_memberCache.TryGetValue(type, out var members)) return;
            
            foreach (var kvp in values)
            {
                if (members.TryGetValue(kvp.Key, out var member))
                {
                    try
                    {
                        if (member is PropertyInfo property)
                            property.SetValue(instance, kvp.Value);
                        else if (member is FieldInfo field)
                            field.SetValue(instance, kvp.Value);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"设置成员值失败: {kvp.Key}, 错误: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 获取成员的元数据信息
        /// </summary>
        public static MemberMetadata GetMemberMetadata(Type type, string memberName)
        {
            if (type == null || string.IsNullOrEmpty(memberName)) return null;
            
            InitializeTypeCache(type);
            
            if (!_memberCache.TryGetValue(type, out var members) || 
                !members.TryGetValue(memberName, out var member))
            {
                return null;
            }
            
            return new MemberMetadata
            {
                Name = member.Name,
                MemberType = member.MemberType,
                DeclaringType = member.DeclaringType,
                Attribute = GetAttribute(member),
                IsProperty = member is PropertyInfo,
                IsField = member is FieldInfo,
                PropertyType = (member as PropertyInfo)?.PropertyType,
                FieldType = (member as FieldInfo)?.FieldType
            };
        }

        /// <summary>
        /// 成员元数据类
        /// </summary>
        public class MemberMetadata
        {
            public string Name { get; set; }
            public MemberTypes MemberType { get; set; }
            public Type DeclaringType { get; set; }
            public TAttribute Attribute { get; set; }
            public bool IsProperty { get; set; }
            public bool IsField { get; set; }
            public Type PropertyType { get; set; }
            public Type FieldType { get; set; }
            
            public Type ValueType => IsProperty ? PropertyType : FieldType;
            
            public override string ToString()
            {
                return $"{Name} ({ValueType?.Name})";
            }
        }
        #endregion
    }

    /// <summary>
    /// 网络属性缓存（专门用于[Networked]特性）
    /// </summary>
    public static class NetworkedPropertyCache
    {
        /// <summary>
        /// 获取带有[Networked]特性的属性
        /// </summary>
        public static PropertyInfo[] GetProperties(Type type) => 
            PropertyCache<Networked>.GetProperties(type);

        /// <summary>
        /// 获取带有[Networked]特性的字段
        /// </summary>
        public static FieldInfo[] GetFields(Type type) => 
            PropertyCache<Networked>.GetFields(type);

        /// <summary>
        /// 获取所有带有[Networked]特性的成员
        /// </summary>
        public static MemberInfo[] GetMembers(Type type) => 
            PropertyCache<Networked>.GetMembers(type);

        /// <summary>
        /// 获取成员值
        /// </summary>
        public static object GetValue(object instance, string memberName) => 
            PropertyCache<Networked>.GetMemberValue(instance, memberName);

        /// <summary>
        /// 设置成员值
        /// </summary>
        public static bool SetValue(object instance, string memberName, object value) => 
            PropertyCache<Networked>.SetMemberValue(instance, memberName, value);

        /// <summary>
        /// 获取所有网络成员的值
        /// </summary>
        public static Dictionary<string, object> GetAllValues(object instance) => 
            PropertyCache<Networked>.GetAllMemberValues(instance);

        /// <summary>
        /// 设置所有网络成员的值
        /// </summary>
        public static void SetAllValues(object instance, Dictionary<string, object> values) => 
            PropertyCache<Networked>.SetAllMemberValues(instance, values);
    }
}
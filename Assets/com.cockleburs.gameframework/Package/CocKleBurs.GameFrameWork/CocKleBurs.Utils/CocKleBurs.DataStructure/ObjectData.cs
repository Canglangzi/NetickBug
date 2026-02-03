using System;
using System.Collections.Generic;
using UnityEngine;


namespace CockleBurs.GameFramework.Utility
{
// 定义 FlexibleData 结构体，存储单条数据的名称和值
[Serializable]
public struct ObjectData
{
    public string name;      // 数据名称（唯一标识）
    public object value;     // 存储任意类型的值

    // 构造函数：初始化名称和值
    public ObjectData(string name, object value)
    {
        this.name = name;
        this.value = value;
    }

    // 重写 ToString，方便调试查看
    public override string ToString()
    {
        return $"[{name}] = {value} (类型: {value?.GetType().Name ?? "null"})";
    }
}

// 静态工具类：管理所有 FlexibleData 结构体的字典
public static class FlexibleDataManager
{
    // 静态字典：存储所有 FlexibleData 结构体，键为 name
    private static readonly Dictionary<string, ObjectData> _dataDict = new Dictionary<string, ObjectData>();

    // 添加或更新数据（将 FlexibleData 结构体存入字典）
    public static void SetData(ObjectData data)
    {
        if (string.IsNullOrEmpty(data.name))
        {
            Debug.LogError("FlexibleData 的 name 不能为空！");
            return;
        }

        if (_dataDict.ContainsKey(data.name))
        {
            _dataDict[data.name] = data; // 覆盖已有数据
        }
        else
        {
            _dataDict.Add(data.name, data); // 添加新数据
        }
    }

    // 重载：直接通过 name 和 value 创建结构体并存储
    public static void SetData(string name, object value)
    {
        SetData(new ObjectData(name, value));
    }

    // 获取数据（返回 FlexibleData 结构体）
    public static bool TryGetData(string name, out ObjectData data)
    {
        return _dataDict.TryGetValue(name, out data);
    }

    // 获取数据的值（泛型转换）
    public static T GetValue<T>(string name)
    {
        if (_dataDict.TryGetValue(name, out var data))
        {
            try
            {
                return (T)data.value;
            }
            catch (InvalidCastException)
            {
                Debug.LogError($"数据 [{name}] 无法转换为 {typeof(T).Name} 类型！");
            }
        }
        else
        {
            Debug.LogWarning($"找不到名称为 [{name}] 的数据！");
        }
        return default;
    }

    // 检查数据是否存在
    public static bool ContainsData(string name)
    {
        return _dataDict.ContainsKey(name);
    }

    // 移除数据
    public static void RemoveData(string name)
    {
        if (_dataDict.ContainsKey(name))
        {
            _dataDict.Remove(name);
            Debug.Log($"已移除数据: [{name}]");
        }
        else
        {
            Debug.LogWarning($"找不到名称为 [{name}] 的数据，无法移除！");
        }
    }

    // 清空所有数据
    public static void ClearAll()
    {
        _dataDict.Clear();
        Debug.Log("所有数据已清空");
    }

    // 打印所有数据（调试用）
    public static void PrintAll()
    {
        Debug.Log("===== 所有 FlexibleData 数据 =====");
        foreach (var data in _dataDict.Values)
        {
            Debug.Log(data.ToString());
        }
        Debug.Log("===================================");
    }
}
}
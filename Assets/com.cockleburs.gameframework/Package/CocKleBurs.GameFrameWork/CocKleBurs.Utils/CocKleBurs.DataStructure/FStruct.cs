using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace CockleBurs.GameFramework.Utility
{
[StructLayout(LayoutKind.Sequential)]
public struct FField<T>
{
    public string Name;
    public T Value;

    public FField(string name, T value)
    {
        Name = name;
        Value = value;
    }
}

public class FStruct
{
    private List<IntPtr> fields ;
    private List<string> fieldNames ;
    private int fieldCount; // 字段数量

    public FStruct()
    {
        fields = new List<IntPtr>();
        fieldNames = new List<string>();
        fieldCount = 0;
    }

    // 添加字段
    public void AddField<T>(string name, T value)
    {
        // 分配内存并存储值
        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(T)));
        Marshal.StructureToPtr(value, ptr, false);
        fields.Add(ptr);
        fieldNames.Add(name);
        fieldCount++;
    }

    // 获取字段
    public T GetField<T>(string name)
    {
        int index = fieldNames.IndexOf(name);
        if (index < 0)
        {
            throw new Exception($"Field '{name}' not found.");
        }

        // 从指针读取值
        return Marshal.PtrToStructure<T>(fields[index]);
    }

    // 设置字段
    public void SetField<T>(string name, T value)
    {
        int index = fieldNames.IndexOf(name);
        if (index < 0)
        {
            throw new Exception($"Field '{name}' not found.");
        }

        // 更新指针指向的值
        Marshal.StructureToPtr(value, fields[index], false);
    }

    // 清理内存
    public void CleanUp()
    {
        foreach (var ptr in fields)
        {
            Marshal.FreeHGlobal(ptr); // 释放内存
        }
        fields.Clear();
        fieldNames.Clear();
        fieldCount = 0;
    }

    // 获取字段名称列表
    public List<string> GetFieldNames()
    {
        return new List<string>(fieldNames);
    }

    // 获取字段数量
    public int FieldCount => fieldCount;
}

}
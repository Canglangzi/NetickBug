using System;
using System.Collections.Generic;
using Netick;
using Netick.Unity;
using UnityEngine;

namespace CockleBurs.GameFramework.Extension
{
    static partial class NetworkArrayExtensions
    {
        // 获取 NetworkArray 的容量
        public static int GetCapacity<T>(this NetworkArray<T> array) where T : unmanaged
        {
            // 使用反射获取内部 _length 字段值
            var lengthField = array.GetType().GetField("_length",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            return lengthField != null ? (int)lengthField.GetValue(array) : 0;
        }

        // 安全设置值的方法
        public static void SafeSet<T>(this NetworkArray<T> array, int index, T value) where T : unmanaged
        {
            if (index < 0 || index >= array.GetCapacity())
            {
                Debug.LogError($"Index {index} out of range for NetworkArray<{typeof(T).Name}>");
                return;
            }

            // 使用索引器设置值
            array[index] = value;
        }

        // 批量设置多个值
        public static void SetRange<T>(this NetworkArray<T> array, int startIndex, IEnumerable<T> values)
            where T : unmanaged
        {
            int capacity = array.GetCapacity();
            int i = startIndex;

            foreach (var value in values)
            {
                if (i >= capacity) break;
                array.SafeSet(i++, value);
            }
        }

        // 批量获取多个值
        public static T[] GetRange<T>(this NetworkArray<T> array, int startIndex, int count) where T : unmanaged
        {
            int capacity = array.GetCapacity();
            if (startIndex < 0 || count <= 0 || startIndex >= capacity)
                return Array.Empty<T>();

            int actualCount = Mathf.Min(count, capacity - startIndex);
            T[] result = new T[actualCount];

            for (int i = 0; i < actualCount; i++)
            {
                result[i] = array[startIndex + i];
            }

            return result;
        }

        // 标记整个数组为脏（强制同步）
        public static void MarkDirty<T>(this NetworkArray<T> array) where T : unmanaged
        {
            int capacity = array.GetCapacity();
            for (int i = 0; i < capacity; i++)
            {
                // 设置值到自身触发脏标记
                array.SafeSet(i, array[i]);
            }
        }
    }
}
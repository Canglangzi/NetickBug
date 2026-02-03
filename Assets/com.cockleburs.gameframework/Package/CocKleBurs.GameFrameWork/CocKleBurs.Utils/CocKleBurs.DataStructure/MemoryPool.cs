using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;


namespace CockleBurs.GameFramework.Utility
{
public enum AllocationStrategy
{
    FirstFit,
    BestFit,
    WorstFit
}

public class MemoryPool<T> where T : unmanaged // 使用 unmanaged 约束以支持指针
{
    private readonly Dictionary<IntPtr, MemoryBlock> allBlocks; // 存储所有内存块
    private readonly Stack<IntPtr> freeBlocks; // 存储空闲块的堆栈
    private AllocationStrategy strategy;

    public MemoryPool(int initialPoolSize, AllocationStrategy allocationStrategy = AllocationStrategy.FirstFit)
    {
        allBlocks = new Dictionary<IntPtr, MemoryBlock>(initialPoolSize);
        freeBlocks = new Stack<IntPtr>(initialPoolSize);
        strategy = allocationStrategy;

        for (int i = 0; i < initialPoolSize; i++)
        {
            IntPtr ptr = AllocateMemory(Marshal.SizeOf<T>());
            freeBlocks.Push(ptr);
            allBlocks[ptr] = new MemoryBlock(ptr, Marshal.SizeOf<T>());
        }
    }

    /// <summary>
    /// 分配内存的方法，使用InternalCall进行内部实现。
    /// </summary>
    /// <param name="size">分配内存的大小</param>
    /// <returns>返回指向分配内存的指针</returns>
    [MethodImpl(MethodImplOptions.InternalCall)]
    [ThreadSafe(true)]
    private static extern IntPtr AllocateMemory(int size);

    /// <summary>
    /// 释放内存的方法，使用InternalCall进行内部实现。
    /// </summary>
    /// <param name="ptr">要释放的内存块指针</param>
    [MethodImpl(MethodImplOptions.InternalCall)]
    [ThreadSafe(true)]
    private static extern void FreeMemory(IntPtr ptr);

    public unsafe T* Allocate()
    {
        if (freeBlocks.Count > 0)
        {
            IntPtr ptr = freeBlocks.Pop();
            allBlocks[ptr].Mark(true); // 标记为已使用
            return (T*)ptr; // 使用指针返回对象
        }

        // 寻找适合的内存块
        MemoryBlock block = FindSuitableBlock(Marshal.SizeOf<T>());
        if (block == null)
        {
            block = new MemoryBlock(AllocateMemory(Marshal.SizeOf<T>()), Marshal.SizeOf<T>());
            allBlocks[block.Data] = block;
        }

        block.Mark(true); // 标记为已使用
        return (T*)block.Data; // 使用指针返回对象
    }

    public unsafe void Release(T* item)
    {
        IntPtr ptr = (IntPtr)item;
        if (allBlocks.TryGetValue(ptr, out MemoryBlock block) && block.IsUsed)
        {
            block.Mark(false); // 标记为未使用
            freeBlocks.Push(ptr); // 将空闲块返回池中
            MergeFreeBlocks();
        }
    }

    public void CollectGarbage()
    {
        foreach (var block in allBlocks.Values)
        {
            block.Mark(false);
        }

        foreach (var ptr in freeBlocks)
        {
            if (allBlocks.TryGetValue(ptr, out MemoryBlock block))
            {
                block.Mark(true);
            }
        }

        // 清除未标记的内存块
        var keysToRemove = new List<IntPtr>();
        foreach (var kvp in allBlocks)
        {
            if (!kvp.Value.IsMarked)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            FreeMemory(key);
            allBlocks.Remove(key);
        }
    }

    private MemoryBlock FindSuitableBlock(int size)
    {
        switch (strategy)
        {
            case AllocationStrategy.FirstFit:
                return FindFirstFit(size);
            case AllocationStrategy.BestFit:
                return FindBestFit(size);
            case AllocationStrategy.WorstFit:
                return FindWorstFit(size);
            default:
                return null;
        }
    }

    private MemoryBlock FindFirstFit(int size)
    {
        foreach (var block in allBlocks.Values)
        {
            if (!block.IsUsed && block.Size >= size)
            {
                return block;
            }
        }
        return null;
    }

    private MemoryBlock FindBestFit(int size)
    {
        MemoryBlock bestFit = null;
        foreach (var block in allBlocks.Values)
        {
            if (!block.IsUsed && block.Size >= size)
            {
                if (bestFit == null || block.Size < bestFit.Size)
                {
                    bestFit = block;
                }
            }
        }
        return bestFit;
    }

    private MemoryBlock FindWorstFit(int size)
    {
        MemoryBlock worstFit = null;
        foreach (var block in allBlocks.Values)
        {
            if (!block.IsUsed && block.Size >= size)
            {
                if (worstFit == null || block.Size > worstFit.Size)
                {
                    worstFit = block;
                }
            }
        }
        return worstFit;
    }

    private void MergeFreeBlocks()
    {
        var freeBlocksList = new List<MemoryBlock>();

        foreach (var block in allBlocks.Values)
        {
            if (!block.IsUsed)
            {
                freeBlocksList.Add(block);
            }
        }

        freeBlocksList.Sort((a, b) => a.Data.ToInt64().CompareTo(b.Data.ToInt64())); // 按地址排序

        for (int i = 0; i < freeBlocksList.Count - 1; i++)
        {
            var currentBlock = freeBlocksList[i];
            var nextBlock = freeBlocksList[i + 1];

            if (!currentBlock.IsUsed && !nextBlock.IsUsed) // 如果两个都是空闲块
            {
                currentBlock.Merge(nextBlock);
                freeBlocksList.RemoveAt(i + 1); // 移除下一个块
                i--; // 调整索引以重新检查当前块
            }
        }
    }

    private class MemoryBlock
    {
        public int Size { get; private set; }
        public bool IsMarked { get; private set; }
        public bool IsUsed { get; private set; }
        public IntPtr Data { get; private set; }

        public MemoryBlock(IntPtr data, int size)
        {
            Size = size;
            Data = data;
            IsMarked = false;
            IsUsed = false; // 初始化为未使用
        }

        public void Mark(bool value)
        {
            IsMarked = value;
            IsUsed = value; // 根据标记更新使用状态
        }

        public void Merge(MemoryBlock other)
        {
            Size += other.Size;
            FreeMemory(other.Data); // 释放被合并的块
        }

        ~MemoryBlock()
        {
            FreeMemory(Data);
        }
    }
}

}
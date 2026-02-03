using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;


namespace CockleBurs.GameFramework.Utility
{
public unsafe class ArenaAllocator : IDisposable {
    byte* buffer; // 'buffer' is a pointer to a byte (i.e., byte*)
    int offset;
    readonly int capacity;

    public ArenaAllocator(int sizeInBytes) {
        buffer = (byte*)UnsafeUtility.Malloc(sizeInBytes, 16, Allocator.Persistent);
        offset = 0;
        capacity = sizeInBytes;
    }

    public T* Alloc<T>(int count = 1) where T : unmanaged {
        int size = UnsafeUtility.SizeOf<T>() * count;
        if (offset + size > capacity) throw new Exception("Arena overflow");
        
        T* ptr = (T*)(buffer + offset);
        offset += size;
        return ptr;
    }
    
    public void Reset() => offset = 0;

    public void Dispose() {
        if (buffer != null) {
            UnsafeUtility.Free(buffer, Allocator.Persistent);
            buffer = null;
        }
        offset = 0;
    }
}
}
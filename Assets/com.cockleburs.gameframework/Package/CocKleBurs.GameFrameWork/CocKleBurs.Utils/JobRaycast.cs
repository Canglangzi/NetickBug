using UnityEngine;
using Unity.Collections;
using Unity.Jobs;


namespace CockleBurs.GameFramework.Utility
{
public static class JobRaycast
{
    private static NativeArray<RaycastCommand> raycastCommandArray;
    private static NativeArray<RaycastHit> raycastHitArray;
    private static JobHandle jobHandle;
    private static bool isInitialized = false;

    // 初始化
    public static void Initialize(int maxHits)
    {
        if (!isInitialized)
        {
            raycastCommandArray = new NativeArray<RaycastCommand>(maxHits, Allocator.Persistent);
            raycastHitArray = new NativeArray<RaycastHit>(maxHits, Allocator.Persistent);
            isInitialized = true;
        }
    }

    // 释放资源
    public static void Dispose()
    {
        if (isInitialized)
        {
            jobHandle.Complete();
            raycastCommandArray.Dispose();
            raycastHitArray.Dispose();
            isInitialized = false;
        }
    }

    // 句柄属性
    public static JobHandle CurrentJobHandle => jobHandle;

    // 封装的单条射线检测方法
    public static bool Raycast(Vector3 origin, Vector3 direction, float maxDistance, int layerMask, out RaycastHit hit)
    {
        hit = new RaycastHit();
        return Raycast(new[] { origin }, new[] { direction }, maxDistance, layerMask, out RaycastHit[] hits) && hits.Length > 0 && (hit = hits[0]).collider != null;
    }

    // 封装的多条射线检测方法
    public static bool Raycast(Vector3 origin, Vector3 direction, float maxDistance, int layerMask, out RaycastHit[] hits)
    {
        return Raycast(new[] { origin }, new[] { direction }, maxDistance, layerMask, out hits);
    }

    // 支持多条射线的重载方法
    public static bool Raycast(Vector3[] origins, Vector3[] directions, float maxDistance, int layerMask, out RaycastHit[] hits)
    {
        int rayCount = origins.Length;
        hits = new RaycastHit[rayCount];

        // 完成上一次的任务
        jobHandle.Complete();

        // 设置射线命令
        for (int i = 0; i < rayCount; i++)
        {
            raycastCommandArray[i] = new RaycastCommand(origins[i], directions[i], maxDistance, layerMask, 1);
        }

        // 调度任务
        jobHandle = RaycastCommand.ScheduleBatch(raycastCommandArray, raycastHitArray, rayCount);

        // 等待完成
        jobHandle.Complete();

        int hitCount = 0;

        // 检查结果
        for (int i = 0; i < rayCount; i++)
        {
            if (raycastHitArray[i].collider != null) // 检查 collider 是否有效
            {
                hits[hitCount++] = raycastHitArray[i]; // 存储碰撞体
            }
        }

        // 如果有命中，更新 hits 数组大小
        System.Array.Resize(ref hits, hitCount);
        return hitCount > 0; // 如果有命中返回 true
    }
}

}
using UnityEngine;
using System.Collections.Generic;

namespace PhysicsExtensions
{
    /// <summary>
    /// 物理检测结果缓冲区管理器
    /// </summary>
    public static class PhysicsBuffer
    {
        // 3D物理检测缓冲区
        private static readonly Dictionary<int, RaycastHit[]> raycastBuffer3D = new Dictionary<int, RaycastHit[]>();
        private static readonly Dictionary<int, Collider[]> overlapBuffer3D = new Dictionary<int, Collider[]>();
        
        // 2D物理检测缓冲区
        private static readonly Dictionary<int, RaycastHit2D[]> raycastBuffer2D = new Dictionary<int, RaycastHit2D[]>();
        private static readonly Dictionary<int, Collider2D[]> overlapBuffer2D = new Dictionary<int, Collider2D[]>();
        
        // 缓冲区大小配置
        public const int DefaultBufferSize = 32;
        private const int MaxBufferSize = 512;

        /// <summary>
        /// 获取或创建3D射线检测缓冲区
        /// </summary>
        public static RaycastHit[] GetRaycastBuffer3D(int size = DefaultBufferSize)
        {
            size = Mathf.Min(size, MaxBufferSize);
            
            if (!raycastBuffer3D.TryGetValue(size, out var buffer))
            {
                buffer = new RaycastHit[size];
                raycastBuffer3D[size] = buffer;
            }
            
            return buffer;
        }

        /// <summary>
        /// 获取或创建3D碰撞体检测缓冲区
        /// </summary>
        public static Collider[] GetOverlapBuffer3D(int size = DefaultBufferSize)
        {
            size = Mathf.Min(size, MaxBufferSize);
            
            if (!overlapBuffer3D.TryGetValue(size, out var buffer))
            {
                buffer = new Collider[size];
                overlapBuffer3D[size] = buffer;
            }
            
            return buffer;
        }

        /// <summary>
        /// 获取或创建2D射线检测缓冲区
        /// </summary>
        public static RaycastHit2D[] GetRaycastBuffer2D(int size = DefaultBufferSize)
        {
            size = Mathf.Min(size, MaxBufferSize);
            
            if (!raycastBuffer2D.TryGetValue(size, out var buffer))
            {
                buffer = new RaycastHit2D[size];
                raycastBuffer2D[size] = buffer;
            }
            
            return buffer;
        }

        /// <summary>
        /// 获取或创建2D碰撞体检测缓冲区
        /// </summary>
        public static Collider2D[] GetOverlapBuffer2D(int size = DefaultBufferSize)
        {
            size = Mathf.Min(size, MaxBufferSize);
            
            if (!overlapBuffer2D.TryGetValue(size, out var buffer))
            {
                buffer = new Collider2D[size];
                overlapBuffer2D[size] = buffer;
            }
            
            return buffer;
        }

        /// <summary>
        /// 清除所有缓冲区（场景切换时调用）
        /// </summary>
        public static void ClearAllBuffers()
        {
            raycastBuffer3D.Clear();
            overlapBuffer3D.Clear();
            raycastBuffer2D.Clear();
            overlapBuffer2D.Clear();
        }
    }

    /// <summary>
    /// Transform的物理检测扩展方法（3D）
    /// </summary>
    public static class PhysicsExtensions3D
    {
        #region Raycast 射线检测

        /// <summary>
        /// 从当前位置向前发射射线（使用Transform的前向方向）
        /// </summary>
        public static bool Raycast(this Transform transform, float maxDistance = Mathf.Infinity, 
            int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return Physics.Raycast(transform.position, transform.forward, maxDistance, layerMask, queryTriggerInteraction);
        }

        /// <summary>
        /// 从当前位置向前发射射线并获取碰撞信息
        /// </summary>
        public static bool Raycast(this Transform transform, out RaycastHit hitInfo, float maxDistance = Mathf.Infinity,
            int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return Physics.Raycast(transform.position, transform.forward, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
        }

        /// <summary>
        /// 从当前位置向指定方向发射射线
        /// </summary>
        public static bool Raycast(this Transform transform, Vector3 direction, float maxDistance = Mathf.Infinity,
            int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return Physics.Raycast(transform.position, direction, maxDistance, layerMask, queryTriggerInteraction);
        }

        /// <summary>
        /// 从当前位置向指定方向发射射线并获取碰撞信息
        /// </summary>
        public static bool Raycast(this Transform transform, Vector3 direction, out RaycastHit hitInfo, float maxDistance = Mathf.Infinity,
            int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return Physics.Raycast(transform.position, direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
        }

        /// <summary>
        /// 从当前位置向前发射射线，检测所有碰撞（零GC版本）
        /// </summary>
        public static int RaycastAllNonAlloc(this Transform transform, RaycastHit[] results, float maxDistance = Mathf.Infinity,
            int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return Physics.RaycastNonAlloc(transform.position, transform.forward, results, maxDistance, layerMask, queryTriggerInteraction);
        }

        /// <summary>
        /// 从当前位置向前发射射线，检测所有碰撞（自动缓冲区版本）
        /// </summary>
        public static int RaycastAll(this Transform transform, out RaycastHit[] results, float maxDistance = Mathf.Infinity,
            int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
            int bufferSize = PhysicsBuffer.DefaultBufferSize)
        {
            results = PhysicsBuffer.GetRaycastBuffer3D(bufferSize);
            return Physics.RaycastNonAlloc(transform.position, transform.forward, results, maxDistance, layerMask, queryTriggerInteraction);
        }

        #endregion

        #region SphereCast 球形检测

        /// <summary>
        /// 从当前位置向前发射球形射线
        /// </summary>
        /// <summary>
        /// 从当前位置向前发射球形射线
        /// </summary>
        public static bool SphereCast(this Transform transform, float radius, float maxDistance = Mathf.Infinity,
            int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            // 使用Ray结构体版本的重载
            Ray ray = new Ray(transform.position, transform.forward);
            return Physics.SphereCast(ray, radius, maxDistance, layerMask, queryTriggerInteraction);
        }
        
        /// <summary>
        /// 从当前位置向前发射球形射线并获取碰撞信息
        /// </summary>
        public static bool SphereCast(this Transform transform, float radius, out RaycastHit hitInfo, float maxDistance = Mathf.Infinity,
            int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return Physics.SphereCast(transform.position, radius, transform.forward, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
        }

        /// <summary>
        /// 球形检测所有碰撞（自动缓冲区版本）
        /// </summary>
        public static int SphereCastAll(this Transform transform, float radius, out RaycastHit[] results, float maxDistance = Mathf.Infinity,
            int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
            int bufferSize = PhysicsBuffer.DefaultBufferSize)
        {
            results = PhysicsBuffer.GetRaycastBuffer3D(bufferSize);
            return Physics.SphereCastNonAlloc(transform.position, radius, transform.forward, results, maxDistance, layerMask, queryTriggerInteraction);
        }

        #endregion

        #region Overlap 重叠检测

        /// <summary>
        /// 球形重叠检测（自动缓冲区版本）
        /// </summary>
        public static int OverlapSphere(this Transform transform, float radius, out Collider[] results,
            int layerMask = Physics.AllLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
            int bufferSize = PhysicsBuffer.DefaultBufferSize)
        {
            results = PhysicsBuffer.GetOverlapBuffer3D(bufferSize);
            return Physics.OverlapSphereNonAlloc(transform.position, radius, results, layerMask, queryTriggerInteraction);
        }

        /// <summary>
        /// 盒形重叠检测（自动缓冲区版本）
        /// </summary>
        public static int OverlapBox(this Transform transform, Vector3 halfExtents, out Collider[] results, 
            Quaternion orientation, int layerMask = Physics.AllLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
            int bufferSize = PhysicsBuffer.DefaultBufferSize)
        {
            results = PhysicsBuffer.GetOverlapBuffer3D(bufferSize);
            return Physics.OverlapBoxNonAlloc(transform.position, halfExtents, results, orientation, layerMask, queryTriggerInteraction);
        }

        /// <summary>
        /// 胶囊体重叠检测（自动缓冲区版本）
        /// </summary>
        public static int OverlapCapsule(this Transform transform, Vector3 point0, Vector3 point1, float radius, out Collider[] results,
            int layerMask = Physics.AllLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
            int bufferSize = PhysicsBuffer.DefaultBufferSize)
        {
            results = PhysicsBuffer.GetOverlapBuffer3D(bufferSize);
            return Physics.OverlapCapsuleNonAlloc(point0, point1, radius, results, layerMask, queryTriggerInteraction);
        }

        #endregion

        #region Check 检查检测

        /// <summary>
        /// 检查球形区域内是否有碰撞体
        /// </summary>
        public static bool CheckSphere(this Transform transform, float radius,
            int layerMask = Physics.AllLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return Physics.CheckSphere(transform.position, radius, layerMask, queryTriggerInteraction);
        }

        /// <summary>
        /// 检查盒形区域内是否有碰撞体
        /// </summary>
        public static bool CheckBox(this Transform transform, Vector3 halfExtents, Quaternion orientation,
            int layerMask = Physics.AllLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return Physics.CheckBox(transform.position, halfExtents, orientation, layerMask, queryTriggerInteraction);
        }

        /// <summary>
        /// 检查胶囊体区域内是否有碰撞体
        /// </summary>
        public static bool CheckCapsule(this Transform transform, Vector3 point0, Vector3 point1, float radius,
            int layerMask = Physics.AllLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return Physics.CheckCapsule(point0, point1, radius, layerMask, queryTriggerInteraction);
        }

        #endregion

        #region Linecast 线段检测

        /// <summary>
        /// 从当前位置到目标位置进行线段检测
        /// </summary>
        public static bool Linecast(this Transform transform, Vector3 targetPosition,
            int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return Physics.Linecast(transform.position, targetPosition, layerMask, queryTriggerInteraction);
        }

        /// <summary>
        /// 从当前位置到目标位置进行线段检测并获取碰撞信息
        /// </summary>
        public static bool Linecast(this Transform transform, Vector3 targetPosition, out RaycastHit hitInfo,
            int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return Physics.Linecast(transform.position, targetPosition, out hitInfo, layerMask, queryTriggerInteraction);
        }

        #endregion
    }

    /// <summary>
    /// Transform的物理检测扩展方法（2D）
    /// </summary>
    public static class PhysicsExtensions2D
    {
        #region Raycast 射线检测

        /// <summary>
        /// 从当前位置向右发射2D射线（使用Transform的右向方向）
        /// </summary>
        public static RaycastHit2D Raycast2D(this Transform transform, float distance = Mathf.Infinity,
            int layerMask = Physics2D.DefaultRaycastLayers, float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity)
        {
            return Physics2D.Raycast(transform.position, transform.right, distance, layerMask, minDepth, maxDepth);
        }

        /// <summary>
        /// 从当前位置向指定方向发射2D射线
        /// </summary>
        public static RaycastHit2D Raycast2D(this Transform transform, Vector2 direction, float distance = Mathf.Infinity,
            int layerMask = Physics2D.DefaultRaycastLayers, float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity)
        {
            return Physics2D.Raycast(transform.position, direction, distance, layerMask, minDepth, maxDepth);
        }

        /// <summary>
        /// 从当前位置发射2D射线，检测所有碰撞（自动缓冲区版本）
        /// </summary>
        public static int RaycastAll2D(this Transform transform, Vector2 direction, out RaycastHit2D[] results, float distance = Mathf.Infinity,
            int layerMask = Physics2D.DefaultRaycastLayers, float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity,
            int bufferSize = PhysicsBuffer.DefaultBufferSize)
        {
            results = PhysicsBuffer.GetRaycastBuffer2D(bufferSize);
            return Physics2D.RaycastNonAlloc(transform.position, direction, results, distance, layerMask, minDepth, maxDepth);
        }

        #endregion

        #region CircleCast 圆形检测

        /// <summary>
        /// 从当前位置发射圆形射线
        /// </summary>
        public static RaycastHit2D CircleCast2D(this Transform transform, float radius, Vector2 direction, float distance = Mathf.Infinity,
            int layerMask = Physics2D.DefaultRaycastLayers, float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity)
        {
            return Physics2D.CircleCast(transform.position, radius, direction, distance, layerMask, minDepth, maxDepth);
        }

        /// <summary>
        /// 圆形检测所有碰撞（自动缓冲区版本）
        /// </summary>
        public static int CircleCastAll2D(this Transform transform, float radius, Vector2 direction, out RaycastHit2D[] results,
            float distance = Mathf.Infinity, int layerMask = Physics2D.DefaultRaycastLayers,
            float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity, int bufferSize = PhysicsBuffer.DefaultBufferSize)
        {
            results = PhysicsBuffer.GetRaycastBuffer2D(bufferSize);
            return Physics2D.CircleCastNonAlloc(transform.position, radius, direction, results, distance, layerMask, minDepth, maxDepth);
        }

        #endregion

        #region Overlap 重叠检测

        /// <summary>
        /// 圆形重叠检测（自动缓冲区版本）
        /// </summary>
        public static int OverlapCircle2D(this Transform transform, float radius, out Collider2D[] results,
            int layerMask = Physics2D.AllLayers, float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity,
            int bufferSize = PhysicsBuffer.DefaultBufferSize)
        {
            results = PhysicsBuffer.GetOverlapBuffer2D(bufferSize);
            return Physics2D.OverlapCircleNonAlloc(transform.position, radius, results, layerMask, minDepth, maxDepth);
        }

        /// <summary>
        /// 盒形重叠检测（自动缓冲区版本）
        /// </summary>
        public static int OverlapBox2D(this Transform transform, Vector2 size, out Collider2D[] results,
            float angle = 0, int layerMask = Physics2D.AllLayers, float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity,
            int bufferSize = PhysicsBuffer.DefaultBufferSize)
        {
            results = PhysicsBuffer.GetOverlapBuffer2D(bufferSize);
            return Physics2D.OverlapBoxNonAlloc(transform.position, size, angle, results, layerMask, minDepth, maxDepth);
        }

        /// <summary>
        /// 胶囊体重叠检测（自动缓冲区版本）
        /// </summary>
        public static int OverlapCapsule2D(this Transform transform, Vector2 size, CapsuleDirection2D direction,
            float angle, out Collider2D[] results, int layerMask = Physics2D.AllLayers,
            float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity, int bufferSize = PhysicsBuffer.DefaultBufferSize)
        {
            results = PhysicsBuffer.GetOverlapBuffer2D(bufferSize);
            return Physics2D.OverlapCapsuleNonAlloc(transform.position, size, direction, angle, results, layerMask, minDepth, maxDepth);
        }

        #endregion

        #region Check 检查检测

        /// <summary>
        /// 检查圆形区域内是否有碰撞体
        /// </summary>
        public static bool CheckCircle2D(this Transform transform, float radius,
            int layerMask = Physics2D.AllLayers, float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity)
        {
            return Physics2D.OverlapCircle(transform.position, radius, layerMask, minDepth, maxDepth) != null;
        }

        /// <summary>
        /// 检查盒形区域内是否有碰撞体
        /// </summary>
        public static bool CheckBox2D(this Transform transform, Vector2 size, float angle = 0,
            int layerMask = Physics2D.AllLayers, float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity)
        {
            return Physics2D.OverlapBox(transform.position, size, angle, layerMask, minDepth, maxDepth) != null;
        }

        #endregion
    }

    /// <summary>
    /// 实用工具扩展
    /// </summary>
    public static class PhysicsUtilityExtensions
    {
        /// <summary>
        /// 获取最近的有效碰撞体
        /// </summary>
        public static Collider GetNearestValidCollider(this Collider[] colliders, Vector3 position, int count)
        {
            Collider nearest = null;
            float nearestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var collider = colliders[i];
                if (collider == null) continue;

                float distance = Vector3.Distance(position, collider.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = collider;
                }
            }

            return nearest;
        }

        /// <summary>
        /// 获取最近的有效2D碰撞体
        /// </summary>
        public static Collider2D GetNearestValidCollider2D(this Collider2D[] colliders, Vector2 position, int count)
        {
            Collider2D nearest = null;
            float nearestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var collider = colliders[i];
                if (collider == null) continue;

                float distance = Vector2.Distance(position, collider.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = collider;
                }
            }

            return nearest;
        }

        /// <summary>
        /// 过滤掉特定游戏对象的碰撞体
        /// </summary>
        public static int FilterOutColliders(this Collider[] colliders, int count, GameObject excludeObject)
        {
            int validCount = 0;
            
            for (int i = 0; i < count; i++)
            {
                if (colliders[i] != null && colliders[i].gameObject != excludeObject)
                {
                    if (validCount != i)
                    {
                        colliders[validCount] = colliders[i];
                    }
                    validCount++;
                }
            }

            // 清理剩余部分
            for (int i = validCount; i < colliders.Length; i++)
            {
                colliders[i] = null;
            }

            return validCount;
        }
    }
}
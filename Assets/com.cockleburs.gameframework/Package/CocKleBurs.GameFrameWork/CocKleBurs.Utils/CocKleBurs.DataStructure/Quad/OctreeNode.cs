using UnityEngine;
using System.Collections.Generic;


namespace CockleBurs.GameFramework.Utility
{
public enum ObjectType { Static, Dynamic }

public class OctreeNode
{
    private Vector3 center; // 节点中心
    private float halfSize; // 节点大小的一半
    private int baseCapacity; // 基础容量
    private int currentCapacity; // 当前容量
    private Dictionary<ObjectType, HashSet<Transform>> objects; // 存储物体按类型分类
    public bool divided; // 是否分割
    public OctreeNode[] children; // 子节点
    private const float densityThreshold = 0.5f; // 分割阈值

    public OctreeNode(Vector3 center, float halfSize, int baseCapacity)
    {
        this.center = center;
        this.halfSize = halfSize;
        this.baseCapacity = baseCapacity;
        this.currentCapacity = baseCapacity;
        this.objects = new Dictionary<ObjectType, HashSet<Transform>>()
        {
            { ObjectType.Static, new HashSet<Transform>() },
            { ObjectType.Dynamic, new HashSet<Transform>() }
        };
        this.divided = false;
        this.children = new OctreeNode[8];
    }

    public void Subdivide()
    {
        float quarterSize = halfSize / 2;
        children[0] = new OctreeNode(center + new Vector3(-quarterSize, quarterSize, -quarterSize), quarterSize, currentCapacity);
        children[1] = new OctreeNode(center + new Vector3(quarterSize, quarterSize, -quarterSize), quarterSize, currentCapacity);
        children[2] = new OctreeNode(center + new Vector3(-quarterSize, -quarterSize, -quarterSize), quarterSize, currentCapacity);
        children[3] = new OctreeNode(center + new Vector3(quarterSize, -quarterSize, -quarterSize), quarterSize, currentCapacity);
        children[4] = new OctreeNode(center + new Vector3(-quarterSize, quarterSize, quarterSize), quarterSize, currentCapacity);
        children[5] = new OctreeNode(center + new Vector3(quarterSize, quarterSize, quarterSize), quarterSize, currentCapacity);
        children[6] = new OctreeNode(center + new Vector3(-quarterSize, -quarterSize, quarterSize), quarterSize, currentCapacity);
        children[7] = new OctreeNode(center + new Vector3(quarterSize, -quarterSize, quarterSize), quarterSize, currentCapacity);
        divided = true;
    }

    public bool Insert(Transform obj, ObjectType type)
    {
        if (obj == null || !Contains(obj.position))
            return false;

        AdjustCapacity();

        if (objects[type].Count < currentCapacity)
        {
            objects[type].Add(obj);
            return true;
        }
        else
        {
            if (!divided)
                Subdivide();

            for (int i = 0; i < 8; i++)
            {
                if (children[i].Insert(obj, type))
                    return true;
            }
        }
        return false;
    }

    public bool Remove(Transform obj, ObjectType type)
    {
        if (obj == null || !Contains(obj.position))
            return false;

        if (objects[type].Remove(obj))
        {
            if (divided && TotalObjectCount() < currentCapacity * densityThreshold)
            {
                Merge();
            }
            return true;
        }

        if (divided)
        {
            for (int i = 0; i < 8; i++)
            {
                if (children[i].Remove(obj, type))
                {
                    if (children[i].IsEmpty() && CanMerge())
                    {
                        Merge();
                    }
                    return true;
                }
            }
        }
        return false;
    }

    public void Update(Transform obj, Vector3 newPosition, ObjectType type)
    {
        if (obj == null) return;

        if (!Contains(newPosition))
        {
            Remove(obj, type);
            Insert(obj, type);
        }
    }

    private void AdjustCapacity()
    {
        if (TotalObjectCount() > currentCapacity)
        {
            currentCapacity++;
        }
        else if (currentCapacity > baseCapacity && TotalObjectCount() < currentCapacity * densityThreshold)
        {
            currentCapacity--;
        }
    }

    private bool Contains(Vector3 position)
    {
        return (position.x >= center.x - halfSize && position.x <= center.x + halfSize &&
                position.y >= center.y - halfSize && position.y <= center.y + halfSize &&
                position.z >= center.z - halfSize && position.z <= center.z + halfSize);
    }

    public void Retrieve(List<Transform> returnObjects, Vector3 point, float range, ObjectType type)
    {
        if (!Contains(point))
            return;

        foreach (Transform obj in objects[type])
        {
            if (obj != null && Vector3.Distance(obj.position, point) <= range)
            {
                returnObjects.Add(obj);
            }
        }

        if (divided)
        {
            foreach (var child in children)
            {
                child.Retrieve(returnObjects, point, range, type);
            }
        }
    }

    public void Clear()
    {
        objects[ObjectType.Static].Clear();
        objects[ObjectType.Dynamic].Clear();
        if (divided)
        {
            foreach (var child in children)
            {
                child.Clear();
            }
            divided = false;
        }
    }

    public bool IsEmpty() => TotalObjectCount() == 0;

    private bool CanMerge()
    {
        for (int i = 0; i < 8; i++)
        {
            if (children[i] != null && !children[i].IsEmpty())
                return false;
        }
        return true;
    }

    private void Merge()
    {
        foreach (var child in children)
        {
            if (child != null)
            {
                foreach (var type in (ObjectType[])System.Enum.GetValues(typeof(ObjectType)))
                {
                    foreach (var obj in child.objects[type])
                    {
                        objects[type].Add(obj);
                    }
                }
                child.Clear();
            }
        }
        divided = false;
    }

    private int TotalObjectCount()
    {
        int count = 0;
        foreach (var type in (ObjectType[])System.Enum.GetValues(typeof(ObjectType)))
        {
            count += objects[type].Count;
        }
        return count;
    }

    public Vector3 GetCenter() => center;
    public float GetHalfSize() => halfSize;
    public Dictionary<ObjectType, HashSet<Transform>> GetObjects() => objects;

    public int GetObjectCount(ObjectType type) => objects[type].Count;

    public void PerformFrustumCulling(Camera camera)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        if (Contains(camera.transform.position))
        {
            foreach (var obj in objects[ObjectType.Dynamic])
            {
                if (obj != null && obj.gameObject.activeInHierarchy) // 检查 Transform 是否有效且对象激活
                {
                    Renderer objRenderer = obj.GetComponent<Renderer>();
                    if (objRenderer != null) // 检查 Renderer 是否存在
                    {
                        if (GeometryUtility.TestPlanesAABB(planes, objRenderer.bounds))
                        {
                            obj.gameObject.SetActive(true);
                        }
                        else
                        {
                            obj.gameObject.SetActive(false);
                        }
                    }
                }
            }

            if (divided)
            {
                foreach (var child in children)
                {
                    if (child != null) // 检查子节点是否有效
                    {
                        child.PerformFrustumCulling(camera);
                    }
                }
            }
        }
    }


    public int GetChildCount()
    {
        if (divided)
        {
            int count = 0;
            foreach (var child in children)
            {
                count += child.GetChildCount();
            }
            return count + 1; // 包括当前节点
        }
        return 1; // 当前节点
    }
}

}
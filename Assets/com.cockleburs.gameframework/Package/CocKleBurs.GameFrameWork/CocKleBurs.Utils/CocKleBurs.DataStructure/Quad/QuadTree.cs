using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
|-------------------------------------------------|
| QuadTree utility to hold generic value at point |
|-------------------------------------------------|

Usage example: Performant way to check if two players/transform etc. are within certain range of each other.
Does need to be implemented properly for real benefit with a high number of points (changing points obviously need to be updated)
*/
namespace CockleBurs.GameFramework.Utility
{

    public class QuadTreePoint<T>{
        public Vector2 point;
        public T data;
        public QuadTreePoint(Vector2 p, T d){
            point = p;
            data = d;
        }
    }

    public class QuadTreeNode<T>{
        public Vector2 center;
        public float size;
        public List<QuadTreePoint<T>> points;
        public int capacity;
        public bool hasSubdivided = false;
        public QuadTreeNode<T> topLeft;
        public QuadTreeNode<T> topRight;
        public QuadTreeNode<T> bottomLeft;
        public QuadTreeNode<T> bottomRight;
        public QuadTreeNode(Vector2 p, float s, int c = 4){
            center = p;
            size = s;
            capacity = c;
            points = new List<QuadTreePoint<T>>();
        }

    }

    public class QuadTree<T>{
        public QuadTreeNode<T> root;
        public QuadTree(Vector2 center, float size, int capacity = 4){
            root = new QuadTreeNode<T>(center, size, capacity);
        }

        public void Insert(Vector2 point, T data){
            Insert(root, point, data);
        }

        private void Insert(QuadTreeNode<T> node, Vector2 point, T data){
            if (!IsPointInNode(node, point)){
                return;
            }
            if (node.points.Count < node.capacity){
                node.points.Add(new QuadTreePoint<T>(point, data));
            }
            else{
                if (!node.hasSubdivided){
                    Subdivide(node);
                }
                Insert(node.topLeft, point, data);
                Insert(node.topRight, point, data);
                Insert(node.bottomLeft, point, data);
                Insert(node.bottomRight, point, data);
            }
        }

        private void Subdivide(QuadTreeNode<T> node){
            float newSize = node.size / 2;
            float halfSize = node.size / 4;
            node.topLeft = new QuadTreeNode<T>(new Vector2(node.center.x - halfSize, node.center.y + halfSize), newSize, node.capacity);
            node.topRight = new QuadTreeNode<T>(new Vector2(node.center.x + halfSize, node.center.y + halfSize), newSize, node.capacity);
            node.bottomLeft = new QuadTreeNode<T>(new Vector2(node.center.x - halfSize, node.center.y - halfSize), newSize, node.capacity);
            node.bottomRight = new QuadTreeNode<T>(new Vector2(node.center.x + halfSize, node.center.y - halfSize), newSize, node.capacity);
            node.hasSubdivided = true;
        }

        private bool IsPointInNode(QuadTreeNode<T> node, Vector2 point)
        {
            float halfSize = node.size / 2;
            return (point.x >= node.center.x - halfSize && point.x <= node.center.x + halfSize &&
                    point.y >= node.center.y - halfSize && point.y <= node.center.y + halfSize);
        }

        private bool IsNodeIntersectingRectangle(QuadTreeNode<T> node, Vector2 rectangleCenter, Vector2 rectangleSize)
        {
            float halfWidth = rectangleSize.x / 2;
            float halfHeight = rectangleSize.y / 2;

            return !(node.center.x + node.size / 2 < rectangleCenter.x - halfWidth ||
                    node.center.x - node.size / 2 > rectangleCenter.x + halfWidth ||
                    node.center.y + node.size / 2 < rectangleCenter.y - halfHeight ||
                    node.center.y - node.size / 2 > rectangleCenter.y + halfHeight);
        }


        public List<QuadTreePoint<T>> FetchInRange(Vector2 center, Vector2 rangeSize)
        {
            HashSet<QuadTreePoint<T>> points = new HashSet<QuadTreePoint<T>>();
            FetchInRange(root, points, center, rangeSize);
            return points.ToList();
        }

        private void FetchInRange(QuadTreeNode<T> node, HashSet<QuadTreePoint<T>> points, Vector2 center, Vector2 rangeSize)
        {
            if (node == null)
            {
                return;
            }

            if (IsNodeIntersectingRectangle(node, center, rangeSize))
            {
                foreach (QuadTreePoint<T> point in node.points)
                {
                    if (Mathf.Abs(center.x - point.point.x) <= rangeSize.x / 2 &&
                        Mathf.Abs(center.y - point.point.y) <= rangeSize.y / 2)
                    {
                        points.Add(point);
                    }
                }

                if (node.hasSubdivided)
                {
                    FetchInRange(node.topLeft, points, center, rangeSize);
                    FetchInRange(node.topRight, points, center, rangeSize);
                    FetchInRange(node.bottomLeft, points, center, rangeSize);
                    FetchInRange(node.bottomRight, points, center, rangeSize);
                }
            }
        }

        private bool IsNodeIntersectingBounds(QuadTreeNode<T> node, Vector2 center, float size){
            float halfSize = size / 2;
            if (node.center.x + node.size / 2 < center.x - halfSize){
                return false;
            }
            if (node.center.x - node.size / 2 > center.x + halfSize){
                return false;
            }
            if (node.center.y + node.size / 2 < center.y - halfSize){
                return false;
            }
            if (node.center.y - node.size / 2 > center.y + halfSize){
                return false;
            }
            return true;
        }

        public void Clear()
        {
            root.points.Clear();
            root.hasSubdivided = false;
            root.topLeft = null;
            root.topRight = null;
            root.bottomLeft = null;
            root.bottomRight = null;
        }

    }
}
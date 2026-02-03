using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CockleBurs.GameFramework.Utility
{
/*
Testing utility for quadtrees includes easy mouse add point and visualizes the data with Quadtree draw methods
*/
public class QuadTreeDebug : MonoBehaviour
{
    [SerializeField]
    private Vector2 center = Vector2.zero;
    [SerializeField]
    private float size = 10;
    [SerializeField]
    private float pointSize = 0.5f;
    [SerializeField]
    private Color pointColor = Color.red;
    [SerializeField]
    private Color selectedColor = Color.green;
    [SerializeField]
    private Color nodeColor = Color.blue;

    QuadTree<Color> quadTree;
    bool isSelecting = false;
    Vector2 selectionStart;
    Vector2 selectionEnd;

    private void Awake()
    {
        quadTree = new QuadTree<Color>(center, size, 3);
    }

    private void Update()
    {
        // Insert new point on left mouse button click
        if (Input.GetMouseButtonDown(0))
        {
            // Convert mouse position to world space
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Insert new point into Quadtree
            quadTree.Insert(mousePosition, Color.red);

            Debug.Log("Mouse position: " + mousePosition);
        }

        // Select points inside box on right mouse button click
        if (Input.GetMouseButtonDown(1))
        {
            if (!isSelecting)
            {
                // Start selecting
                isSelecting = true;
                selectionStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
            else
            {
                // End selecting
                isSelecting = false;
                selectionEnd = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                // Find all points inside the box
                Vector2 selectionCenter = (selectionStart + selectionEnd) / 2;
                float rangeWidth = Mathf.Abs(selectionEnd.x - selectionStart.x);
                float rangeHeight = Mathf.Abs(selectionEnd.y - selectionStart.y);
                float rangeRadius = Mathf.Max(rangeWidth, rangeHeight) / 2;

                List<QuadTreePoint<Color>> points = quadTree.FetchInRange(selectionCenter, new Vector2(rangeWidth, rangeHeight));

               // Set all points in whole tree red
                foreach (QuadTreePoint<Color> point in quadTree.FetchInRange(quadTree.root.center, new Vector2(quadTree.root.size * 2, quadTree.root.size * 2)))
                {
                    point.data = Color.red;
                }
                int count = 0;
                // Change color of all points inside the box to green
                foreach (QuadTreePoint<Color> point in points)
                {
                    count++;
                    point.data = Color.green;
                }
                Debug.Log("Points inside box: " + count);
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Only draw Gizmos in play mode
        if (!Application.isPlaying)
        {
            return;
        }

        DrawNode(quadTree.root);

        // Draw selection box if selecting
        if (isSelecting)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Gizmos.color = selectedColor;
            Gizmos.DrawWireCube(new Vector2((selectionStart.x + mousePosition.x) / 2, (selectionStart.y + mousePosition.y) / 2), new Vector3(Mathf.Abs(mousePosition.x - selectionStart.x), Mathf.Abs(mousePosition.y - selectionStart.y), 0));
        }
    }

    private void DrawNode(QuadTreeNode<Color> node)
    {
        if (node == null)
        {
            return;
        }

        // Draw node bounds
        Gizmos.color = nodeColor;
        Gizmos.DrawWireCube(node.center, new Vector3(node.size, node.size, 0));

        // Draw points
        foreach (QuadTreePoint<Color> point in node.points)
        {
            Gizmos.color = point.data;
            Gizmos.DrawSphere(point.point, pointSize);
        }
         // Recursively draw child nodes
        if (node.hasSubdivided)
        {
            DrawNode(node.topLeft);
            DrawNode(node.topRight);
            DrawNode(node.bottomLeft);
            DrawNode(node.bottomRight);
        }
    }
}



}
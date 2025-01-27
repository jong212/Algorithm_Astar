using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
public class Node
{
    public bool walkable;         // �ش� ��带 ������ �� �ִ��� ����
    public Vector3 worldPosition; // ����� ���� ��ǥ
    public int gridX;             // �׸��� ���� X �ε���
    public int gridY;             // �׸��� ���� Y �ε���

    public int gCost;             // ���� ���κ����� ���
    public int hCost;             // ��ǥ �������� ���� ���
    public int fCost { get { return gCost + hCost; } } // �� ���

    public Node parent;           // ��� ������ ���� �θ� ���

    public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY)
    {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
    }
}
public class Grid : MonoBehaviour
{
    public LayerMask unwalkableMask;    // �̵� �Ұ����� ���̾� ����ũ
    public Vector2 gridWorldSize;       // �׸����� ���� ũ�� (����, ����)
    public float nodeRadius = 0.5f;     // ����� ������ (�⺻�� 0.5)

    public List<Node> path;             // ���� ��θ� ������ ����Ʈ

    Node[,] grid;                        // ��� �迭

    float nodeDiameter;
    int gridSizeX, gridSizeY;

    void Awake()
    {
        // nodeRadius�� 0 ���Ϸ� �������� �ʵ��� Ȯ��
        if (nodeRadius <= 0)
        {
            nodeRadius = 0.5f;
        }

        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        // gridSizeX�� gridSizeY�� ������� Ȯ��
        if (gridSizeX <= 0 || gridSizeY <= 0)
        {
            //Debug.LogError("gridSizeX �Ǵ� gridSizeY�� 0 �����Դϴ�. gridWorldSize�� nodeRadius�� Ȯ���ϼ���.");
            return;
        }

        CreateGrid();
    }

    void CreateGrid()
    {
        // ��� Ÿ���� 2���� �迭 �ʱ�ȭ
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position -
            Vector3.right * gridWorldSize.x / 2 -
            Vector3.forward * gridWorldSize.y / 2;

        // ��� �߽� ���ؼ� grid ������ �ֱ�
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft +
                    Vector3.right * (x * nodeDiameter + nodeRadius) +
                    Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    // ���� ��ǥ�� �׸��� ���� ���� ��ȯ
    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        if (grid == null)
        {
            return null;
        }

        // ���� ��ǥ�� �׸��� ������ ��� ������ ��ġ�ϴ��� ���
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;

        // ������ 0~1 ������ ����� �ʵ��� Ŭ����
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        // ������ ������� �׸��� �迭�� �ε��� ���
        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        // �ε����� �׸��� ũ�⸦ ����� �ʵ��� ���� ����
        x = Mathf.Clamp(x, 0, gridSizeX - 1);
        y = Mathf.Clamp(y, 0, gridSizeY - 1);

        return grid[x, y];
    }


    // �̿� ��� �������� (8���� Ž��)
    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // �ڱ� �ڽ��� ����
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                // �׸��� ���� ������ Ȯ��
                if (checkX >= 0 && checkX < gridSizeX &&
                    checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;                                                                 // 1. �׸��� ������ üũ �ϴ� �� ( �ٱ� �׵θ� �ʷϻ� )
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y)); // 1. �׸��� ������ üũ �ϴ� �� ( �ٱ� �׵θ� �ʷϻ� )

        if (grid != null)
        {
            foreach (Node node in grid)
            {
                Gizmos.color = node.walkable ? Color.white : Color.red;                             // 2. �׸��� �� ��� ��忡 ���� ���� �� �ִ� ������� �Ǵ� ( ���簢�� )
                Gizmos.DrawWireCube(node.worldPosition, new Vector3(1,0.1f,1));                     // 2. �׸��� �� ��� ��忡 ���� ���� �� �ִ� ������� �Ǵ� ( ���簢�� )

                Gizmos.color = Color.white;                                                          // 3. ��� ���� ( ���� ��, ȸ�� )
                Gizmos.DrawSphere(node.worldPosition, 0.1f);                                         // 3. ��� ���� ( ���� ��, ȸ�� )
            }
        }


        if(path != null)
        {
            Gizmos.color = Color.black;                                                             // 4. ��� ����Ʈ (������ ū �� �� ��)
            for (int i = 0; i < path.Count; i++)
            {
                

                Gizmos.DrawSphere(path[i].worldPosition, 0.3f);                                      // 4. ��� ����Ʈ (������ ū �� �� ��)

                // ���� ���� �����ϴ� �� �׸���
                if (i < path.Count - 1)
                {
                    Gizmos.DrawLine(path[i].worldPosition, path[i + 1].worldPosition);               // 4. ��� ����Ʈ (������ ū �� �� ��)
                }
            }
        }
 
    }
}

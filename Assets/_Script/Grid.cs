using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
public class Node
{
    public bool walkable;         // 해당 노드를 지나갈 수 있는지 여부
    public Vector3 worldPosition; // 노드의 월드 좌표
    public int gridX;             // 그리드 상의 X 인덱스
    public int gridY;             // 그리드 상의 Y 인덱스

    public int gCost;             // 시작 노드로부터의 비용
    public int hCost;             // 목표 노드까지의 예상 비용
    public int fCost { get { return gCost + hCost; } } // 총 비용

    public Node parent;           // 경로 추적을 위한 부모 노드

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
    public LayerMask unwalkableMask;    // 이동 불가능한 레이어 마스크
    public Vector2 gridWorldSize;       // 그리드의 월드 크기 (가로, 세로)
    public float nodeRadius = 0.5f;     // 노드의 반지름 (기본값 0.5)

    public List<Node> path;             // 현재 경로를 저장할 리스트

    Node[,] grid;                        // 노드 배열

    float nodeDiameter;
    int gridSizeX, gridSizeY;

    void Awake()
    {
        // nodeRadius가 0 이하로 설정되지 않도록 확인
        if (nodeRadius <= 0)
        {
            nodeRadius = 0.5f;
        }

        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        // gridSizeX와 gridSizeY가 양수인지 확인
        if (gridSizeX <= 0 || gridSizeY <= 0)
        {
            //Debug.LogError("gridSizeX 또는 gridSizeY가 0 이하입니다. gridWorldSize와 nodeRadius를 확인하세요.");
            return;
        }

        CreateGrid();
    }

    void CreateGrid()
    {
        // 노드 타입의 2차원 배열 초기화
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position -
            Vector3.right * gridWorldSize.x / 2 -
            Vector3.forward * gridWorldSize.y / 2;

        // 노드 중심 구해서 grid 변수에 넣기
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

    // 월드 좌표를 그리드 상의 노드로 변환
    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        if (grid == null)
        {
            return null;
        }

        // 월드 좌표가 그리드 내에서 어느 비율에 위치하는지 계산
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;

        // 비율이 0~1 범위를 벗어나지 않도록 클램핑
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        // 비율을 기반으로 그리드 배열의 인덱스 계산
        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        // 인덱스가 그리드 크기를 벗어나지 않도록 범위 제한
        x = Mathf.Clamp(x, 0, gridSizeX - 1);
        y = Mathf.Clamp(y, 0, gridSizeY - 1);

        return grid[x, y];
    }


    // 이웃 노드 가져오기 (8방향 탐색)
    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // 자기 자신은 제외
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                // 그리드 범위 내인지 확인
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
        Gizmos.color = Color.cyan;                                                                 // 1. 그리드 사이즈 체크 하는 선 ( 바깥 테두리 초록색 )
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y)); // 1. 그리드 사이즈 체크 하는 선 ( 바깥 테두리 초록색 )

        if (grid != null)
        {
            foreach (Node node in grid)
            {
                Gizmos.color = node.walkable ? Color.white : Color.red;                             // 2. 그리드 내 모든 노드에 대해 걸을 수 있는 노드인지 판단 ( 정사각형 )
                Gizmos.DrawWireCube(node.worldPosition, new Vector3(1,0.1f,1));                     // 2. 그리드 내 모든 노드에 대해 걸을 수 있는 노드인지 판단 ( 정사각형 )

                Gizmos.color = Color.white;                                                          // 3. 노드 센터 ( 작은 원, 회색 )
                Gizmos.DrawSphere(node.worldPosition, 0.1f);                                         // 3. 노드 센터 ( 작은 원, 회색 )
            }
        }


        if(path != null)
        {
            Gizmos.color = Color.black;                                                             // 4. 경로 리스트 (검은색 큰 원 및 선)
            for (int i = 0; i < path.Count; i++)
            {
                

                Gizmos.DrawSphere(path[i].worldPosition, 0.3f);                                      // 4. 경로 리스트 (검은색 큰 원 및 선)

                // 다음 노드와 연결하는 선 그리기
                if (i < path.Count - 1)
                {
                    Gizmos.DrawLine(path[i].worldPosition, path[i + 1].worldPosition);               // 4. 경로 리스트 (검은색 큰 원 및 선)
                }
            }
        }
 
    }
}

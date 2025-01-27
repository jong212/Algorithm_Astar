// Pathfinding.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.EventSystems.EventTrigger;

public class Pathfinding : MonoBehaviour
{
    
    private Grid grid;

    // 경로가 업데이트될 때 호출되는 이벤트
    public event Action<List<Node>> OnPathUpdated;

    void Awake()
    {
        grid = FindObjectOfType<Grid>();
    }
/*    List<Node> openSet = new List<Node>();          // 코루틴 테스트 용 전역 변수
    HashSet<Node> closedSet = new HashSet<Node>();  // 코루틴 테스트 용 전역 변수*/
    public IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        if (startNode == null || targetNode == null)
        {
            Debug.LogError("시작 노드 또는 목표 노드가 null입니다.");
            OnPathUpdated?.Invoke(null);
            yield break; // 코루틴 종료
        }

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
          //  yield return new WaitForSeconds(1f);  // 1초 대기

            Node currentNode = openSet[0];

            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||
                    openSet[i].fCost == currentNode.fCost &&
                    openSet[i].hCost < currentNode.hCost)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                RetracePath(startNode, targetNode);
                yield break; // 코루틴 종료
            }

            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
               // yield return new WaitForSeconds(1f);  // 1초 대기
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                    continue;

                int newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);

                if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    if(newCostToNeighbour < neighbour.gCost)
                    {
                        Debug.Log(newCostToNeighbour + "dd" + neighbour.gCost);
                    }
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }

        }

        //Debug.LogWarning("경로를 찾지 못했습니다.");
        //OnPathUpdated?.Invoke(null);
    }

    void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
            if (currentNode == null)
            {
                Debug.LogError("경로 추적 중 부모 노드가 null입니다.");
                OnPathUpdated?.Invoke(null);
                return;
            }
        }
        path.Reverse();

        grid.path = path;
        if (path.Count == 0) return;
        //Debug.Log("경로가 생성되었습니다. 노드 수: " + path.Count);

        // 경로 업데이트 이벤트 호출
        //OnPathUpdated?.Invoke(path);
    }
    void OnDrawGizmos()
    {
        /*if (openSet != null)
        {
            foreach (Node node in openSet)
            {
                Gizmos.color = Color.green; // openSet에 있는 노드는 초록색으로 표시
                Gizmos.DrawCube(node.worldPosition, Vector3.one); // 노드 위치에 큐브 그리기
            }
        }

        if (closedSet != null)
        {
            foreach (Node node in closedSet)
            {
                Gizmos.color = Color.red; // closedSet에 있는 노드는 빨간색으로 표시
                Gizmos.DrawCube(node.worldPosition, Vector3.one);
            }
        }*/

    }


    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

}

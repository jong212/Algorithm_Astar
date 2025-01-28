using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    
    private Grid grid;

    void Awake()
    {
        grid = FindObjectOfType<Grid>();
    }
                      //List<Node> openSet = new List<Node>();          // 코루틴 테스트 용 전역 변수
                    //HashSet<Node> closedSet = new HashSet<Node>();  // 코루틴 테스트 용 전역 변수
    //Node currentNode;
    public IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        // 시작 노드와 목표 노드를 월드 좌표에서 가져옴        
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        // 노드가 null인 경우 에러 처리 후 코루틴 종료
        if (startNode == null || targetNode == null)
        {
            Debug.LogError("시작 노드 또는 목표 노드가 null입니다.");
            yield break; // 코루틴 종료
        }
               
        List<Node> openSet = new List<Node>();                          // 오픈 리스트(openSet) 생성, 탐색할 노드 목록  
        HashSet<Node> closedSet = new HashSet<Node>();                  // 클로즈드 리스트(closedSet) 생성, 이미 탐색된 노드 목록

        openSet.Add(startNode);                                         // 시작 노드를 오픈 리스트에 추가

        while (openSet.Count > 0)                                       // 오픈 리스트에 노드가 남아 있는 동안 반복
        {
            //yield return new WaitForSeconds(.5f);
            Node currentNode = openSet[0];                              // 오픈 리스트에서 가장 비용이 낮은 노드 선택
            //currentNode = openSet[0];                              // 오픈 리스트에서 가장 비용이 낮은 노드 선택

            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||             // fCost가 더 낮거나
                    openSet[i].fCost == currentNode.fCost &&
                    openSet[i].hCost < currentNode.hCost)               // fCost가 같더라도 hCost가 더 낮으면
                {
                    currentNode = openSet[i];
                }
            }
            openSet.Remove(currentNode);                                 // 선택된 노드를 오픈 리스트에서 제거
            closedSet.Add(currentNode);                                  // 클로즈드 리스트에 추가

            // 목표 노드에 도달한 경우 경로를 추적하고 코루틴 종료
            if (currentNode == targetNode)
            {
                RetracePath(startNode, targetNode); // 경로를 추적
                yield break; // 코루틴 종료
            }

            // 현재 노드의 이웃 노드 탐색
            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                // 이웃 노드가 벽이거나 이미 탐색된 경우 건너뜀
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                    continue;
                //yield return new WaitForSeconds(.3f);  // 1초 대기
                // 현재 노드를 거쳐서 이웃 노드에 도달하는 비용 계산
                int newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);

                // 새로 계산된 비용이 기존 비용보다 작거나, 오픈 리스트에 없는 경우 업데이트
                if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newCostToNeighbour; // gCost 업데이트
                    neighbour.hCost = GetDistance(neighbour, targetNode); // hCost 업데이트
                    neighbour.parent = currentNode; // 부모 노드 설정 (경로 추적을 위해)

                    // 이웃 노드가 오픈 리스트에 없으면 추가
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
        // 경로를 저장할 리스트 생성
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        // 시작 노드까지 부모 노드를 따라가며 경로를 추적
        while (currentNode != startNode)
        {
            path.Add(currentNode); // 현재 노드를 경로에 추가
            currentNode = currentNode.parent; // 부모 노드로 이동

            // 부모 노드가 null인 경우 에러 처리
            if (currentNode == null)
            {
                Debug.LogError("경로 추적 중 부모 노드가 null입니다.");
                return;
            }
        }
        path.Reverse(); // 역순으로 추가했으므로 뒤집어서 올바른 경로로 만듦

        grid.path = path; // 그리드에 경로 설정
        if (path.Count == 0) return; // 경로가 없으면 종료
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
        }

        if (currentNode != null)
        {
           
                Gizmos.color = Color.white; // closedSet에 있는 노드는 빨간색으로 표시
                Gizmos.DrawCube(currentNode.worldPosition, Vector3.one);
        }
*/
    }


    int GetDistance(Node nodeA, Node nodeB)
    {
        // 두 노드 간의 거리 계산
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        
        // 대각선 이동이 더 비용이 크므로 우선적으로 계산
        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

}

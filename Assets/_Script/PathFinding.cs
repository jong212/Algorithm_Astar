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

    // ��ΰ� ������Ʈ�� �� ȣ��Ǵ� �̺�Ʈ
    public event Action<List<Node>> OnPathUpdated;

    void Awake()
    {
        grid = FindObjectOfType<Grid>();
    }
/*    List<Node> openSet = new List<Node>();          // �ڷ�ƾ �׽�Ʈ �� ���� ����
    HashSet<Node> closedSet = new HashSet<Node>();  // �ڷ�ƾ �׽�Ʈ �� ���� ����*/
    public IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        if (startNode == null || targetNode == null)
        {
            Debug.LogError("���� ��� �Ǵ� ��ǥ ��尡 null�Դϴ�.");
            OnPathUpdated?.Invoke(null);
            yield break; // �ڷ�ƾ ����
        }

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
          //  yield return new WaitForSeconds(1f);  // 1�� ���

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
                yield break; // �ڷ�ƾ ����
            }

            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
               // yield return new WaitForSeconds(1f);  // 1�� ���
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

        //Debug.LogWarning("��θ� ã�� ���߽��ϴ�.");
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
                Debug.LogError("��� ���� �� �θ� ��尡 null�Դϴ�.");
                OnPathUpdated?.Invoke(null);
                return;
            }
        }
        path.Reverse();

        grid.path = path;
        if (path.Count == 0) return;
        //Debug.Log("��ΰ� �����Ǿ����ϴ�. ��� ��: " + path.Count);

        // ��� ������Ʈ �̺�Ʈ ȣ��
        //OnPathUpdated?.Invoke(path);
    }
    void OnDrawGizmos()
    {
        /*if (openSet != null)
        {
            foreach (Node node in openSet)
            {
                Gizmos.color = Color.green; // openSet�� �ִ� ���� �ʷϻ����� ǥ��
                Gizmos.DrawCube(node.worldPosition, Vector3.one); // ��� ��ġ�� ť�� �׸���
            }
        }

        if (closedSet != null)
        {
            foreach (Node node in closedSet)
            {
                Gizmos.color = Color.red; // closedSet�� �ִ� ���� ���������� ǥ��
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

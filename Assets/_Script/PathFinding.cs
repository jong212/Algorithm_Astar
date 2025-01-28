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
                      //List<Node> openSet = new List<Node>();          // �ڷ�ƾ �׽�Ʈ �� ���� ����
                    //HashSet<Node> closedSet = new HashSet<Node>();  // �ڷ�ƾ �׽�Ʈ �� ���� ����
    //Node currentNode;
    public IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        // ���� ���� ��ǥ ��带 ���� ��ǥ���� ������        
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        // ��尡 null�� ��� ���� ó�� �� �ڷ�ƾ ����
        if (startNode == null || targetNode == null)
        {
            Debug.LogError("���� ��� �Ǵ� ��ǥ ��尡 null�Դϴ�.");
            yield break; // �ڷ�ƾ ����
        }
               
        List<Node> openSet = new List<Node>();                          // ���� ����Ʈ(openSet) ����, Ž���� ��� ���  
        HashSet<Node> closedSet = new HashSet<Node>();                  // Ŭ����� ����Ʈ(closedSet) ����, �̹� Ž���� ��� ���

        openSet.Add(startNode);                                         // ���� ��带 ���� ����Ʈ�� �߰�

        while (openSet.Count > 0)                                       // ���� ����Ʈ�� ��尡 ���� �ִ� ���� �ݺ�
        {
            //yield return new WaitForSeconds(.5f);
            Node currentNode = openSet[0];                              // ���� ����Ʈ���� ���� ����� ���� ��� ����
            //currentNode = openSet[0];                              // ���� ����Ʈ���� ���� ����� ���� ��� ����

            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||             // fCost�� �� ���ų�
                    openSet[i].fCost == currentNode.fCost &&
                    openSet[i].hCost < currentNode.hCost)               // fCost�� ������ hCost�� �� ������
                {
                    currentNode = openSet[i];
                }
            }
            openSet.Remove(currentNode);                                 // ���õ� ��带 ���� ����Ʈ���� ����
            closedSet.Add(currentNode);                                  // Ŭ����� ����Ʈ�� �߰�

            // ��ǥ ��忡 ������ ��� ��θ� �����ϰ� �ڷ�ƾ ����
            if (currentNode == targetNode)
            {
                RetracePath(startNode, targetNode); // ��θ� ����
                yield break; // �ڷ�ƾ ����
            }

            // ���� ����� �̿� ��� Ž��
            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                // �̿� ��尡 ���̰ų� �̹� Ž���� ��� �ǳʶ�
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                    continue;
                //yield return new WaitForSeconds(.3f);  // 1�� ���
                // ���� ��带 ���ļ� �̿� ��忡 �����ϴ� ��� ���
                int newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);

                // ���� ���� ����� ���� ��뺸�� �۰ų�, ���� ����Ʈ�� ���� ��� ������Ʈ
                if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newCostToNeighbour; // gCost ������Ʈ
                    neighbour.hCost = GetDistance(neighbour, targetNode); // hCost ������Ʈ
                    neighbour.parent = currentNode; // �θ� ��� ���� (��� ������ ����)

                    // �̿� ��尡 ���� ����Ʈ�� ������ �߰�
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
        // ��θ� ������ ����Ʈ ����
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        // ���� ������ �θ� ��带 ���󰡸� ��θ� ����
        while (currentNode != startNode)
        {
            path.Add(currentNode); // ���� ��带 ��ο� �߰�
            currentNode = currentNode.parent; // �θ� ���� �̵�

            // �θ� ��尡 null�� ��� ���� ó��
            if (currentNode == null)
            {
                Debug.LogError("��� ���� �� �θ� ��尡 null�Դϴ�.");
                return;
            }
        }
        path.Reverse(); // �������� �߰������Ƿ� ����� �ùٸ� ��η� ����

        grid.path = path; // �׸��忡 ��� ����
        if (path.Count == 0) return; // ��ΰ� ������ ����
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
        }

        if (currentNode != null)
        {
           
                Gizmos.color = Color.white; // closedSet�� �ִ� ���� ���������� ǥ��
                Gizmos.DrawCube(currentNode.worldPosition, Vector3.one);
        }
*/
    }


    int GetDistance(Node nodeA, Node nodeB)
    {
        // �� ��� ���� �Ÿ� ���
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        
        // �밢�� �̵��� �� ����� ũ�Ƿ� �켱������ ���
        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

}

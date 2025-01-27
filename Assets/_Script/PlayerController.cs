using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Pathfinding pathfinding;
    [SerializeField] Transform monsterTransform;




    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StartCoroutine(pathfinding.FindPath(transform.position, monsterTransform.position));
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StumblePlayer : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerMove = other.GetComponent<PlayerMove>();
            if (playerMove)
            {
                playerMove.OnStumble();
            }
        }
    }
}

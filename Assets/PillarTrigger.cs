using System.Collections;
using DG.Tweening;
using UnityEngine;

public class PillarTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pillar"))
        {
            Pillar pillar = other.GetComponent<Pillar>();
            if (pillar != null)
            {
                pillar.StartMoving();
            }
        }
    }
}
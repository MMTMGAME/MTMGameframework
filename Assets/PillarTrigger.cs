using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;

public class PillarTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pillar"))
        {
            other.transform.DOMove(other.transform.position + UnityEngine.Vector3.down * 100, 0.5f);
        }
        
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Pillar"))
        {
            other.transform.DOMove(other.transform.position + UnityEngine.Vector3.up * 100, 0.5f);
        }
        
    }
}

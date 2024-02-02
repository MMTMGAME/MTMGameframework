using System;
using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class ShowInteractTipTest : MonoBehaviour
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
        if (other.CompareTag("Player"))
        {
            //交互提示的内容应该本地化
            GameEntry.EntityUi.ShowUI<InteractTipItem>(other.GetComponent<Entity>(),"交互测试");
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //交互提示的内容应该本地化
            GameEntry.EntityUi.HideUI<InteractTipItem>(other.GetComponent<Entity>());
        }
    }
}

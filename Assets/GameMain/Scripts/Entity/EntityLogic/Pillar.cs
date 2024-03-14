using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GameFramework.Sound;
using GameMain;
using UnityEngine;

public class Pillar : Entity // 假设这里应该是 MonoBehaviour，除非你的 Entity 基类有特殊的逻辑
{

    private bool inPlayerRange;
    private bool isMovingDown = false;

    private Vector3 startPosition;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        startPosition = transform.position;
        
    }


    public void Collapse(Vector3 dir)
    {
        rb.AddTorque(dir);
        rb.angularDrag = 0.15f;
    }
    public void StartMoving()
    {
        if (!isMovingDown)
        {
            isMovingDown = true;
            inPlayerRange = true;
            transform.DOKill();
            // 下降动画
            transform.DOMove(startPosition + Vector3.down * 200, 4f)
                .OnComplete(() => StartCoroutine(WaitAndMoveUp()));
        }
    }

    public void OnExitPlayer()
    {
        inPlayerRange = false;
    }

    IEnumerator WaitAndMoveUp()
    {
        isMovingDown = false;
        yield return new WaitForSeconds(4f); // 等待3秒

        
        while (true)
        {
            yield return null;

            if (inPlayerRange == false)
            {
                transform.DOKill(); // 停止当前的所有动画
                transform.DOMove(startPosition, 4f); // 开始上升动画
                break; // 跳出循环
            }
            
            
        }
            
    }
}
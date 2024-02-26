using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GameMain;
using UnityEngine;

public class Pillar : Entity // 假设这里应该是 MonoBehaviour，除非你的 Entity 基类有特殊的逻辑
{
   
    
    private bool isMovingDown = false;

    public void StartMoving()
    {
        if (!isMovingDown)
        {
            isMovingDown = true;

            transform.DOKill();
            // 下降动画
            transform.DOMove(transform.position + Vector3.down * 200, 4f)
                .OnComplete(() => StartCoroutine(WaitAndMoveUp()));
        }
    }

    IEnumerator WaitAndMoveUp()
    {
        yield return new WaitForSeconds(4f); // 等待3秒

        isMovingDown = false;
        
        // 上升动画
        transform.DOMove(transform.position + Vector3.up * 200, 4f);
    }
}
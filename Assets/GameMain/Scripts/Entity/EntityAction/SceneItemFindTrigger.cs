using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityGameFramework.Runtime;

using GameEntry = GameMain.GameEntry;

public class SceneItemFindTrigger : MonoBehaviour
{

    private PlayerInteract PlayerInteract { get; set; }

    [Header("盒子投射应该使用spine的旋转")]
    public Transform spine;
    
   

    public void Init(PlayerInteract playerInteract)
    {
        this.PlayerInteract = playerInteract;

        
    }

    public SceneItem ShowingItem
    {
        get;
        private set;
    }

    private void Update()
    {
        if (PlayerInteract.GetCurrentTrigger() != this) //不是这只手{
        {
            if (ShowingItem != null)
            {
                GameEntry.InteractTip.HideItem(ShowingItem);
                ShowingItem = null;
            }
            return;
        }

        if (PlayerInteract.GetCurrentHolding() != null) //是这只手但是这只手已经拿着东西了
        {
            if (ShowingItem!=null)
            {
                GameEntry.InteractTip.HideItem(ShowingItem);
                ShowingItem = null;
            }
            return;
        }
            

       
        
        // if(ShowingItem!=null)//是这之手，没有拿东西，但是已经显示了其他东西的拾取提示了
        //     return;


        
        Collider[] ret = new Collider[6];
        var count = Physics.OverlapBoxNonAlloc(transform.position, new Vector3(2, 0.5f, 0.5f), ret, spine.transform.rotation,
            1<<LayerMask.NameToLayer("SceneItem"));
        for (int i = 0; i < count; i++)
        {
            var collider = ret[i];
            var sceneItem = collider.GetComponentInParent<SceneItem>();
            
            
            if (sceneItem != null && sceneItem.sceneItemData.PickAble && sceneItem.holding==false)
            {
                if (ShowingItem != null)
                {
                    GameEntry.InteractTip.HideItem(ShowingItem);
                }
                
                GameEntry.InteractTip.ShowInteractTip(sceneItem,"拾取");
                ShowingItem = sceneItem;
                return;
            }
            
        }
        

       
        GameEntry.InteractTip.HideItem(ShowingItem);
        ShowingItem = null;
        
        
        
    }

    void OnDrawGizmos()
    {
        Vector3 boxSize = new Vector3(4, 1, 1); // 盒子大小
        Quaternion rotation = spine.transform.rotation; // 当前物体的旋转
        Vector3 boxCenter = transform.position; // 盒子中心位置
        
        // 计算盒子的顶点
        Vector3 halfSize = boxSize * 0.5f;
        Vector3[] corners = new Vector3[8];
        corners[0] = boxCenter + rotation * new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);
        corners[1] = boxCenter + rotation * new Vector3(halfSize.x, -halfSize.y, -halfSize.z);
        corners[2] = boxCenter + rotation * new Vector3(halfSize.x, -halfSize.y, halfSize.z);
        corners[3] = boxCenter + rotation * new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
        corners[4] = boxCenter + rotation * new Vector3(-halfSize.x, halfSize.y, -halfSize.z);
        corners[5] = boxCenter + rotation * new Vector3(halfSize.x, halfSize.y, -halfSize.z);
        corners[6] = boxCenter + rotation * new Vector3(halfSize.x, halfSize.y, halfSize.z);
        corners[7] = boxCenter + rotation * new Vector3(-halfSize.x, halfSize.y, halfSize.z);
        
        // 绘制盒子的边
        Gizmos.color = Color.red;
        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]); // 底部
            Gizmos.DrawLine(corners[i + 4], corners[((i + 1) % 4) + 4]); // 顶部
            Gizmos.DrawLine(corners[i], corners[i + 4]); // 侧面
        }
       
    }


    private void OnDisable()
    {
        if (ShowingItem!=null)
        {
            GameEntry.InteractTip.HideItem(ShowingItem);
            
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;
using UnityGameFramework.Runtime;
using Entity = GameMain.Entity;
using GameEntry = GameMain.GameEntry;

public class OldMan : Entity
{
    private Transform cartTransform;
    private Animator animator;

    public Transform lookTrans;

    public bool AngryEnd
    {
        get;
        private set;
    }

    public bool FindPlayerUsingItem
    {
        get;
        private set;
    }

    private GameBase gameBase;
    private Player player;
    private Player Player
    {
        get
        {
            if (player == null)
            {
                player = gameBase.Player;
                if(player!=null)
                    playerInteract = player.GetComponent<PlayerInteract>();
               
            }

            return player;
        }
        set => player = value;
    }
    private PlayerInteract playerInteract;
    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
        animator = GetComponent<Animator>();
        gameBase = (GameEntry.Procedure.CurrentProcedure as ProcedureLevel)?.GetGameBase();
        lookTrans = transform.FindDeep("head");

        AngryEnd = false;
    }

    public void SetCartTrans(Transform target)
    {
        this.cartTransform = target;
    }


    private void Update()
    {
        if(FindPlayerUsingItem==true)
            return;
        
        transform.position = cartTransform.position;
        transform.rotation = cartTransform.rotation;
        
        //checkPlayer
        if(Player==null)
            return;
       
        //角度检测
        var lookDir = new Vector3(lookTrans.forward.x,0, lookTrans.forward.z);//去掉y轴影响
        var angle = Vector3.Angle(lookDir, player.transform.position - transform.position);
        Debug.DrawRay(lookTrans.position, lookTrans.forward*20);
        if (angle < 25 && playerInteract.UsingItem!=null)
        {
            FindPlayerUsingItem = true;
            StartCoroutine(AngryCo());
        }
    }

    IEnumerator AngryCo()
    {
        animator.SetBool("Angry", true);
        animator.applyRootMotion = true;

        var level5Cam = gameBase.SceneCam as SceneCamLevel5;
        level5Cam.SetOldManCam(transform);
        level5Cam.EnableOldManCam();
        float timer = 0;
        while (timer < 0.5f)
        {
            var dir = Player.transform.position - transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10 * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
        
        yield return new WaitForSeconds(2);
        AngryEnd = true;
        Debug.Log("AngryEnd!!!");
    }
}

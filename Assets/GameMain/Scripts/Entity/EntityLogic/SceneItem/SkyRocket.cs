using System;
using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class SkyRocket : SceneItem
{

    public bool died;

    public ParticleSystem particleSystem;

    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
        particleSystem = GetComponentInChildren<ParticleSystem>();
        particleSystem.Stop();
    }

    protected override void OnDead(Entity attacker)
    {
        if(died==true)
            return;

        died = true;
        //base.OnDead(attacker);
       
        GameEntry.Sound.PlaySound(10008,transform.position);
        //RigidbodyComp.AddForce(transform.forward*300f);
        StartCoroutine(Fly());
        (GameEntry.Procedure.CurrentProcedure as ProcedureLevel).GetGameBase().SceneCam.AddToTargetGroup(transform,3);
    }

    IEnumerator Fly()
    {
        //可以和玩家碰撞
        gameObject.layer = Constant.Layer.DefaultLayerId;
        
        //播放滋花特效
        particleSystem.Play();
        yield return new WaitForSeconds(1);
        //RigidbodyComp.isKinematic = false;
        //RigidbodyComp.constraints = RigidbodyConstraints.FreezeRotation;
        float elapsedTime = 0;
        elapsedTime += Time.deltaTime;
        while (elapsedTime < 3)
        {
            elapsedTime += Time.deltaTime;
            //RigidbodyComp.AddForce(transform.up * (15 * Time.deltaTime),ForceMode.VelocityChange);
            //transform.position +=transform.up * (1 * Time.deltaTime);
            transform.Translate(Vector3.up * (1 * Time.deltaTime),Space.Self);
            yield return null;
        }
    
        GameEntry.Entity.HideEntity(this);
        Explosion();
        
    }

    protected override void OnRecycle()
    {
        base.OnRecycle();
        died = false;
    }

    void Explosion()
    {
        AIUtility.ExplosionWithForce(this,transform.position,1,sceneItemData.attack);
        GameEntry.Entity.ShowEffect(new EffectData(GameEntry.Entity.GenerateSerialId(),sceneItemData.deadEffectId)
        {
            Position = transform.position
        });
        GameEntry.Sound.PlaySound(sceneItemData.deadSoundId);
        Invoke(nameof(RemoveFromCam),1);
    }

    void RemoveFromCam()
    {
        (GameEntry.Procedure.CurrentProcedure as ProcedureLevel).GetGameBase().SceneCam.RemoveFromTargetGroup(transform);
        Debug.Log("Remove!!!!");
    }

    private void OnCollisionEnter(Collision collision)
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (holding && other.gameObject.GetComponent<Collider>().gameObject.layer==LayerMask.NameToLayer("Ground"))
        {
            GameEntry.Entity.DetachEntity(this.Entity);
            RigidbodyComp.isKinematic = true;
        }
    }
}

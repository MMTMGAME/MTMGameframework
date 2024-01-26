using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class Firecracker : SceneItem
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
        
        StartCoroutine(Fly());
        (GameEntry.Procedure.CurrentProcedure as ProcedureLevel).GetGameBase().SceneCam.AddToTargetGroup(transform,3);
    }

    IEnumerator Fly()
    {
        //播放滋花特效
        particleSystem.Play();
        GameEntry.Sound.PlaySound(10008,transform.position);
        yield return new WaitForSeconds(3);
        
        

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
        AIUtility.ExplosionWithForce(this,transform.position,0.7f,sceneItemData.attack);
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
}

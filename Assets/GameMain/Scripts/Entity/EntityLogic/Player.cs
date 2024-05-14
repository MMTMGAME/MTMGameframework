using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework;
using GameFramework.Event;
using GameMain;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityGameFramework.Runtime;
using GameEntry = GameMain.GameEntry;

public class Player : BattleUnit
{
    private PlayerData playerData;

    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        playerInputActions.Enable();

        
        Cursor.lockState = CursorLockMode.Locked;
            
        GameEntry.Event.Subscribe(OpenUIFormSuccessEventArgs.EventId,OpenUIFormSuccess);
        GameEntry.Event.Subscribe(CloseUIFormCompleteEventArgs.EventId ,CloseUIFormSuccess);

        playerInputActions.Player.MouseRight.started += StartMouseRight;
        playerInputActions.Player.MouseRight.canceled += CancelMouseRight;

        playerInputActions.Player.MouseLeft.started += StartMouseLeft;
        playerInputActions.Player.MouseLeft.canceled += CancelMouseLeft;
        
    }

    private void OnDisable()
    {
        playerInputActions.Disable();
        
        GameEntry.Event.Unsubscribe(OpenUIFormSuccessEventArgs.EventId,OpenUIFormSuccess);
        GameEntry.Event.Unsubscribe(CloseUIFormCompleteEventArgs.EventId ,CloseUIFormSuccess);

        
        playerInputActions.Player.MouseRight.started -= StartMouseRight;
        playerInputActions.Player.MouseRight.canceled -= CancelMouseRight;

        playerInputActions.Player.MouseLeft.started -= StartMouseLeft;
        playerInputActions.Player.MouseLeft.canceled -= CancelMouseLeft;
        
        
        Cursor.lockState = CursorLockMode.None;
    }
    
    private string pauseFormAssetName = "";
    void OpenUIFormSuccess(object sender,GameEventArgs gameEventArgs)
    {
        OpenUIFormSuccessEventArgs args = (OpenUIFormSuccessEventArgs)gameEventArgs;
        if (args != null && args.UIForm.Logic as PauseForm)
        {
            Cursor.lockState = CursorLockMode.None;
            pauseFormAssetName = args.UIForm.UIFormAssetName;
        }
        if (args != null && args.UIForm.Logic as DialogForm && args.UIForm.Logic.Name.Contains("GameEnd"))
        {
            Cursor.lockState = CursorLockMode.None;
            
        }
    }
    
    void CloseUIFormSuccess(object sender,GameEventArgs gameEventArgs)
    {
        CloseUIFormCompleteEventArgs args = (CloseUIFormCompleteEventArgs)gameEventArgs;
        if (args != null && args.UIFormAssetName.Contains(pauseFormAssetName))
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    

    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
        playerData=userData as PlayerData;

        if (playerData == null)
        {
            Log.Error("PlayerData is Invalid");
            return;
        }
        Name = Utility.Text.Format("Player ({0})", Id);
        
        chaState.LearnSkill(DesingerTables.Skill.data["fire"]);
        chaState.LearnSkill(DesingerTables.Skill.data["spaceMonkeyBall"]);
    }

    protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        

        var horizontal = Input.GetAxis("Horizontal");
        var vertical= Input.GetAxis("Vertical");
        
        transform.Rotate(Vector3.up,30*Time.deltaTime*horizontal *(vertical>=0?1:-1) );
        transform.Translate(Vector3.forward * (2 * (Time.deltaTime * vertical)),Space.Self);
        
    }
    
    void StartMouseLeft(InputAction.CallbackContext callbackContext)
    {
        chaState.CastSkill(chaState.skills[0].model.id);

        // var weapon = GetWeaponByIndex(0);
        // if (weapon != null)
        // {
        //
        //     //weapon.StartFire();
        //     //理论上应该用上面的StartFire，但是目前demo没有接入动画模块，直接触发攻击,接入技能和buff模块后再接入动画
        //     weapon.HandleAnimEvent("Shoot");
        // }
    }
    
    void CancelMouseLeft(InputAction.CallbackContext callbackContext)
    {
        // var weapon = GetWeaponByIndex(0);
        // if (weapon != null)
        // {
        //
        //     weapon.CancelFire();
        // }
    }
    
    void StartMouseRight(InputAction.CallbackContext callbackContext)
    {
        chaState.CastSkill(chaState.skills[1].model.id);
        
        // var weapon = GetWeaponByIndex(1);
        // if (weapon != null)
        // {
        //     //weapon.StartFire();
        //     //理论上应该用上面的StartFire，但是目前demo没有接入动画模块，直接触发攻击,接入技能和buff模块后再接入动画
        //     weapon.HandleAnimEvent("Shoot");
        // }
    }
    
    void CancelMouseRight(InputAction.CallbackContext callbackContext)
    {
        var weapon = GetWeaponByIndex(1);
        if (weapon != null)
        {

            weapon.CancelFire();
        }
    }
    

  
}

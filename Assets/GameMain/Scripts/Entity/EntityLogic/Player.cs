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

    private Camera mainCam;
    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        mainCam=Camera.main;
    }

    private void OnEnable()
    {
        playerInputActions.Enable();

        
        //Cursor.lockState = CursorLockMode.Locked;
            
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
            //改为俯视角了，不再lock了
            //Cursor.lockState = CursorLockMode.None;
            
        }
    }
    
    void CloseUIFormSuccess(object sender,GameEventArgs gameEventArgs)
    {
        CloseUIFormCompleteEventArgs args = (CloseUIFormCompleteEventArgs)gameEventArgs;
        if (args != null && args.UIFormAssetName.Contains(pauseFormAssetName))
        {
            //改为俯视角了，不再lock了
            //Cursor.lockState = CursorLockMode.Locked;
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
        
        
    }

    protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        

        var horizontal = Input.GetAxis("Horizontal");
        var vertical= Input.GetAxis("Vertical");
        
        //transform.Rotate(Vector3.up,30*Time.deltaTime*horizontal *(vertical>=0?1:-1) );
        
        Vector2 cursorPos = Input.mousePosition;
        
        float rotateTo = transform.rotation.eulerAngles.y;
        if (mainCam){
            //先获得主角的屏幕坐标，然后对比鼠标坐标就知道转向了
            Vector2 mScreenPos = RectTransformUtility.WorldToScreenPoint(mainCam, transform.position);
            rotateTo = Mathf.Atan2(cursorPos.x - mScreenPos.x, cursorPos.y - mScreenPos.y) * 180.00f / Mathf.PI;
            chaState.OrderRotateTo(Quaternion.Euler(0,rotateTo,0));
        }

        if (horizontal != 0 || vertical != 0){
            float mSpd = chaState.MoveSpeed;
            Transform cameraTransform = mainCam.transform;  // 获取相机的Transform

            // 忽略相机的X轴旋转
            Vector3 forward = cameraTransform.forward;
            forward.y = 0;
            forward.Normalize();  // 标准化向量，避免斜坡移动

            Vector3 right = cameraTransform.right;
            right.y = 0;
            right.Normalize();  // 标准化向量

            // 基于相机方向计算新的移动向量
            Vector3 mInfo = (forward * vertical + right * horizontal) * mSpd;

            // 让角色移动
            chaState.OrderMove(mInfo);
        }

    }

   
    void StartMouseLeft(InputAction.CallbackContext callbackContext)
    {
        CastWeaponSkill(0);

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
        CastWeaponSkill(1);
        
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

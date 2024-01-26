using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerInteract : MonoBehaviour
{
    

    [Header("获取手部控制脚本，对比使用时间判断上次使用的手是哪只")]
    public HandIkMove leftHandIk;

    public HandIkMove rightHandIk;

    public SceneItemFindTrigger leftHandTrigger;
    public SceneItemFindTrigger rightHandTrigger;

    // [Header("左右手道具放置位置")] public Transform leftHandLighterPos;
    //
    // public Transform rightHandLighterPos;
    
    private UnityGameFramework.Runtime.Entity leftHandHoldingEntity;
    private UnityGameFramework.Runtime.Entity rightHandHoldingEntity;
    
    //拾取放下和使用道具的操作
    private PlayerInputActions playerInputActions;

    private UnityGameFramework.Runtime.Entity playerEntity;


    private Player playerLogic;

    private LevelDisplayForm levelDisplayForm;

    //public bool usingItem;
    // Start is called before the first frame update
    void Start()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.F.performed += PickOrDrop;
        playerInputActions.Player.MouseRight.started += UseItem;
        playerInputActions.Player.MouseRight.canceled += CancelItem;
        playerInputActions.Enable();

        playerEntity = GetComponent<UnityGameFramework.Runtime.Entity>();
        
        leftHandTrigger.Init(this);
        rightHandTrigger.Init(this);

        if (playerEntity)
        {
            playerLogic = playerEntity.Logic as Player;
            if (playerLogic)
            {
            
                playerLogic.OnAttachItem += OnAttachItem;
                playerLogic.OnDetachItem += OnDetachItem;
            }
        }
        
        
        
        levelDisplayForm = GameEntry.UI.GetUIForm(200,"Display") as LevelDisplayForm;
    }

    void OnAttachItem(UnityGameFramework.Runtime.Entity entity,string handName)
    {
        var hand = handName.EndsWith("L") ? leftHandIk : rightHandIk;
        if (hand == leftHandIk)
            leftHandHoldingEntity = entity;
        else
        {
            rightHandHoldingEntity = entity;
        }
        levelDisplayForm?.SwitchUseAndDropTip(true);
    }
    
    void OnDetachItem(UnityGameFramework.Runtime.Entity entity,string handName)
    {
        var hand = handName.EndsWith("L") ? leftHandIk : rightHandIk;
        if(hand==leftHandIk)
            leftHandHoldingEntity = null;
        else
        {
            rightHandHoldingEntity = null;
        }
        levelDisplayForm?.SwitchUseAndDropTip(false);

    }
    
    public HandIkMove GetCurrentHand()
    {
        return leftHandIk.LastDownTime > rightHandIk.LastDownTime ? leftHandIk : rightHandIk;
    }

    public SceneItemFindTrigger GetCurrentTrigger()
    {
        return GetCurrentHand() == leftHandIk ? leftHandTrigger : rightHandTrigger;
    }

    public UnityGameFramework.Runtime.Entity GetCurrentHolding()
    {
        return GetCurrentHand() == leftHandIk ? leftHandHoldingEntity : rightHandHoldingEntity;
    }

   
    void PickOrDrop(InputAction.CallbackContext callbackContext)
    {
        //Debug.LogError("按下F");
        var hand = GetCurrentHand();
        var trigger = leftHandTrigger;
        var targetHoldingEntity = leftHandHoldingEntity;
        if (hand == leftHandIk)
        {
            trigger = leftHandTrigger;
            targetHoldingEntity = leftHandHoldingEntity;
        }
        else
        {
            trigger = rightHandTrigger;
            targetHoldingEntity = rightHandHoldingEntity;
        }
        
        if (targetHoldingEntity != null)//当前手有东西，丢掉当前手的东西
        {
            GameEntry.Entity.DetachEntity(targetHoldingEntity);
            // if(hand==leftHandIk)
            //     leftHandHoldingEntity = null;
            // else
            // {
            //     rightHandHoldingEntity = null;
            // }
            // levelDisplayForm?.SwitchUseAndDropTip(false);
           
            return;
                
        }
        
        if (trigger.ShowingItem != null)//拾取东西后东西要改变layer，改变layer后trigger就检测不到了，所以不会重复获取
        {
           
            
            var targetPath = "";
            var logic = trigger.ShowingItem.Entity.Logic as SceneItem;
            var data = logic.sceneItemData;
            var table = GameEntry.DataTable.GetDataTable<DRSceneItem>();
            DRSceneItem info = table.GetDataRow(data.TypeId);
            var leftPath = info.LeftHandPath;
            var rightPath = info.RightHandPath;

            
            
            GameEntry.Entity.AttachEntity(trigger.ShowingItem,playerEntity.Id,hand==leftHandIk?leftPath:rightPath);

           
           
          
            
        }
    }

    public SceneItem UsingItem
    {
        get;
        private set;
    }
    void UseItem(InputAction.CallbackContext callbackContext)
    {
        var hand = GetCurrentHand();
        
        var targetHoldingEntity = leftHandHoldingEntity;
        if (hand == leftHandIk)
        {
           
            targetHoldingEntity = leftHandHoldingEntity;
        }
        else
        {
            targetHoldingEntity = rightHandHoldingEntity;
        }

        if (targetHoldingEntity != null)
        {
            var sceneItem = (SceneItem)targetHoldingEntity.Logic;
            sceneItem.StartUse();
            UsingItem = sceneItem;
        }
    }
    
    void CancelItem(InputAction.CallbackContext callbackContext)
    {
        if(UsingItem)
            UsingItem.StopUse();
        UsingItem = null;
    }


    private void OnDestroy()
    {
        
        playerInputActions.Player.F.performed -= PickOrDrop;
        playerInputActions.Player.MouseRight.started -= UseItem;
        playerInputActions.Player.MouseRight.canceled -= CancelItem;
        playerInputActions.Disable();

        if (playerLogic)
        {
            //场景中单位没有这个，，所以要加判断
            playerLogic = playerEntity.Logic as Player;
            playerLogic.OnAttachItem += OnAttachItem;
            playerLogic.OnDetachItem += OnDetachItem;
        }
    }
    
    
}

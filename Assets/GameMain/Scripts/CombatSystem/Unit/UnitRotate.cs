using System.Collections;
using System.Collections.Generic;
using UnityEngine;


///<summary>
///单位旋转控件，如果一个单位要通过游戏逻辑来进行旋转，就应该用它，不论是角色还是aoe还是bullet什么的
///</summary>
public class UnitRotate : MonoBehaviour
{
    ///<summary>
    ///单位当前是否可以旋转角度
    ///</summary>
    private bool canRotate = true;

    ///<summary>
    ///旋转的速度，1秒只能转这么多度（角度）
    ///每帧转动的角度上限是这个数字 * Time.fixedDeltaTime得来的。
    ///</summary>
    ///[Tooltip("旋转的速度，1秒只能转这么多度（角度），每帧转动的角度上限是这个数字*Time.fixedDeltaTime得来的。")]
    public float rotateSpeed=5f;

    private Quaternion targetRotation;  //目标转到多少度，因为旋转发生在围绕y轴旋转，所以只有y就足够了

    void FixedUpdate() {
        if (this.canRotate == false || DoneRotate() == true) return;
        
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.fixedDeltaTime);
    }

    //判断是否完成了旋转
    private bool DoneRotate(){
       
        return Quaternion.Angle(transform.rotation,targetRotation) < 0.01f; //允许一定的误差也当是达成了。
    }

    ///<summary>
    ///旋转到指定角度
    ///<param name="degree">需要旋转到的角度</param>
    ///</summary>
    public void RotateTo(Quaternion rotation){
        targetRotation = rotation;
    }

    

    ///<summary>
    ///禁止单位可以旋转的能力，这会终止当前正在进行的旋转
    ///终止当前的旋转看起来是一个side-effect，但是依照游戏规则设计来说，他只是“配套功能”所以严格的说并不是side-effect
    ///</summary>
    public void DisableRotate(){
        canRotate = false;
        targetRotation = transform.rotation;
    }

    ///<summary>
    ///开启单位可以旋转的能力
    ///</summary>
    public void EnableRotate(){
        canRotate = true;
    }
}

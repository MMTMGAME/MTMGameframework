using UnityEngine;

/// <summary>
/// 清空累计Trigger
/// </summary>
public class FSMClearSignals : StateMachineBehaviour
{
    public string[] ClearAtEnter;
    public string[] ClearAtExit;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //进入时清空trigger
        foreach (var item in ClearAtEnter)
        {
            animator.ResetTrigger(item);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //退出时清空trigger
        foreach (var item in ClearAtExit)
        {
            animator.ResetTrigger(item);
        }
    }
}
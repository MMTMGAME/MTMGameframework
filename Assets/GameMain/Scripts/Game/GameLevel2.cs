using GameFramework.Event;
using GameMain;
using UnityGameFramework.Runtime;

public class GameLevel2 : GameBase
{

   
   
    public Firecracker Firecracker { get; private set; }


    protected override void CheckGameOverOrWin()
    {
        base.CheckGameOverOrWin();
        if (Firecracker != null && !Firecracker.Available && Player.IsDead == false)
        {
            GameWin = true;
        }
    }


    protected override void OnShowEntitySuccess(object sender, GameEventArgs e)
    {
        base.OnShowEntitySuccess(sender,e);
        ShowEntitySuccessEventArgs ne = (ShowEntitySuccessEventArgs)e;
        if (ne.EntityLogicType == typeof(Firecracker))
        {

            Firecracker = ne.Entity.Logic as Firecracker;

        }
    }
}

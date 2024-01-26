using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class ProcedureLevel7 : ProcedureLevel
{
   

    public override GameMode GameLevel
    {
        get
        {
            return GameMode.Level7;
        }
    }

    protected override void OnGameEnd()
    {
        var currentGame = GetGameBase();
        if (currentGame.GameWin)
        {
            GameEntry.UI.OpenDialog(new DialogParams()
            {
                Mode = 1,
                Title = GameEntry.Localization.GetString("GameClear.Title"),
                Message = GameEntry.Localization.GetString("GameClear.Message"),
                OnClickConfirm = delegate(object userData) {GotoMenu(0.5f); },
                ConfirmText = GameEntry.Localization.GetString("GameClear.MainMenu"),
                
            });
        }

        if (currentGame.GameOver)
        {
            GameEntry.UI.OpenDialog(new DialogParams()
            {
                Mode = 2,
                Title = GameEntry.Localization.GetString("GameOver.Title"),
                Message = GameEntry.Localization.GetString("GameOver.Message"),
                OnClickConfirm = delegate(object userData) { Retry(); },
                ConfirmText = GameEntry.Localization.GetString("GameOver.Retry"),
                OnClickCancel = delegate(object userData) { GotoMenu(0.5f); },
                CancelText = GameEntry.Localization.GetString("GameOver.MainMenu"),
            });
        }
        
    }
}

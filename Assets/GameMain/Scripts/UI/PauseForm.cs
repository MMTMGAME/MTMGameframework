using System.Collections;
using GameFramework.Localization;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;
namespace GameMain
{
    public class PauseForm : UGuiForm
    {
        public void Continue()
        {
            GameEntry.Base.ResetNormalGameSpeed();

            procedureOwner?.GetGameBase().SwitchSpeedAdjust(true);
            GameEntry.Sound.PlayUISound(20006);
            Close();
        }

        private ProcedureLevel procedureOwner;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            procedureOwner = GameEntry.Procedure.CurrentProcedure as ProcedureLevel;
            
            GameEntry.Sound.PlayUISound(20005);
            procedureOwner?.GetGameBase().SwitchSpeedAdjust(false);
            Invoke(nameof(SetGameSpeedTo0),0.22f);
        }

        void SetGameSpeedTo0()
        {
            GameEntry.Base.GameSpeed = 0f;
        }


        

        public void GoToMainMenu()
        {
            var procedureCurrent = GameEntry.Procedure.CurrentProcedure;
            if (procedureCurrent is ProcedureLevel procedureMain)
            {
                Close(true);
                Invoke(nameof(ChangeScene), 0.1f);
            }
        }

        void ChangeScene()
        {
            var procedureCurrent = GameEntry.Procedure.CurrentProcedure;
            if (procedureCurrent is ProcedureLevel procedureMain)
            {
                procedureMain.GotoMenu(0);
            }
        }

        public void QuitGame()
        {
            //Time.timeScale = 1;
            StartCoroutine(QuitGameCoroutine());
            // Time.timeScale = 1;
            // GameEntry.UI.OpenDialog(new DialogParams()
            // {
            //     Mode = 2,
            //     Title = GameEntry.Localization.GetString("AskQuitGame.Title"),
            //     Message = GameEntry.Localization.GetString("AskQuitGame.Message"),
            //     OnClickConfirm = delegate(object userData)
            //     {
            //         //UnityGameFramework.Runtime.GameEntry.Shutdown(ShutdownType.Quit);
            //
            //         
            //         Application.Quit();
            //     },
            // });

            //ShowCanvasGroupImmidiately();

        }

        IEnumerator QuitGameCoroutine()
        {
            
            GameEntry.UI.OpenDialog(new DialogParams()
            {
                Mode = 2,
                Title = GameEntry.Localization.GetString("AskQuitGame.Title"),
                Message = GameEntry.Localization.GetString("AskQuitGame.Message"),
                OnClickConfirm = delegate(object userData)
                {
                    //UnityGameFramework.Runtime.GameEntry.Shutdown(ShutdownType.Quit);
                    
                    Application.Quit();
                },
            });
            yield return null;
            
        }


        public void Retry()
        {
            var procedure = GameEntry.Procedure.CurrentProcedure as ProcedureLevel;
            if (procedure != null)
            {
                
                
                procedure.Retry();
                Close();
            }
        }
    
       
    }
}
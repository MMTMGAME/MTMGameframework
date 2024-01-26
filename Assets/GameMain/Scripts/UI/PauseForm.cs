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
            Close();
        }

        private ProcedureBase procedureOwner;

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
            GameEntry.UI.OpenDialog(new DialogParams()
            {
                Mode = 2,
                Title = GameEntry.Localization.GetString("AskQuitGame.Title"),
                Message = GameEntry.Localization.GetString("AskQuitGame.Message"),
                OnClickConfirm = delegate(object userData)
                {
                    UnityGameFramework.Runtime.GameEntry.Shutdown(ShutdownType.Quit);
                },
            });
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
    
        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            GameEntry.Base.ResetNormalGameSpeed();
            
        }
    }
}
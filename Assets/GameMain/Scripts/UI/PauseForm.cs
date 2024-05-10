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


        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            #if PLATFORM_STANDALONE
                GameEntry.Base.GameSpeed = 0f;
            #endif
        }

        public void GoToMainMenu()
        {
            GameEntry.Base.ResetNormalGameSpeed();
            var procedureCurrent = GameEntry.Procedure.CurrentProcedure;
            if (procedureCurrent is ProcedureLevel procedureMain)
            {
                Close(true);
                Invoke(nameof(ChangeScene), 0.001f);
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
            #if PLATFORM_STANDALONE
                GameEntry.Base.ResetNormalGameSpeed();
            #endif
        }
    }
}
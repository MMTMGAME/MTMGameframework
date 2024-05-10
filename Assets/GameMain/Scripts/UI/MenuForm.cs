﻿//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using UnityEngine;
using UnityGameFramework.Runtime;

namespace GameMain
{
    public class MenuForm : UGuiForm
    {
        [SerializeField]
        private GameObject m_QuitButton = null;

        private ProcedureMenu m_ProcedureMenu = null;

        public void OnStartButtonClick()
        {
            m_ProcedureMenu.ShowLevelSelectForm();
        }

        public void OnSettingButtonClick()
        {
            var landscape = Screen.width > Screen.height;//横竖屏判断
            GameEntry.UI.OpenUIForm(landscape?UIFormId.SettingForm: UIFormId.SettingFormPortrait);
        }

        public void OnAboutButtonClick()
        {
            GameEntry.UI.OpenUIForm(UIFormId.AboutForm);
        }

        public void OnLeaderButtonClick()
        {
            GameEntry.UI.OpenUIForm(UIFormId.LeaderboardForm);
        }

        public void OnQuitButtonClick()
        {
            GameEntry.UI.OpenDialog(new DialogParams()
            {
                Mode = 2,
                Title = GameEntry.Localization.GetString("AskQuitGame.Title"),
                Message = GameEntry.Localization.GetString("AskQuitGame.Message"),
                OnClickConfirm = delegate (object userData) { UnityGameFramework.Runtime.GameEntry.Shutdown(ShutdownType.Quit); },
            });
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnOpen(object userData)
#else
        protected internal override void OnOpen(object userData)
#endif
        {
            base.OnOpen(userData);

            m_ProcedureMenu = (ProcedureMenu)userData;
            if (m_ProcedureMenu == null)
            {
                Log.Warning("ProcedureMenu is invalid when open MenuForm.");
                return;
            }

            m_QuitButton.SetActive(Application.platform != RuntimePlatform.IPhonePlayer);
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnClose(bool isShutdown, object userData)
#else
        protected internal override void OnClose(bool isShutdown, object userData)
#endif
        {
            m_ProcedureMenu = null;

            base.OnClose(isShutdown, userData);
        }
    }
}

namespace Maze.Views.Common
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class SettingsView : BaseView
    {
        [SerializeField] Button _backButton;

        public event EventHandler BackButtonClicked;

        protected override void Awake()
        {
            base.Awake();

            _backButton.onClick.AddListener(OnBackButtonClicked);
        }

        private void OnBackButtonClicked()
        {
            if (BackButtonClicked != null)
                BackButtonClicked(this, EventArgs.Empty);
        }
    }
}
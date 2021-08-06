
namespace Maze.Views.Common
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class MasksView : BaseView
    {
        [SerializeField]
        private Button _backButton;

        public event EventHandler BackButtonClickEvent;

        protected override void Awake()
        {
            _backButton.onClick.AddListener(OnBackButtonClicked);
        }

        private void InvokeBackButtonClickedEvent()
        {
            var handler = BackButtonClickEvent;

            if (handler != null)
                handler.Invoke(this, EventArgs.Empty);
        }

        private void OnBackButtonClicked()
        {
            InvokeBackButtonClickedEvent();
        }
    }
}

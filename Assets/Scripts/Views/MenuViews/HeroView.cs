
namespace Maze.Views.Common
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class HeroView : BaseView
    {
        public event EventHandler AbilitiesButtonClickEvent;
        public event EventHandler BackButtonClickEvent;
        public event EventHandler MasksButtonClickEvent;
        public event EventHandler PlayButtonClickEvent;

        [SerializeField]
        private Button _abilitiesButton;
        [SerializeField]
        private Button _backButton;
        [SerializeField]
        private Button _masksButton;
        [SerializeField]
        private Button _playButton;

        protected override void Awake()
        {
            _abilitiesButton.onClick.AddListener(OnAbilitiesButtonClicked);
            _backButton.onClick.AddListener(OnBackButtonClicked);
            _masksButton.onClick.AddListener(OnMasksButtonClicked);
            _playButton.onClick.AddListener(OnPlayButtonClicked);
        }

        private void OnAbilitiesButtonClicked()
        {
            InvokeAbilitiesButtonClickEvent();
        }

        private void OnBackButtonClicked()
        {
            InvokeBackButtonClickEvent();
        }

        private void OnMasksButtonClicked()
        {
            InvokeMasksButtonClicked();
        }

        private void OnPlayButtonClicked()
        {
            InvokePlayButtonClickEvent();
        }

        private void InvokeAbilitiesButtonClickEvent()
        {
            var handler = AbilitiesButtonClickEvent;

            if (handler != null)
                handler.Invoke(this, EventArgs.Empty);
        }

        private void InvokeBackButtonClickEvent()
        {
            var handler = BackButtonClickEvent;

            if (handler != null)
                handler.Invoke(this, EventArgs.Empty);
        }

        private void InvokeMasksButtonClicked()
        {
            var handler = MasksButtonClickEvent;

            if (handler != null)
                handler.Invoke(this, EventArgs.Empty);
        }

        private void InvokePlayButtonClickEvent()
        {
            var handler = PlayButtonClickEvent;

            if (handler != null)
                handler.Invoke(this, EventArgs.Empty);
        }

    }
}

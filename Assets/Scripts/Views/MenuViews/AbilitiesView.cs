
namespace Maze.Views.Common
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class AbilitiesView : BaseView
    {
        public event EventHandler<AbilitiesEventArgs> ItemClickedEvent;
        public event EventHandler CloseEvent;

        [SerializeField]
        private Button _firstButton;
        [SerializeField]
        private Button _secondButton;
        [SerializeField]
        private Button _thirdButton;

        [Space(10)]
        [SerializeField]
        private Button _backButton;

        protected override void Awake()
        {
            _firstButton.onClick.AddListener(OnFirstButtonClick);
            _secondButton.onClick.AddListener(OnSecondButtonClick);
            _thirdButton.onClick.AddListener(OnThirdButtonClick);

            _backButton.onClick.AddListener(OnBackButtonClick);
        }

        private void OnFirstButtonClick()
        {
            InvokeItemClickedEvent("first");
        }

        private void OnSecondButtonClick()
        {
            InvokeItemClickedEvent("second");
        }

        private void OnThirdButtonClick()
        {
            InvokeItemClickedEvent("third");
        }

        private void OnBackButtonClick()
        {
            InvokeCloseEvent();
        }

        private void InvokeCloseEvent()
        {
            var handler = CloseEvent;
            if (handler != null)
                handler.Invoke(this, EventArgs.Empty);
        }

        private void InvokeItemClickedEvent(string abilityName)
        {
            var handler = ItemClickedEvent;
            if (handler != null)
                handler.Invoke(this, new AbilitiesEventArgs(abilityName));
        }

        public class AbilitiesEventArgs : EventArgs
        {
            public string AbilityName { get; private set; }

            public AbilitiesEventArgs(string abilityName)
            {
                AbilityName = abilityName;
            }
        }
    }
}
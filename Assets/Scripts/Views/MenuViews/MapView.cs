namespace Maze.Views.Common {
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class MapView : BaseView {
        [SerializeField] private Button[] _buttons;

        public event EventHandler LevelButtonClickEvent;

        protected override void Awake() {
            for (int i = 0; i < _buttons.Length; i++) {
                _buttons[i].onClick.AddListener(OnButtonClicked);
            }
        }

        private void OnButtonClicked() {
            InvokeButtonClickedEvent();
        }

        private void InvokeButtonClickedEvent() {
            var handler = LevelButtonClickEvent;

            if (handler != null)
                handler.Invoke(this, EventArgs.Empty);
        }
    }
}
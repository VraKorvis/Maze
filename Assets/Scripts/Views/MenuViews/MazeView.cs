using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Maze.Views
{
    public class MazeView : BaseView
    {
        public event EventHandler BackButtonClickEvent;

        [SerializeField]
        private Button _backButton;

        protected override void Awake()
        {
            _backButton.onClick.AddListener(OnBackButtonClicked);
        }

        private void InvokeBackButtonClickEvent()
        {
            var handler = BackButtonClickEvent;

            if (handler != null)
                handler.Invoke(this, EventArgs.Empty);
        }

        private void OnBackButtonClicked()
        {
            InvokeBackButtonClickEvent();
        }
    }
}
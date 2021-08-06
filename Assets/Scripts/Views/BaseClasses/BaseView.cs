
namespace Maze.Views
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public interface IView
    {
        Transform transform { get; }
        GameObject gameObject { get; }

        void Show();
        void Hide();
    }

    public class BaseView : UIBehaviour, IView
    {
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}
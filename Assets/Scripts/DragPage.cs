using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{
    public class DragPage : MonoBehaviour
    {

        protected bool _isPressed = false;
        protected Vector2 cachePos;
        protected int indexTouch = 0;
        protected UIPanel panel;
        private void Awake()
        {
            panel = gameObject.GetComponentInParents<UIPanel>();
        }
        void OnDrag(Vector2 d)
        {
            
            if (_isPressed )
            {
                var delta = (UICamera.currentTouch.pos - cachePos);
                if (delta != Vector2.zero) {
                    MainScene.Instance.DragMapLayer(delta * (1080.0f/(float)panel.GetViewSize().x), indexTouch);
                    indexTouch++;
                }
            }
        }
        void OnPress(bool isPressed) {

            _isPressed = isPressed;
            cachePos = UICamera.currentTouch.pos;
            indexTouch = 0;
            Debug.Log("Press" + isPressed);
            if (!isPressed)
            {
                MainScene.Instance.DragMapLayer(Vector2.zero, 0,true);
            }
            else
            {
                //MainScene.Instance.DragMapLayer(Vector2.zero, 0, false);
                //indexTouch++;
            }
        }
    }
}

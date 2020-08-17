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
        void OnDrag(Vector2 d)
        {
            if (_isPressed )
            {
                var delta = (UICamera.currentTouch.pos - cachePos);
                if (delta != Vector2.zero) {
                    MainScene.Instance.DragMapLayer(delta, indexTouch);
                    indexTouch++;
                }
            }
        }
        void OnPress(bool isPressed) {

            _isPressed = isPressed;
            cachePos = UICamera.currentTouch.pos;
            indexTouch = 0;
            if (!isPressed)
            {
                MainScene.Instance.DragMapLayer(Vector2.zero, 0,true);
            }
        }
    }
}

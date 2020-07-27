using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Pok
{
    public class AlphaController : MonoBehaviour
    {
        protected UIWidget _widget;
        protected UIPanel _panel;
        private void Awake()
        {
            _widget = GetComponent<UIWidget>();
            _panel = GetComponent<UIPanel>();
        }
     
        protected float _currentAlpha = 1;
        public float Alpha
        {
            set
            {
                _currentAlpha = value;
                if (_widget)
                {
                    _widget.alpha = value;
                }
                if (_panel)
                {
                    _panel.alpha = value;
                }
            }
            get
            {
                return _currentAlpha;
            }
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

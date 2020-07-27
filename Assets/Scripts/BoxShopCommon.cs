using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;

namespace Pok
{
    public class BoxShopCommon : Singleton<BoxShopCommon>
    {
        public UIElement container;
        public UITable table;
        public IShop[] elements;

        protected Coroutine checkStateCoroutine;
        protected override void Awake()
        {
            base.Awake();
            elements = GetComponentsInChildren<IShop>(true);
        }
        private void OnEnable()
        {
            checkStateCoroutine = StartCoroutine(checkState());
        }

        protected virtual void OnDisable()
        {
            if (checkStateCoroutine != null)
            {
                StopCoroutine(checkStateCoroutine);
            }
        }
        public bool isSubBoxDone
        {
            get
            {
                foreach(var element in elements)
                {
                    if (!element.isInitDone())
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        IEnumerator checkState()
        {
            while (!isSubBoxDone)
            {
                yield return new WaitForEndOfFrame();

            }
            table.Reposition();
        }

        public void show()
        {
            container.show();
        }

    }
}
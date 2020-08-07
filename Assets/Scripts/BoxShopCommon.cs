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
        protected bool reposition = false;
        protected override void Awake()
        {
            base.Awake();
            elements = GetComponentsInChildren<IShop>(true);
        }
        private void OnEnable()
        {
            reposition = false;
            checkStateCoroutine = StartCoroutine(checkState());
        }

        protected virtual void OnDisable()
        {
            if (checkStateCoroutine != null)
            {
                StopCoroutine(checkStateCoroutine);
            }
            planGoShop = "";
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
            reposition = true;
            if (!string.IsNullOrEmpty(planGoShop))
            {
                if (planGoShop == "ShopGold")
                {
                    showBoxGold();
                }
                else
                {
                    showBoxCrystal();
                }
            }
        }

        public void show()
        {
            container.show();
        }
        protected string planGoShop = "";
        public void showBoxGold() {
            show();
            if (reposition)
            {
                var element = System.Array.Find(elements, x => x.shopID() == "ShopGold");
                GetComponentInChildren<UICenterOnChild>(true).CenterOn(element.getContainer().transform);
            }
            else
            {
                planGoShop = "ShopGold";
            }
        }
        public void showBoxCrystal()
        {
            show();
            if (reposition)
            {
                var element = System.Array.Find(elements, x => x.shopID() == "ShopCrystal");
                GetComponentInChildren<UICenterOnChild>(true).CenterOn(element.getContainer().transform);
            }
            else
            {
                planGoShop = "ShopCrystal";
            }
        }
    }
}
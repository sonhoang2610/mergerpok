using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{
    public class BoxBank : MonoBehaviour
    {
        public UILabel moneyBonus;
        public UIButton btnButtonGetX2;
        public UIElement container;
        protected Color cacheColorTextButtonX2;

        protected string cacheMoney = "0";
        private void Awake()
        {
            cacheColorTextButtonX2 = btnButtonGetX2.GetComponentInChildren<UILabel>(true).color;
        }
        public void setActiveButtonX2(bool active)
        {
            btnButtonGetX2.isEnabled = active;
            btnButtonGetX2.GetComponentInChildren<UILabel>(true).color = active ? cacheColorTextButtonX2 : Color.white;
        }
 
        public void get()
        {
            GetComponent<UIElement>().close();
            claim(1);
        }
        public void claim(int factor)
        {
            var coin = GameManager.Instance.Database.getItem("Coin");
            coin.addQuantity((cacheMoney.toBigInt()*factor).ToString());
        }

        public void getX2()
        {
            btnButtonGetX2.isEnabled = false;
            GetComponent<UIElement>().close();
            GameManager.Instance.WatchRewardADS("Bank", (o) =>
            {
                if (o)
                {
                    claim(2);
                }
                else
                {
                    claim(1);
                }
            });
        }
        public void show(string quantity)
        {
            container.show();
            moneyBonus.text = quantity.ToKMBTA();
            cacheMoney = quantity;
        }

        private void OnEnable()
        {
            if (!GameManager.Instance.isRewardADSReady("Bank"))
            {
                setActiveButtonX2(false);
                loadADS();
            }
            else
            {
                setActiveButtonX2(true);
            }

        }
        private void OnDisable()
        {
            StopAllCoroutines();
        }
        public void loadADS()
        {
            GameManager.Instance.LoadRewardADS("Bank", (o)=>{
                if (!gameObject.activeSelf) return;
                if (o)
                {
                    setActiveButtonX2(true);
                }
                else
                {
                    loadADS();
                }
            });
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
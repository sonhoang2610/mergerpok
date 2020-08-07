using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;


namespace Pok
{
    public class HUDManager : Singleton<HUDManager>
    {
        public GameObject inGameHud;
        public UILabel quanityHour,timeXInCome,timeDisCountCreature,timeEggReduce,factorGoldToBuy;
        public GameObject fullSlotIcon;
        public UIElement boxNotEnoughCrystal,boxMagicCaseContain;
        public BoxVip boxVip;
        public BoxShopCommon boxShop;
        public BoxUpgrade boxUpgrade;
        public BoxWheelFortune boxWheel;
        public BoxRewardADS boxRewardADS;
        public UIElement boxSetting;
        public UIElement boxFullSlot;
        public UIElement boxMagicCase;
        public BoxReward boxReward;
        public BoxBank boxBank;
        public void showBoxRewardADS()
        {

        }
        public void showBoxFullSlot()
        {
            boxFullSlot.show();
        }
        public void showBoxSetting()
        {
            boxSetting.show();
        }

        public void showBoxMagicCaseContain()
        {
            boxMagicCaseContain.show();
        }
        public void showBoxVip()
        {
            boxVip.show();
        }
        public void showBoxWheel()
        {
            boxWheel.show();
        }
        protected override void Awake()
        {
            base.Awake();
            StartCoroutine(init());
        }
        public void showBoxNotEnough(BaseItemGame item)
        {
            if(item.ItemID == "Crystal")
            {
                boxNotEnoughCrystal.show();
            }
            else if(item.ItemID == "Coin")
            {
                BoxShopCommon.Instance.showBoxGold();
            }
        }
        public void showBoxUpgrade()
        {
            boxUpgrade.show();
        }
        public IEnumerator init()
        {
            yield return new WaitForEndOfFrame();
            var  zone = ChooseZoneLayer.Instance.container.GetComponent<UIElement>();
            zone.onEnableEvent.AddListener(() => {
                inGameHud.gameObject.SetActive(false);
            });
            zone.onDisableEvent.AddListener(() => {
                inGameHud.gameObject.SetActive(true);
            });
        }
        public void showBoxCollectionCreature()
        {
            BoxCreatureCollection.Instance.show();
        }
        public void showChooseZoneLayer()
        {
            ChooseZoneLayer.Instance.show();
        }

        public void showBoxShopCreature()
        {
            BoxShopCreature.Instance.show();
        }

        public void showBoxShopGold()
        {
            BoxShopCommon.Instance.showBoxGold();
        }
        public void showBoxShopCrystal()
        {
            BoxShopCommon.Instance.showBoxCrystal();
        }
        public void showBoxShopBooster()
        {
            BoxShopCommon.Instance.show();
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

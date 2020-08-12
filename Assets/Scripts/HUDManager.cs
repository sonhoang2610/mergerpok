using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;
using EasyMobile;
using UnityEngine.Purchasing;

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
        public BoxBonusEvolutionPok boxEvoPok;
        public GameObject btnVip;

        protected Dictionary<GameObject,List<string>> markActiveObject = new Dictionary<GameObject, List<string>>();
        public void factorGoldToBuyActive(string id, bool active)
        {
            ActiveObject(factorGoldToBuy.transform.parent.parent.gameObject, id, active);
        }

        public void ActiveObject(GameObject pObject,string id,bool active)
        {
            if (!markActiveObject.ContainsKey(pObject))
            {
                markActiveObject.Add(pObject, new List<string>());
            }
            if (active)
            {
                markActiveObject[pObject].Remove(id);
                pObject.gameObject.SetActive(markActiveObject[pObject].Count == 0);
            }
            else
            {
                if (!markActiveObject[pObject].Contains(id))
                {
                    markActiveObject[pObject].Add(id);
                }
                pObject.gameObject.SetActive(false);
            }
        }
        
        
        public void showBoxEvo(CreatureItem from,CreatureItem to,CreatureInstanceSaved id)
        {
            var time = TimeCounter.Instance.timeCollection.Value.Find(x => x.id.Contains("EvolutionPok"));
            if (time != null)
            {
                return;
            }
            TimeCounter.Instance.addTimer(new TimeCounterInfo() { id = $"[Block]EvolutionPok", autoRemoveIfToDestiny = true, destinyIfHave = GameManager.Instance.TimeDelayBonusEvolution });
            boxEvoPok.showBoxEvo(from, to,id);
        }
        public void showBoxRewardADS()
        {
            if (GameManager.Instance.isRewardADSReady("BoxRewardADS"))
            {
                GameManager.Instance.WatchRewardADS("BoxRewardADS", (o) =>
                {
                    if (o)
                    {
                        boxRewardADS.GetComponent<UIElement>().show();
                    }
                });
            }
            else
            {
                GameManager.Instance.LoadRewardADS("BoxRewardADS");
            }
           
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
        private void OnEnable()
        {
            InAppPurchasing.PurchaseCompleted += PurchaseCompleted;
            StartCoroutine(checkState());
        }

        public IEnumerator checkState()
        {
            while (!GameManager.readyForThisState("Main"))
            {
                yield return new WaitForEndOfFrame();
            }
            while (!InAppPurchasing.IsInitialized())
            {
                yield return new WaitForEndOfFrame();
            }
            checkVip();
        }
        private void OnDisable()
        {
            InAppPurchasing.PurchaseCompleted -= PurchaseCompleted;
        }
        public void checkVip()
        {
            boxVip.item.loadAssetWrapped<BaseItemGame>((o) =>
            {
                var product = InAppPurchasing.GetIAPProductById(o.ItemID);
                SubscriptionInfo infoPro = InAppPurchasing.GetSubscriptionInfo(product.Name);
                if ((infoPro.isSubscribed() == Result.True || infoPro.isFreeTrial() == Result.True) && infoPro.isExpired() == Result.False)
                {
                    btnVip.GetComponent<UIButton>().isEnabled = true;
                }
                else
                {
                    btnVip.GetComponent<UIButton>().isEnabled = false;
                    var exist = GameManager.Instance.Database.getItem(o.ItemID);
                    exist.setQuantity("0");
                }
            });
        }
        public void PurchaseCompleted(IAPProduct pro)
        {
            checkVip();
        }
    }
}

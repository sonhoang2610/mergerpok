using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;
using EasyMobile;
using UnityEngine.Purchasing;
using ScriptableObjectArchitecture;

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
        public BoxMultiplyBonus boxMultiplyBonus;
        public BoxPackageInApp boxPackedInapp;
        public GameObject btnVip;
        public GameObject hand;
        public GameObject attachMentPackageMultiplyBonus;
        public GameObject btnEvlotion;
        public UIElement boxEvolution;

        public UI2DSprite processTimeMixing;
        public UnitDefineLevelIntVariable timeMixLimit;

        public void showBoxEvolution()
        {
            boxEvolution.show();
        }

        public void checkEvolutionPack()
        {
            btnEvlotion.gameObject.SetActive(false);
            if (GameManager.Instance.ZoneChoosed != "Zone1")
            {
               var zone = GameManager.Instance.Database.zoneInfos.Find(x => x.Id == GameManager.Instance.ZoneChoosed);
                if(!string.IsNullOrEmpty(zone.CurentUnlock) && int.Parse( zone.CurentUnlock.Remove(0,3)) < 6 && !ES3.Load<bool>("Evolution" + GameManager.Instance.ZoneChoosed,false))
                {
                    btnEvlotion.gameObject.SetActive(true);

                }
               
            }
        }

        public void checkExistMultiplyBonus()
        {
           var times = TimeCounter.Instance.timeCollection.Value.FindAll(x => x.id.Contains("[MultiplyBonus]"));
            foreach (var time in times)
            {
                if (time != null)
                {
                    var strs = time.id.Remove(0, ("[MultiplyBonus]").Length).Split('/');
                    var item = GameDatabase.Instance.getItemInventory(strs[0]);
                    var creature = GameDatabase.Instance.CreatureCollection.Find(x => x.ItemID == strs[1]);
            
                    if (item != null && strs.Length == 3 && GameManager.Instance.ZoneChoosed == strs[2])
                    {
                        var findObject = attachMentPackageMultiplyBonus.transform.Find("[MultiplyBonus]" + item.ItemID + "/" + creature.ItemID + "/" + GameManager.Instance.ZoneChoosed);
                        if (findObject)
                        {
                            continue;
                        }
                        item.getModelForState((o) =>
                        {

                            var timeObject = attachMentPackageMultiplyBonus.AddChild(o);
                            timeObject.GetComponent<TimeListener>().listenID = "[MultiplyBonus]" + item.ItemID + "/" + creature.ItemID + "/" + GameManager.Instance.ZoneChoosed;
                            timeObject.transform.SetSiblingIndex(0);
                            timeObject.GetComponent<TimeListener>().onTimeOut.AddListener(() =>
                            {
                                timeObject.gameObject.SetActive(false);
                                Destroy(timeObject);
                                attachMentPackageMultiplyBonus.GetComponent<UIGrid>().Reposition();
                            });
                            timeObject.GetComponent<UIButton>().onClick.Add(new EventDelegate(() =>
                            {
                                ShopItemInfo itemPackage = GameDatabase.Instance.packageMultiplyBonus1;
                                if (GameDatabase.Instance.packageMultiplyBonus2.itemSell == item)
                                {
                                    itemPackage = GameDatabase.Instance.packageMultiplyBonus2;
                                }

                                boxMultiplyBonus.showData(itemPackage, creature);
                            }));
                            attachMentPackageMultiplyBonus.GetComponent<UIGrid>().Reposition();
                        });
                    }
                }
            }
        }
        public void checkExistPackage()
        {
        
            var times = TimeCounter.Instance.timeCollection.Value.FindAll(x => x.id.Contains("[Package]") && x.id.Contains(GameManager.Instance.ZoneChoosed));
            foreach (var time in times)
            {
                if (time != null)
                {
                    var strs = time.id.Remove(0, ("[Package]").Length).Split('/');
                    var item = GameDatabase.Instance.getItemInventory(strs[0]);
               
                    if (item != null)
                    {
                        var findObject = attachMentPackageMultiplyBonus.transform.Find("[Package]" + item.ItemID + "/" + GameManager.Instance.ZoneChoosed);
                        if (findObject)
                        {
                            continue;
                        }
                        item.getModelForState((o) =>
                        {

                            var timeObject = attachMentPackageMultiplyBonus.AddChild(o);
                            timeObject.name = "[Package]" + item.ItemID + "/" + GameManager.Instance.ZoneChoosed;
                            timeObject.GetComponent<TimeListener>().listenID = "[Package]" + item.ItemID+"/"+ GameManager.Instance.ZoneChoosed;
                            timeObject.transform.SetSiblingIndex(0);
                            timeObject.GetComponent<TimeListener>().onTimeOut.AddListener(() =>
                            {
                                timeObject.gameObject.SetActive(false);
                                Destroy(timeObject);
                                attachMentPackageMultiplyBonus.GetComponent<UIGrid>().Reposition();
                            });
                            timeObject.GetComponent<UIButton>().onClick.Add(new EventDelegate(() =>
                            {
                                ShopItemInfo itemPackage = GameDatabase.Instance.startedKit;

                                boxPackedInapp.showData(itemPackage);
                            }));
                            attachMentPackageMultiplyBonus.GetComponent<UIGrid>().Reposition();
                        });
                    }
                }
            }
        }
        public void updateTimeMixing()
        {
            processTimeMixing.fillAmount = (float)GameManager.Instance.MixingTime / (float)timeMixLimit.Value.getCurrentUnit();
        }

        

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
                var activeBool = markActiveObject[pObject].Count == 0;
                if (activeBool != pObject.gameObject.activeSelf)
                {
                    pObject.gameObject.SetActive(markActiveObject[pObject].Count == 0);
                }
            }
            else
            {
                if (!markActiveObject[pObject].Contains(id))
                {
                    markActiveObject[pObject].Add(id);
                }
                if (pObject.gameObject.activeSelf)
                {
                    pObject.gameObject.SetActive(false);
                }
            }
        }
        
        public void hack1()
        {
            GameManager.Instance.hack();
        }
        public void hack2()
        {
            GameManager.Instance.hack1();
        }
        public void showBoxEvo(CreatureItem from,CreatureItem to,CreatureInstanceSaved id)
        {
            var time = TimeCounter.Instance.timeCollection.Value.Find(x => x.id.Contains("EvolutionPok"));
            if (time != null)
            {
                return;
            }
            TimeCounter.Instance.addTimer(new TimeCounterInfo() { id = $"[Block]EvolutionPok", autoRemoveIfToDestiny = true, destinyIfHave = GameManager.Instance.TimeDelayBonusEvolution, resetOnStart = true });
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

        public void SaveGame()
        {
            GameManager.Instance.SaveGame();
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
            updateTimeMixing();
            while (!InAppPurchasing.IsInitialized())
            {
                yield return new WaitForEndOfFrame();
            }
            checkEvolutionPack();
            checkExistMultiplyBonus();
            checkExistPackage();
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
                var product = InAppPurchasing.GetIAPProductById(o.ItemID.ToLower());
                SubscriptionInfo infoPro = InAppPurchasing.GetSubscriptionInfo(product.Name);
                if (infoPro != null)
                {
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
                }
            });
        }
        public void PurchaseCompleted(IAPProduct pro)
        {
            checkVip();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;
using ScriptableObjectArchitecture;
using DG.Tweening;

namespace Pok
{
    public static class PokUltis
    {
        public static string calculateCreaturePrice(int indexWay,int numberBought,CreatureItem creauture)
        {
            if (indexWay == 0)
            {
                System.Numerics.BigInteger startMoney = 560;
              
                if (GameManager.Instance.ZoneChoosed != "Zone1")
                {
                    startMoney = creauture.goldAFKReward[GameManager.Instance.ZoneChoosed].toBigInt();
                    System.Numerics.BigInteger newMoney = startMoney;
                    if (numberBought > 0)
                    {
                        newMoney += ( Mathf.Clamp(numberBought,0,2)) * 20 * startMoney / 100;
                    }
                    if (numberBought > 2)
                    {
                        newMoney += (numberBought - 2) * 20 * startMoney / 100;
                    }
                    return newMoney.ToString();
                }
                else
                {
                    startMoney = (startMoney * (long)System.Math.Pow(3, creauture.RankChild));
                    startMoney += (int)Mathf.Clamp(numberBought, 0, 5) * startMoney;
                    if(numberBought > 5)
                    {
                        startMoney += (numberBought - 5) *20* startMoney/100;
                    }
                    return startMoney.toString();
                }
            }
            else
            {
                int startMoney = 1 + 3 * (int.Parse(GameManager.Instance.ZoneChoosed.Substring(GameManager.Instance.ZoneChoosed.Length - 1, 1)) - 1);
                startMoney += (creauture.RankChild / 2);
                startMoney += numberBought / 10;
                return startMoney.ToString();
            }
        }
    }
    public interface IMachineItem
    {
        void onEnable(BaseItemGameInstanced itemSlot);
        void onAdded(BaseItemGameInstanced itemSlot);
        void onRemoved(BaseItemGameInstanced itemSlot);
        void onDirtyChange(BaseItemGameInstanced itemSlot);
    }
    public class VipModule: IMachineItem,EzEventListener<TimeEvent>
    {
        public int quantityCrystal = 30;
        public float discountCrystal = 0.2f;
        public int timeClaimCrystal = 86400;
        public IEnumerator execute(BaseItemGameInstanced itemSlot)
        {
            while (!GameManager.readyForThisState("Main"))
            {
                yield return new WaitForEndOfFrame();
            }
            GameManager.Instance.addFactorSuperIncome(2, -1);
            TimeCounter.Instance.addTimer(new TimeCounterInfo() { id = "vip", destinyIfHave = timeClaimCrystal });
        }
        public void onAdded(BaseItemGameInstanced itemSlot)
        {
            ES3.Save("BlockADS", ES3.Load<float>("BlockADS", 0) + 1);
            ES3.Save("BonusCrystal", ES3.Load<float>("BonusCrystal", 0) +  discountCrystal);
            GameManager.Instance.StartCoroutine(execute(itemSlot));
            EzEventManager.AddListener(this);
        }

        public void onDirtyChange(BaseItemGameInstanced itemSlot)
        {
            GameManager.Instance.StartCoroutine(execute(itemSlot));
        }

        public void onEnable(BaseItemGameInstanced itemSlot)
        {
            GameManager.Instance.StartCoroutine(execute(itemSlot));
            EzEventManager.AddListener(this);
        }

        public void OnEzEvent(TimeEvent eventType)
        {
            if(eventType.timeInfo.id == "vip" && eventType.timeInfo.counterTime > eventType.timeInfo.destinyIfHave)
            {
                var crystal = GameManager.Instance.Database.getItem("Crystal");
                crystal.addQuantity(quantityCrystal.ToString());
                eventType.timeInfo.firstTimeAdd = eventType.timeInfo.firstTimeAdd + eventType.timeInfo.CounterTime -  eventType.timeInfo.CounterTime % timeClaimCrystal;
                HUDManager.Instance.boxReward.show(new ItemRewardInfo[] { new ItemRewardInfo() {
                    itemReward = new ItemWithQuantity()
                    {
                        item = GameDatabase.Instance.getItemInventory("Crystal"),
                        quantity = quantityCrystal.ToString()
                    }
                } });
            }
        }

        public void onRemoved(BaseItemGameInstanced itemSlot)
        {
            ES3.Save("BlockADS", ES3.Load<float>("BlockADS", 0) - 1);
            ES3.Save("BonusCrystal", ES3.Load<float>("BonusCrystal", 0) - discountCrystal);
            var timeX2 = GameManager.Instance.Database.timeRestore.Find(x => x.id == $"[SuperInCome]2" && x.destinyIfHave == -1);
            GameManager.Instance.Database.removeTime(timeX2);
            var time= TimeCounter.Instance.timeCollection.Value.Find(x => x.id == "vip");
            GameManager.Instance.Database.removeTime(time);
        }
    }
    public class UpgradeItemLevelEgg : IMachineItem
    {
        public string egg = "Egg";

        public IEnumerator execute(BaseItemGameInstanced itemSlot)
        {
            while (!GameManager.readyForThisState("Main"))
            {
                yield return new WaitForEndOfFrame();
            }
            var exist = GameManager.Instance.Database.creatureInfos.Find(x => x.id == egg + GameManager.Instance.ZoneChoosed);
            if (exist != null)
                exist.level = (itemSlot.CurrentLevel);
            var creautres = GameManager.Instance.Database.getCreatureExist(egg + GameManager.Instance.ZoneChoosed, GameManager.Instance.ZoneChoosed);
            var creatureOriginal = GameDatabase.Instance.getItemInventory(exist.id);
            var id = ((PackageCreatureObject)creatureOriginal).creatureExtra[itemSlot.CurrentLevel].ItemID;
            foreach(var creature in creautres)
            {
                ((PackageCreatureInstanceSaved) creature).creature = id;
            }
            itemSlot.item.updateString = () => { return $"{ GameManager.Instance.getLevelItem(itemSlot.itemID) - 1} => {GameManager.Instance.getLevelItem(itemSlot.itemID)}"; };
        }
        public void onAdded(BaseItemGameInstanced itemSlot)
        {
           GameManager.Instance.StartCoroutine( execute(itemSlot));
        }

        public void onDirtyChange(BaseItemGameInstanced itemSlot)
        {
            GameManager.Instance.StartCoroutine(execute(itemSlot));
        }

        public void onEnable(BaseItemGameInstanced itemSlot)
        {
            GameManager.Instance.StartCoroutine(execute(itemSlot));
        }

        public void onRemoved(BaseItemGameInstanced itemSlot)
        {

        }
    }
    public class UpgradeLimitSlot : IMachineItem
    {
        public string itemUprade;
        public bool varitantZone = true;
        public string ItemUprade { get => itemUprade+(varitantZone ? GameManager.Instance.ZoneChoosed:"") ; set => itemUprade = value; }
        public void execute(BaseItemGameInstanced itemSlot)
        {
            var exist = GameDatabase.Instance.getItemInventory(ItemUprade);
            if (exist != null && exist.limitInInventory != null)
            {
                exist.limitInInventory.setLevel(itemSlot.CurrentLevel);
                itemSlot.item.updateString = ()=> {
                    return exist.limitInInventory.getUnit(itemSlot.CurrentLevel).toTimeHour() + " => " + exist.limitInInventory.getUnit(itemSlot.CurrentLevel + 1).toTimeHour();
                };
            }
        }
        public void onAdded(BaseItemGameInstanced itemSlot)
        {
            execute(itemSlot);
        }

        public void onDirtyChange(BaseItemGameInstanced itemSlot)
        {
            execute(itemSlot);
        }

        public void onEnable(BaseItemGameInstanced itemSlot)
        {
            execute(itemSlot);
        }

        public void onRemoved(BaseItemGameInstanced itemSlot)
        {

        }
    }
    public class UpgradeTimeRestore : IMachineItem
    {
        public string itemUprade;
        public bool varitantZone = true;
        public string ItemUprade { get => itemUprade + (varitantZone ? GameManager.Instance.ZoneChoosed : ""); set => itemUprade = value; }

        public void execute(BaseItemGameInstanced itemSlot)
        {
            var exist = GameDatabase.Instance.getItemInventory(ItemUprade);
            if (exist != null)
            {
                exist.timeToRestore.setLevel(itemSlot.CurrentLevel);
                itemSlot.item.updateString = () => {
                    return exist.timeToRestore.getUnit(itemSlot.CurrentLevel)+ " => " + exist.timeToRestore.getUnit(itemSlot.CurrentLevel + 1);
                };
            }
        }
        public void onAdded(BaseItemGameInstanced itemSlot)
        {
            execute(itemSlot);
        }

        public void onDirtyChange(BaseItemGameInstanced itemSlot)
        {
            execute(itemSlot);
        }

        public void onEnable(BaseItemGameInstanced itemSlot)
        {
            execute(itemSlot);
        }

        public void onRemoved(BaseItemGameInstanced itemSlot)
        {

        }
    }
    public class MixingScale : IMachineItem, EzEventListener<AddCreatureEvent>
    {
        public UnitDefineLevelIntVariable timeMixing;
        public int MixingTime
        {
            get
            {
                var setting = new ES3Settings();
                setting.location = ES3.Location.Cache;
                return ES3.Load<int>("MixingTime", 0, setting);
            }

            set
            {
                var setting = new ES3Settings();
                setting.location = ES3.Location.Cache;
                ES3.Save<int>("MixingTime", value, setting);
            }
        }

        public void execute(BaseItemGameInstanced itemSlot)
        {
            timeMixing.Value.setLevel(() =>
            {
                return GameManager.Instance.getLevelItem(itemSlot.itemID);
            });
            itemSlot.item.updateString = () => { return timeMixing.Value.getUnit(itemSlot.CurrentLevel) + " => " + timeMixing.Value.getUnit(itemSlot.CurrentLevel + 1); };
        }
        public void onAdded(BaseItemGameInstanced itemSlot)
        {
            EzEventManager.AddListener(this);
            execute(itemSlot);
        }

        public void onDirtyChange(BaseItemGameInstanced itemSlot)
        {
        }

        public void onEnable(BaseItemGameInstanced itemSlot)
        {
            EzEventManager.AddListener(this);
            execute(itemSlot);
        }

        public void OnEzEvent(AddCreatureEvent eventType)
        {
            if (!GameManager.Instance.Database.creatureInfos[7].isUnLock)
            {
                return;
            }
            if (!eventType.manualByHand || eventType.change <= 0) return;
            MixingTime++;
            if (MixingTime >= timeMixing.Value.getCurrentUnit())
            {
                MixingTime = 0;
                var crystal = GameManager.Instance.Database.getItem("Crystal");
                crystal.addQuantity("1");
                var iconPos = HUDManager.Instance.processTimeMixing.transform.parent.GetChild(1).position;
                var Home = System.Array.Find(GameObject.FindObjectsOfType<InventorySlot>(), x => x._info.itemID == "Crystal");
                if (Home)
                {
                    GameObject pObject = new GameObject();
                    var sprite = pObject.AddComponent<UI2DSprite>();
                    sprite.width = 100;
                    sprite.height = 100;
                    var item = GameManager.Instance.Database.getItem("Crystal");
                    item.item.getSpriteForState((o) =>
                    {
                        sprite.sprite2D = o;
                    });
                    pObject.transform.parent = Home.transform;
                    pObject.SetLayerRecursively(HUDManager.Instance.gameObject.layer);
                    pObject.transform.localScale = new Vector3(1, 1, 1);
                    pObject.transform.position = iconPos;
                    NGUITools.BringForward(pObject);
                    Sequence seq = DOTween.Sequence();
                    seq.Append(pObject.transform.DOScale(1.5f, 0.75f));
                    seq.Append( pObject.transform.DOMove(Home.transform.position, 0.5f));
                    seq.Join( pObject.transform.DOScale(0, 0.6f).OnComplete(() =>
                    {
                        GameObject.Destroy(pObject);
                    }));
                }
            }
            HUDManager.Instance.updateTimeMixing();
     
        }

        public void onRemoved(BaseItemGameInstanced itemSlot)
        {
            EzEventManager.RemoveListener(this);
        }
    }
    public class NoAds : IBoosterExecute
    {
#if UNITY_EDITOR
        [UnityEditor.Callbacks.DidReloadScripts]
#endif
        static void AutoRegister()
        {
            if (!ItemBoosterObject.boosterType.ContainsKey("NoAds"))
            {
                ItemBoosterObject.boosterType.Add("NoAds", typeof(NoAds));
            }
        }
        public void Excute(Dictionary<string, object> blackBoard)
        {
            ES3.Save("BlockADS", 1);
        }

        public string getContent(Dictionary<string, object> blackBoard)
        {
            return "Block ADS";
        }
    }
    public class SuperInCome : IBoosterExecute
    {
#if UNITY_EDITOR
        [UnityEditor.Callbacks.DidReloadScripts]
#endif
        static void AutoRegister()
        {
            if (!ItemBoosterObject.boosterType.ContainsKey(nameof(SuperInCome)))
            {
                ItemBoosterObject.boosterType.Add(nameof(SuperInCome), typeof(SuperInCome));
            }
        }
        public void Excute(Dictionary<string, object> blackBoard)
        {
            var factor = 2;
            if (GameManager.Instance.getFactorIncome().x >= 2)
            {
                factor = 4;
            }

            int factorTime = factor == 2 ? 60 : 1;
            double time = blackBoard["time"].GetType() == typeof(double) ? (double)blackBoard["time"] : Random.Range(((Vector2Int)blackBoard["time"]).x, ((Vector2Int)blackBoard["time"]).y);
            GameManager.Instance.addFactorSuperIncome(factor, time/60);
        }

        public string getContent(Dictionary<string, object> blackBoard)
        {
            var factor = 2;
            if(GameManager.Instance.getFactorIncome().x >= 2)
            {
                factor = 4;
            }
            return "x" + factor + " Gold";
        }
    }
    public class DiscountCreature : IBoosterExecute
    {
#if UNITY_EDITOR
        [UnityEditor.Callbacks.DidReloadScripts]
#endif
        static void AutoRegister()
        {
            if (!ItemBoosterObject.boosterType.ContainsKey(nameof(DiscountCreature)))
            {
                ItemBoosterObject.boosterType.Add(nameof(DiscountCreature), typeof(DiscountCreature));
            }
        }
        public void Excute(Dictionary<string, object> blackBoard)
        {
            var discount = (float)blackBoard["DiscountCreature"];
            double time = blackBoard["time"].GetType() == typeof(double) ? (double)blackBoard["time"] : Random.Range(((Vector2Int)blackBoard["time"]).x, ((Vector2Int)blackBoard["time"]).y);
            GameManager.Instance.DiscountCreature(discount, time);
        }

        public string getContent(Dictionary<string, object> blackBoard)
        {
            return "Discount";
        }
    }
    public class ReduceTimeEgg : IBoosterExecute
    {
#if UNITY_EDITOR
        [UnityEditor.Callbacks.DidReloadScripts]
#endif
        static void AutoRegister()
        {
            if (!ItemBoosterObject.boosterType.ContainsKey(nameof(ReduceTimeEgg)))
            {
                ItemBoosterObject.boosterType.Add(nameof(ReduceTimeEgg), typeof(ReduceTimeEgg));
            }
        }
        public void Excute(Dictionary<string, object> blackBoard)
        {
            var discount = (float)blackBoard["ReduceTimeEgg"];
            double time = blackBoard["time"].GetType() == typeof(double) ? (double)blackBoard["time"] : Random.Range(((Vector2Int)blackBoard["time"]).x, ((Vector2Int)blackBoard["time"]).y);
            GameManager.Instance.ReduceTimeEgg(discount, time);
        }

        public string getContent(Dictionary<string, object> blackBoard)
        {
            return "Reduce Time Egg";
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;

namespace Pok
{
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
        public UnitDefineLevelInt timeMixing;
        public int MixingTime
        {
            get
            {
                return ES3.Load<int>("MixingTime", 0);
            }

            set
            {
                ES3.Save<int>("MixingTime", value);
            }
        }

        public void execute(BaseItemGameInstanced itemSlot)
        {
            timeMixing.setLevel(() =>
            {
                return GameManager.Instance.getLevelItem(itemSlot.itemID);
            });
            itemSlot.item.updateString = () => { return timeMixing.getUnit(itemSlot.CurrentLevel) + " => " + timeMixing.getUnit(itemSlot.CurrentLevel + 1); };
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
            if (!eventType.manualByHand || eventType.change <= 0) return;
            MixingTime++;
            if (MixingTime >= timeMixing.getCurrentUnit())
            {
                MixingTime = 0;
                var crystal = GameManager.Instance.Database.getItem("Crystal");
                crystal.addQuantity("1");
            }
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
            var factor = (float)blackBoard["SuperInCome"];
            double time = blackBoard["time"].GetType() == typeof(double) ? (double)blackBoard["time"] : Random.Range(((Vector2Int)blackBoard["time"]).x, ((Vector2Int)blackBoard["time"]).y);
            GameManager.Instance.addFactorSuperIncome(factor, time);
        }

        public string getContent(Dictionary<string, object> blackBoard)
        {
            var factor = (float)blackBoard["SuperInCome"];
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


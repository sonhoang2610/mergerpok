using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;
using ScriptableObjectArchitecture;
using DG.Tweening;

namespace Pok
{
    //public static class PokConstraint
    //{
    //    public static string[] AnotherDataKey = new string[]
    //    {
    //        "WatchADSMission","BlockADS","FirstUnlockPok6",
    //    };
    //}
    public static class PokUltis
    {
        public static void KillTween(string id)
        {
            if (DG.Tweening.DOTween.IsTweening(id))
            {
                DG.Tweening.DOTween.Kill(id,false);
            }
          
        }
        public static string calculateCreaturePrice(int indexWay,int numberBought,CreatureItem creauture)
        {
            if (indexWay == 0)
            {
                System.Numerics.BigInteger startMoney = 560;
                int startIncrease = 5;
                int percentIncrease = 16;
                float startFactor = 1;
                if (GameManager.Instance.ZoneChoosed != "Zone1")
                {
                    
                    startMoney = creauture.goldAFKReward[GameManager.Instance.ZoneChoosed].toBigInt();
               
                    switch (GameManager.Instance.ZoneChoosed)
                    {
                        case "Zone2":
                            startIncrease = 3;
                            percentIncrease = 40;
                            startFactor = 4.6f;
                            break;
                        case "Zone3":
                            startIncrease = 3;
                            percentIncrease = 45;
                            startFactor = 5.1f;
                            break;
                        case "Zone4":
                            startIncrease = 3;
                            percentIncrease = 45;
                            startFactor = 5.6f;
                            break;
                        case "Zone5":
                            startIncrease = 2;
                            percentIncrease = 50;
                            startFactor = 6.6f;
                            break;
                        case "Zone6":
                            startIncrease =2;
                            percentIncrease = 60;
                            startFactor = 7.6f;
                            break;
                    }
                    if (creauture.RankChild > 0)
                    {
                        var startCreature = GameDatabase.Instance.CreatureCollection.Find(x => x.ItemID == "Pok1");
                        startMoney = startCreature.goldAFKReward[GameManager.Instance.ZoneChoosed].toBigInt()*((int)(Mathf.Pow( startFactor,creauture.RankChild)*100))/100;
                    }
                }
                else
                {
                    startMoney = (startMoney * (long)System.Math.Pow(2.82f, creauture.RankChild));
                }
               
                if (numberBought >= startIncrease)
                {
                    for (int i = 0; i < numberBought - startIncrease; ++i)
                    {
                        if (i == 0)
                        {
                            startMoney += 100 * startMoney / 100;
                        }
                        else
                        {
                            startMoney += percentIncrease * startMoney / 100;
                        }
                    }
                }
                startMoney *= (int)((1 - GameManager.Instance.getPercentDiscount().x) * 100);
                startMoney /= 100;
                return startMoney.toString();
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
            ES3.Save("BlockADS", ES3.Load("BlockADS", 0) + 1);
            var oldBonus =  ES3.Load("BonusCrystalVip", 0);
            ES3.Save("BonusCrystalVip", discountCrystal);
            ES3.Save("BonusCrystal", ES3.Load<float>("BonusCrystal", 0) - oldBonus +  discountCrystal);
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
            ES3.Save("BlockADS", ES3.Load("BlockADS", 0) - 1);
            var oldBonus = ES3.Load("BonusCrystalVip", 0.0f);
            if(oldBonus == 0)
            {
                oldBonus = discountCrystal;
            }
            ES3.Save("BonusCrystalVip", 0);
            ES3.Save("BonusCrystal", ES3.Load<float>("BonusCrystal", 0) - oldBonus);
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
                exist.Level = (itemSlot.CurrentLevel);
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
                return ES3.Load<int>("MixingTime", 0);
            }

            set
            {
                ES3.Save<int>("MixingTime", value);
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
     
            var zone = GameManager.Instance.Database.zoneInfos.Find(x => x.Id == GameManager.Instance.ZoneChoosed);
            if (string.IsNullOrEmpty(zone.CurentUnlock)) return;
            string index =string.IsNullOrEmpty(zone.CurentUnlock) ? "1" : zone.CurentUnlock.Replace("Pok", "");
            if (int.Parse( index) < 8)
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
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void AutoRegister()
        {
            if (!ItemBoosterObject.boosterType.ContainsKey("NoAds"))
            {
                Debug.Log("register Type" + nameof(NoAds));
                ItemBoosterObject.boosterType.Add("NoAds", typeof(NoAds));
            }
        }
        public void Excute(Dictionary<string, object> blackBoard)
        {
            ES3.Save("BlockADS", ES3.Load("BlockADS", 0) + 1);
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
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void AutoRegister()
        {
            if (!ItemBoosterObject.boosterType.ContainsKey(nameof(SuperInCome)))
            {
                Debug.Log("register Type" + nameof(SuperInCome));
                ItemBoosterObject.boosterType.Add(nameof(SuperInCome), typeof(SuperInCome));
            }
        }
        public void Excute(Dictionary<string, object> blackBoard)
        {
            var factor = 2;
            if (GameManager.Instance.getFactorIncome().x >= 2 && !blackBoard.ContainsKey("fixed"))
            {
                factor = 4;
            }

            int factorTime = factor == 2 ? 1 : 60;
            double time = blackBoard["time"].GetType() == typeof(double) ? (double)blackBoard["time"] : Random.Range(((Vector2Int)blackBoard["time"]).x, ((Vector2Int)blackBoard["time"]).y);
            GameManager.Instance.addFactorSuperIncome(factor, time/ factorTime);
        }

        public string getContent(Dictionary<string, object> blackBoard)
        {
            var factor = 2;
            if (GameManager.Instance.getFactorIncome().x >= 2 && !blackBoard.ContainsKey("fixed"))
            {
                factor = 4;
            }
            int factorTime = factor == 2 ? 1 : 60;
            double time = blackBoard["time"].GetType() == typeof(double) ? (double)blackBoard["time"] : Random.Range(((Vector2Int)blackBoard["time"]).x, ((Vector2Int)blackBoard["time"]).y);
            time /= factorTime;
            return System.TimeSpan.FromSeconds(time).Hours == 0 ? (System.TimeSpan.FromSeconds(time).Minutes + "M") : (System.TimeSpan.FromSeconds(time).Hours + "H");
        }
    }
    public class DiscountCreature : IBoosterExecute
    {
#if UNITY_EDITOR
        [UnityEditor.Callbacks.DidReloadScripts]
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void AutoRegister()
        {
            if (!ItemBoosterObject.boosterType.ContainsKey(nameof(DiscountCreature)))
            {
                Debug.Log("register Type"+ nameof(DiscountCreature));
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
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void AutoRegister()
        {
            if (!ItemBoosterObject.boosterType.ContainsKey(nameof(ReduceTimeEgg)))
            {
                Debug.Log("register Type" + nameof(ReduceTimeEgg));
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


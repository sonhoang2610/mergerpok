using DG.Tweening;
using EasyMobile;
using EazyEngine.Tools;
using Firebase.Extensions;
using ScriptableObjectArchitecture;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;
using static EasyMobile.StoreReview;

namespace Pok
{
    public class InappPendingInfo
    {
        public string id;
        public System.Action<bool> result;
    }
    public class GameManager : PersistentSingleton<GameManager>, EzEventListener<TimeEvent>, EzEventListener<AddCreatureEvent>, EzEventListener<GameDatabaseInventoryEvent>, EzEventListener<RemoveTimeEvent>
    {

        public static Dictionary<string, int> stateReady = new Dictionary<string, int>();
        public Dictionary<string, Collider[]> behaviorsDisable = new Dictionary<string, Collider[]>();
        public TimeCounterInfoCollection timeCollection;
        public StringVariable currentMap;

        public void deActiveBehaviors(string id, Collider[] behaviors)
        {
            if (!behaviorsDisable.ContainsKey(id))
            {
                behaviorsDisable.Add(id, behaviors);
                foreach (var behavior in behaviors)
                {
                    behavior.enabled = false;
                }
            }
        }
        int guideindex = -1;
        public int GuideIndex
        {
            get
            {
                if (guideindex == -1)
                {
                    guideindex = ES3.Load("FirstGuide", 0);
                }
                return guideindex;
            }

            set
            {
                ES3.Save("FirstGuide", value);
                guideindex = value;
            }
        }
        public IEnumerator checkGuide()
        {
            while (!GameManager.readyForThisState("Main"))
            {
                yield return new WaitForEndOfFrame();
            }

            if (GuideIndex == 0)
            {
                var colliders = FindObjectsOfType<Collider>();
                GameManager.Instance.deActiveBehaviors("FirstGuide0", colliders);
                var time = TimeCounter.Instance.timeCollection.Value.Find(x => x.id.Contains("Egg") && x.id.Contains(GameManager.Instance.ZoneChoosed));
                if (time != null)
                    time.pauseTime(true);
                GuideIndex = 1;
            }
            else if (GuideIndex == 1)
            {
                var colliders = FindObjectsOfType<Collider>();
                var time = TimeCounter.Instance.timeCollection.Value.Find(x => x.id.Contains("Egg") && x.id.Contains(GameManager.Instance.ZoneChoosed));
                if (time != null)
                {
                    time.firstTimeAdd -= time.destinyIfHave;
                    time.pauseTime(false);
                }
                GuideIndex = 2;
            }
            else if (GuideIndex == 2)
            {
                GuideIndex = 3;
            }
            else if (GuideIndex == 3)
            {
                HUDManager.Instance.hand.gameObject.SetActive(true);
                HUDManager.Instance.hand.transform.localPosition = new UnityEngine.Vector3(-200, -200, 0);
                HUDManager.Instance.hand.transform.DOLocalMove(new UnityEngine.Vector3(200, 200, 0), 1.5f).SetLoops(-1, LoopType.Restart);
                GuideIndex = 4;
            }
            else if (GuideIndex == 5)
            {
                GameManager.Instance.ActiveBehaviors("FirstGuide0");
                GuideIndex = 6;
            }
        }
        public void ActiveBehaviors(string id)
        {
            if (!behaviorsDisable.ContainsKey(id)) return;
            var behaviours = behaviorsDisable[id];
            foreach (var behavior in behaviours)
            {
                behavior.enabled = true;
            }
        }
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


        public void addFactorSuperIncome(float factor, double time)
        {
            var timing = GameManager.Instance.Database.timeRestore.Find(x => x.id == $"[SuperInCome]{factor}");
            if (timing != null)
            {
                if(timing.CounterTime > timing.destinyIfHave)
                {
                    GameManager.Instance.Database.removeTime(timing);
                    addFactorSuperIncome(factor, time);
                    return;
                }
                if (time != -1 && timing.destinyIfHave != -1)
                {
                    timing.destinyIfHave += time;
                }
                if (time == -1)
                {
                    timing.destinyIfHave = -1;
                    timing.autoRemoveIfToDestiny = false;
                }
            }
            else
            {
                TimeCounter.Instance.addTimer(new TimeCounterInfo() { id = $"[SuperInCome]{factor}", destinyIfHave = time, autoRemoveIfToDestiny = time != -1 });
            }
            var quantitySecIncome = System.Numerics.BigInteger.Parse(GameManager.Instance.getTotalGoldGrowthCurrentZone()) * (int)GameManager.Instance.getFactorIncome().x;
            HUDManager.Instance.quanityHour.text = quantitySecIncome.ToString().ToKMBTA();
        }
        public void DiscountCreature(float percent, double time)
        {
            var timing = GameManager.Instance.Database.timeRestore.Find(x => x.id == $"[DiscountCreature]{percent}");
            if (timing != null)
            {
                if (timing.CounterTime > timing.destinyIfHave)
                {
                    GameManager.Instance.Database.removeTime(timing);
                    DiscountCreature(percent, time);
                    return;
                }
                timing.destinyIfHave += time;
            }
            else
            {
                TimeCounter.Instance.addTimer(new TimeCounterInfo() { id = $"[DiscountCreature]{percent}", destinyIfHave = time,autoRemoveIfToDestiny=true });
            }
        }
        public void ReduceTimeEgg(float percent, double time)
        {
            var timing = GameManager.Instance.Database.timeRestore.Find(x => x.id == $"[ReduceTimeEgg]{percent}");
            if (timing != null)
            {
                if (timing.CounterTime > timing.destinyIfHave)
                {
                    GameManager.Instance.Database.removeTime(timing);
                    ReduceTimeEgg(percent, time);
                    return;
                }
                timing.destinyIfHave += time;

            }
            else
            {
                TimeCounter.Instance.addTimer(new TimeCounterInfo() { id = $"[ReduceTimeEgg]{percent}", destinyIfHave = time, autoRemoveIfToDestiny = true });
            }
        }
        public UnityEngine.Vector2 getPercentReduceTimeEgg()
        {

            var timings = GameManager.Instance.Database.timeRestore.FindAll(x => x.id.Contains("[ReduceTimeEgg]"));
            UnityEngine.Vector2 factor = new UnityEngine.Vector2(0, 0);
            foreach (var timing in timings)
            {
                float factorTime = float.Parse(timing.id.Remove(0, ("[ReduceTimeEgg]").Length));
                if (factorTime > factor.x)
                {
                    double timeLefxt = (timing.destinyIfHave - timing.CounterTime).Clamp(timing.destinyIfHave, 0);
                    factor = new UnityEngine.Vector2(factorTime, (float)timeLefxt);
                }
            }
            return factor;
        }
        public UnityEngine.Vector2 getPercentDiscount()
        {

            var timings = GameManager.Instance.Database.timeRestore.FindAll(x => x.id.Contains("[DiscountCreature]"));
            UnityEngine.Vector2 factor = new UnityEngine.Vector2(0, 0);
            foreach (var timing in timings)
            {
                float factorTime = float.Parse(timing.id.Remove(0, ("[DiscountCreature]").Length));
                if (factorTime > factor.x)
                {
                    double timeLefxt = (timing.destinyIfHave - timing.CounterTime).Clamp(timing.destinyIfHave, 0);
                    factor = new UnityEngine.Vector2(factorTime, (float)timeLefxt);
                }
            }
            return factor;
        }
        public UnityEngine.Vector2 getFactorIncome()
        {

            var timings = GameManager.Instance.Database.timeRestore.FindAll(x => x.id.Contains("[SuperInCome]"));
            UnityEngine.Vector2 factor = new UnityEngine.Vector2(1, 0);
            foreach (var timing in timings)
            {
                float factorTime = float.Parse(timing.id.Remove(0, ("[SuperInCome]").Length));
                double timeLefxt = (timing.destinyIfHave - timing.CounterTime).Clamp(timing.destinyIfHave, 0);
                if (factorTime > factor.x && (timeLefxt > 0 || timing.destinyIfHave ==-1))
                {
                    factor = new UnityEngine.Vector2(factorTime, (float)timeLefxt);
                }
            }
            return factor;
        }
        public static bool readyForThisState(string state)
        {
            if (stateReady.ContainsKey(state))
            {
                return stateReady[state] == 0;
            }
            return false;
        }
        public static void addDirtyState(string state)
        {
            if (!stateReady.ContainsKey(state))
            {
                stateReady.Add(state, 1);
            }
            else
            {
                stateReady[state]++;
            }

        }
        public static void removeDirtyState(string state)
        {
            if (stateReady.ContainsKey(state))
            {
                stateReady[state]--;
            }
        }
        protected GameDatabaseInstanced _database;
        public UnityEngine.Vector2 resolution = new UnityEngine.Vector2(1080, 1920);
        public GameDatabaseInstanced _defaultDatabase;
        public BaseItemGameInstanced[] itemADDIfNotExist;
        public GameDatabaseInstanced Database
        {
            get
            {
                return _database == null ? _database = LoadDatabaseGame() : _database;
            }
        }
        protected string _zoneChoosed;
        public string ZoneChoosed
        {
            set
            {
                ES3.Save("ZoneChoosed", value);
                _zoneChoosed = value;
            }
            get
            {
                if (string.IsNullOrEmpty(_zoneChoosed))
                {
                    return ES3.Load<string>(key: "ZoneChoosed", defaultValue: GameDatabase.Instance.ZoneCollection[0].ItemID);
                }
                return _zoneChoosed;
            }
        }

        [SerializeField]
        [HideInInspector]
        private int generateID = -1;
        public int GenerateID
        {
            set
            {
                ES3.Save("GenerateID", value);
                generateID = value;
            }
            get
            {
                if (generateID == -1)
                {
                    return ES3.Load<int>(key: "GenerateID", defaultValue: 0);
                }
                return generateID;
            }
        }

        public static UnityEngine.Vector3 ScaleFactor
        {
            get
            {
                return new UnityEngine.Vector3(1, 1, 1);
            }
        }

        protected override void Awake()
        {
            base.Awake();
#if !UNITY_EDITOR
          //  Debug.unityLogger.logEnabled = false;
#endif
            ES3.CacheFile();
            var guide = ES3.Load("FirstGuide", 0);
            if (guide > 0 && guide < 5)
            {
                ES3.Save("FirstGuide", 5);
            }
            if (!GameDatabase.Instance.isInit)
            {
                GameDatabase.Instance.onInit();
            }

            _database = LoadDatabaseGame();
            for (int i = 0; i < itemADDIfNotExist.Length; ++i)
            {
                var item = _database.inventory.Find(x => x.itemID == itemADDIfNotExist[i].itemID);
                if (item == null)
                {
                    _database.inventory.Add(ES3.Deserialize<BaseItemGameInstanced>(ES3.Serialize<BaseItemGameInstanced>(itemADDIfNotExist[i])));
                }
                else if (item.EmptySlot || item.QuantityBig <= 0)
                {
                    var info = ES3.Deserialize<BaseItemGameInstanced>(ES3.Serialize<BaseItemGameInstanced>(itemADDIfNotExist[i]));
                    _database.inventory[_database.inventory.IndexOf(item)] = info;
                }
            }

            _database.InitDatabase();
            _zoneChoosed = ZoneChoosed;
            if (!InAppPurchasing.IsInitialized())
            {
                InAppPurchasing.InitializePurchasing();
       
            }
            if (!RuntimeManager.IsInitialized())
            {
                RuntimeManager.Init();
                Advertising.LoadRewardedAd();
                Advertising.LoadInterstitialAd();
            }
            InAppPurchasing.PurchaseCompleted += PurchaseComplete;
            InAppPurchasing.PurchaseFailed += PurchaseFailed;
            Advertising.RewardedAdCompleted += AdComplete;
            Advertising.RewardedAdSkipped += AdSkipped;
            StartCoroutine(checkFetchData());
        }
        public IEnumerator checkFetchData()
        {
            while (!GameManager.readyForThisState("Main"))
            {
                yield return new WaitForEndOfFrame();
            }
            while (SceneManager.Instance.app == null)
            {
                yield return new WaitForEndOfFrame();
            }
            FetchDataAsync();
        }

        public void AdComplete(RewardedAdNetwork net, AdPlacement placement)
        {
            IsShowingADS = false;
            foreach (var result in resultWatch)
            {
                result(true);
            }
            resultWatch.Clear();
        }
        public void AdSkipped(RewardedAdNetwork net, AdPlacement placement)
        {
            IsShowingADS = false;
            foreach (var result in resultWatch)
            {
                result(false);
            }
            resultWatch.Clear();
        }
        private void Start()
        {
            StartCoroutine(ScheduleSaveGame());
        }
        public List<InappPendingInfo> inappPending = new List<InappPendingInfo>();
        public void PurchaseComplete(IAPProduct product)
        {
            var inapps = inappPending.FindAll(x => x.id.ToLower() == product.Id.ToLower());
            foreach (var inapp in inapps)
            {
                inapp.result?.Invoke(true);
                inappPending.Remove(inapp);
            }
        }
        public void PurchaseFailed(IAPProduct product)
        {
            var inapps = inappPending.FindAll(x => x.id.ToLower() == product.Id.ToLower());
            foreach (var inapp in inapps)
            {
                inapp.result?.Invoke(false);
                inappPending.Remove(inapp);
            }
        }
        public CreatureItem[] items;
        [ContextMenu("removeVip")]
        public void removeVip()
        {
            var exist = GameManager.Instance.Database.getItem("VipItem");
            exist.setQuantity("0");
        }
        [ContextMenu("hack")]
        public void hack()
        {
            HUDManager.Instance.boxPackedInapp.showData(GameDatabase.Instance.startedKit);
            var quantityAdd = System.Numerics.BigInteger.Parse(("999ao").clearDot());
            GameManager.Instance.Database.getItem("Coin").addQuantity(quantityAdd.ToString());
            //for (int i = 0; i < items.Length - 2; ++i)
            //{
            //    var list = new List<CreatureItem>();
            //    items[i].getChild(list, 6);
            //    foreach (var element in list)
            //    {
            //        GameManager.Instance.Database.creatureInfos.Find(x => x.id == element.ItemID).isUnLock = true;
            //    }
            //}
            //GameManager.Instance.Database.creatureInfos.Find(x => x.id == items[items.Length - 2].ItemID).isUnLock = true;
            //GameManager.Instance.Database.creatureInfos.Find(x => x.id == items[items.Length - 1].ItemID).isUnLock = true;
            // GameManager.Instance.Database.zoneInfos[0].addLeader
        }
        public void hack1()
        {
            var quantityAdd = (int)getFactorIncome().x * System.Numerics.BigInteger.Parse(getTotalGoldGrowthCurrentZone().clearDot()) * 7200;
            //if(quantityAdd < System.Numerics.BigInteger.Parse(("999ak").clearDot()))
            //{
            //    quantityAdd = System.Numerics.BigInteger.Parse(("999ak").clearDot());
            //}
            GameManager.Instance.Database.getItem("Coin").addQuantity(quantityAdd.ToString());
            //for (int i = 0; i < items.Length - 2; ++i)
            //{
            //    var list = new List<CreatureItem>();
            //    items[i].getChild(list, 6);
            //    foreach (var element in list)
            //    {
            //        GameManager.Instance.Database.creatureInfos.Find(x => x.id == element.ItemID).isUnLock = true;
            //    }
            //}
            //GameManager.Instance.Database.creatureInfos.Find(x => x.id == items[items.Length - 2].ItemID).isUnLock = true;
            //GameManager.Instance.Database.creatureInfos.Find(x => x.id == items[items.Length - 1].ItemID).isUnLock = true;
            // GameManager.Instance.Database.zoneInfos[0].addLeader
        }
        public string getTotalGoldGrowthCurrentZone()
        {
            var creatures = GameManager.Instance.Database.getAllCreatureInstanceInZone(GameManager.Instance.ZoneChoosed);
            var total = System.Numerics.BigInteger.Parse("0");
            for (int i = creatures.Count - 1; i >= 0; --i)
            {
                var creature = creatures[i];
                var original = GameDatabase.Instance.CreatureCollection.Find(x => x.ItemID == creature.id);
                if (original == null)
                {
                    creatures.RemoveAt(i);
                    continue;
                }
                total += System.Numerics.BigInteger.Parse(original.getGoldAFK(GameManager.Instance.ZoneChoosed)) * creatures[i].level;
            }
            return total.toString();
        }
        public GameDatabaseInstanced LoadDatabaseGame()
        {
            GameDatabaseInstanced pDatabase = new GameDatabaseInstanced();
            if (ES3.FileExists() || !ES3.FileExists(new ES3Settings() { path = SaveDataConstraint.ANOTHER }))
            {
                pDatabase = ES3.Load<GameDatabaseInstanced>("Database", ES3.Deserialize<GameDatabaseInstanced>(ES3.Serialize(_defaultDatabase)));
                breakComponentSaveDatabase(SaveDataConstraint.INVENTORY, pDatabase);
                breakComponentSaveDatabase(SaveDataConstraint.WORLD_INFO, pDatabase);
                breakComponentSaveDatabase(SaveDataConstraint.WORLD_DATA, pDatabase);
                breakComponentSaveDatabase(SaveDataConstraint.TIME_DATA, pDatabase);
                if (ES3.FileExists())
                {
                    var keys = ES3.GetKeys();
                    foreach (var key in keys)
                    {
                        if (key != "Database")
                        {
                            var value = ES3.Load(key);
                            ES3.Save(key: key, value, value.GetType(), new ES3Settings() { location = ES3.Location.Cache, path = SaveDataConstraint.ANOTHER });
                            ES3.markDirty(SaveDataConstraint.ANOTHER);
                        }
                    }
                    ES3.DeleteFile();
                }
            }
            else
            {
                ES3.CacheFile(SaveDataConstraint.ANOTHER);
                loadComponentDatabase(pDatabase);
            }
            return pDatabase;
        }
        public void loadComponentDatabase(GameDatabaseInstanced database)
        {
            database.inventory = ES3.Load("Data", SaveDataConstraint.INVENTORY, database.inventory);
            database.worldData = ES3.Load("Data", SaveDataConstraint.WORLD_DATA, database.worldData);
            database.timeRestore = ES3.Load("Data", SaveDataConstraint.TIME_DATA, database.timeRestore);
            database.creatureInfos = ES3.Load("Data", SaveDataConstraint.WORLD_INFO, database.creatureInfos);
            database.zoneInfos = ES3.Load("Data1", SaveDataConstraint.WORLD_INFO, database.zoneInfos);
        }
        public void breakComponentSaveDatabase(string path, GameDatabaseInstanced database)
        {
            switch (path)
            {
                case SaveDataConstraint.INVENTORY:
                    ES3.SaveComponentDatabase("Data", SaveDataConstraint.INVENTORY, database.inventory);
                    break;
                case SaveDataConstraint.WORLD_DATA:
                    ES3.SaveComponentDatabase("Data", SaveDataConstraint.WORLD_DATA, database.worldData);
                    break;
                case SaveDataConstraint.WORLD_INFO:
                    ES3.SaveComponentDatabase("Data", SaveDataConstraint.WORLD_INFO, database.creatureInfos);
                    ES3.SaveComponentDatabase("Data1", SaveDataConstraint.WORLD_INFO, database.zoneInfos);
                    break;
                case SaveDataConstraint.TIME_DATA:
                    ES3.SaveComponentDatabase("Data", SaveDataConstraint.TIME_DATA, database.timeRestore);
                    break;
            }
        }

        public int getLevelItem(string item)
        {
            var originalItem = GameDatabase.Instance.getItemInventory(item);
            if (originalItem.categoryItem == CategoryItem.COMMON)
            {
                var pInfo = Database.getItem(originalItem.ItemID);
                return pInfo.CurrentLevel;
            }
            else if (originalItem.categoryItem == CategoryItem.CREATURE)
            {
                return Database.creatureInfos.Find(x => x.id == item).Level;
            }
            return 0;
        }
        public int getNumberBoughtItem(string item)
        {
            var originalItem = GameDatabase.Instance.getItemInventory(item);
            if (originalItem.categoryItem == CategoryItem.COMMON)
            {
                var pInfo = Database.getItem(originalItem.ItemID);
                return pInfo.boughtNumber;
            }
            else if (originalItem.categoryItem == CategoryItem.CREATURE)
            {
                return Database.creatureInfos.Find(x => x.id == item).BoughtNumber;
            }
            return 0;
        }
        private void OnEnable()
        {
            EzEventManager.AddListener<TimeEvent>(this);
            EzEventManager.AddListener<AddCreatureEvent>(this);
            EzEventManager.AddListener<GameDatabaseInventoryEvent>(this);
            EzEventManager.AddListener<RemoveTimeEvent>(this);
        }
        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                SaveGame();
            }
        
        }
        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                SaveGame();
            }
        }
        private void OnDisable()
        {
            EzEventManager.RemoveListener<TimeEvent>(this);
            EzEventManager.RemoveListener<AddCreatureEvent>(this);
            EzEventManager.RemoveListener<GameDatabaseInventoryEvent>(this);
            EzEventManager.RemoveListener<RemoveTimeEvent>(this);
            SaveGame();
        }
        private void OnDestroy()
        {
            SaveGame();
        }
        private void OnApplicationQuit()
        {
            SaveGame();
        }
        public static bool blockingSave = false;
        public Coroutine coroutineBlock;
        public IEnumerator resetBlock()
        {
            yield return new WaitForSeconds(3);
            blockingSave = false;
        }
        public void SaveGame()
        {
            if (blockingSave)
            {
                if (coroutineBlock == null && !GameManager.InstanceRaw.IsDestroyed() && GameManager.InstanceRaw.gameObject.activeSelf)
                {
                    coroutineBlock = StartCoroutine(resetBlock());
                }
                return;
            }
            blockingSave = true;
            breakComponentSaveDatabase(SaveDataConstraint.INVENTORY,_database);
            breakComponentSaveDatabase(SaveDataConstraint.WORLD_INFO, _database);
            breakComponentSaveDatabase(SaveDataConstraint.WORLD_DATA, _database);
            breakComponentSaveDatabase(SaveDataConstraint.TIME_DATA, _database);
            for (int i = 0; i < ES3.dirty.Count; ++i)
            {
                if (ES3.dirty.Values.ElementAt(i) || ES3.dirty.Keys.ElementAt(i) == SaveDataConstraint.TIME_DATA)
                {
                    ES3.StoreCachedFile(new ES3Settings() {path = ES3.dirty.Keys.ElementAt(i) });
                }
            }
     
            if (coroutineBlock != null)
            {
                StopCoroutine(coroutineBlock);
                coroutineBlock = null;
            }
            ES3.clearDirty();
            blockingSave = false;
            //Thread thread = new Thread(delegate ()
            //{
            //    try
            //    {
            //        ES3.StoreCachedFile();
            //    }
            //    catch
            //    {

            //    }
            //    finally
            //    {
            //        UnityToolbag.Dispatcher.InvokeAsync(() =>
            //        {
            //            if (coroutineBlock != null)
            //            {
            //                StopCoroutine(coroutineBlock);
            //                coroutineBlock = null;
            //            }
            //            ES3.dirty = false;
            //            blockingSave = false;
            //        });
            //    }

            //});
            ////Start the Thread and execute the code inside it
            //thread.Start();

        }
        int blockDirty = 0;
        public IEnumerator ScheduleSaveGame()
        {
            yield return new WaitForSeconds(2);
            if (ES3.isDirty())
            {
                SaveGame();
            }
            else
            {
                //blockDirty++;
                //if (blockDirty >= 5)
                //{
                //    blockDirty = 0;
                //    ES3.dirty = true;
                //}
            }
            StartCoroutine(ScheduleSaveGame());
        }

        public void checkShowAds()
        {
            if (readyShowInterstitialAd && Advertising.IsInterstitialAdReady() && TimeCounter.Instance.breakTime <= 0)
            {
                IsShowingADS = true;
                Advertising.ShowInterstitialAd();
                StartCoroutine(showADS());
                readyShowInterstitialAd = false;
            }
            else if(readyShowInterstitialAd)
            {
                readyShowInterstitialAd = false;
                StartCoroutine(showADS(5));
            }
        }
        [System.NonSerialized]
        public bool readyShowInterstitialAd = false;
        int indexShowADS = 0;
        string[] arrayTimeShowADS;

        public IEnumerator showADS(float sec = 0)
        {
            yield return new WaitForSeconds(sec == 0 ? int.Parse(arrayTimeShowADS[indexShowADS]) : sec);
            if (ES3.Load("BlockADS", 0) > 0) { yield return null; }
            if (IsShowingADS)
            {
                StartCoroutine(showADS(5));
                yield return null;
            }
            if (Advertising.IsInterstitialAdReady() && TimeCounter.Instance.breakTime <= 0)
            {
                indexShowADS++;
                if (indexShowADS >= arrayTimeShowADS.Length)
                {
                    indexShowADS = arrayTimeShowADS.Length - 1;
                }
                readyShowInterstitialAd = true;
            }
            else if (TimeCounter.Instance.breakTime <= 0)
            {
                Advertising.LoadInterstitialAd();
                StartCoroutine(showADS(5));
            }
            else 
            {
                StartCoroutine(showADS(5));
            }

        }
        int indexFecth = 0;


        public void FetchComplete(Task task)
        {
            var arrayTime = Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue("time_bonus_evo").StringValue.Split(',');
            randomTimeBosnusEvolution = new Vector2Int(int.Parse(arrayTime[0]), int.Parse(arrayTime[1]));
            arrayTime = Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue("time_magic_case").StringValue.Split(',');
            GameDatabase.Instance.timeAFKShowBoxTreasure = int.Parse(arrayTime[0]);
            arrayTime = Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue("time_reward_ads").StringValue.Split(',');
            randomTimeRewardAds = new Vector2Int(int.Parse(arrayTime[0]), int.Parse(arrayTime[1]));
            arrayTime = Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue("time_switch_app").StringValue.Split(',');
            randomTimeSwitchApp = new Vector2Int(int.Parse(arrayTime[0]), int.Parse(arrayTime[1]));
            arrayTimeShowADS = Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue("time_delay_ads").StringValue.Split(',');
            StartCoroutine(showADS(0));
            var info = Firebase.RemoteConfig.FirebaseRemoteConfig.Info;
            switch (info.LastFetchStatus)
            {
                case Firebase.RemoteConfig.LastFetchStatus.Success:
                    Firebase.RemoteConfig.FirebaseRemoteConfig.ActivateFetched();
                    Debug.Log(String.Format("Remote data loaded and ready (last fetch time {0}).",
                                           info.FetchTime));
                    break;
                case Firebase.RemoteConfig.LastFetchStatus.Failure:
                    switch (info.LastFetchFailureReason)
                    {
                        case Firebase.RemoteConfig.FetchFailureReason.Error:
                            Debug.Log("Fetch failed for unknown reason");
                            break;
                        case Firebase.RemoteConfig.FetchFailureReason.Throttled:
                            Debug.Log("Fetch throttled until " + info.ThrottledEndTime);
                            break;
                    }
                    break;
                case Firebase.RemoteConfig.LastFetchStatus.Pending:
                    Debug.Log("Latest Fetch call still pending.");
                    break;
            }
            indexFecth++;
            if (indexFecth == 1)
            {
                FetchDataAsync();
            }
        }

        public Task FetchDataAsync()
        {
            System.Threading.Tasks.Task fetchTask = Firebase.RemoteConfig.FirebaseRemoteConfig.FetchAsync(
                TimeSpan.Zero);
            return fetchTask.ContinueWithOnMainThread(FetchComplete);
        }
        Vector2Int randomTimeBosnusEvolution = new Vector2Int(300, 1500), randomTimeRewardAds = new Vector2Int(300, 1500),randomTimeSwitchApp;
        public int TimeDelayADSSwitchApp
        {
            get
            {
                return UnityEngine.Random.Range(randomTimeSwitchApp.x, randomTimeSwitchApp.y);
            }
        }
        public int TimeDelayBonusEvolution
        {
            get
            {
                return UnityEngine.Random.Range(randomTimeBosnusEvolution.x, randomTimeBosnusEvolution.y);
            }
        }
        public int TimeDelayBoxRewardADS
        {
            get
            {
                return UnityEngine.Random.Range(randomTimeRewardAds.x, randomTimeRewardAds.y);
            }
        }
        public static double lastTimeShowAds = 0;
        public bool IsShowingADS { get => isShowingADS; set {
                isShowingADS = value;
                lastTimeShowAds = TimeCounter.CounterValue;
            }
        }

        public void tryShowBoxRewardAds()
        {
            if (TimeCounter.InstanceRaw.IsDestroyed()) return;
            if (TimeCounter.CounterValue < 30) return;
            var time = TimeCounter.Instance.timeCollection.Value.Find(x => x.id.Contains("RewardADS"));
            if (time != null)
            {
                return;
            }
            var zone = GameManager.Instance.Database.zoneInfos.Find(x => x.Id == GameManager.Instance.ZoneChoosed);
            string index = zone.CurentUnlock.Replace("Pok", "");
            int indexInt = 0;
            if (!int.TryParse( index,out indexInt) || indexInt < 8)
            {
                return;
            }
            TimeCounter.Instance.addTimer(new TimeCounterInfo() { id = $"[Block]RewardADS", autoRemoveIfToDestiny = true, destinyIfHave =  GameManager.Instance.TimeDelayBoxRewardADS , resetOnStart = true });

            HUDManager.Instance.showBoxRewardADS();
        }
        public void showBoxRate(System.Action<UserAction> callback)
        {
            StoreReview.RequestRating(null, callback);
        }
        public void OnEzEvent(TimeEvent eventType)
        {
            var eventString = eventType.timeInfo.id;
            if (eventString.Contains("[Restore]"))
            {
                eventString = eventString.Remove(0, 9);
                var timeID = eventString.Split('/');
                var itemExist = timeID.Length == 1 ? GameManager.Instance.Database.getItem(eventString) : GameManager.Instance.Database.getCreatureItem(timeID[1], timeID[0]);
                if (itemExist != null && eventType.timeInfo.destinyIfHave != -1 && eventType.timeInfo.counterTime >= eventType.timeInfo.destinyIfHave)
                {
                    Database.checkTimeItem(timeID.Length == 2 ? timeID[1] : eventString);
                }
            }
            if (eventType.timeInfo.id.Contains("SuperInCome"))
            {
                var factor = GameManager.Instance.getFactorIncome();
                if (HUDManager.InstanceRaw)
                    HUDManager.Instance.timeXInCome.transform.parent.gameObject.SetActive(factor.y > 0);
                if (factor.y > 0)
                {

                    var timeSpan = TimeSpan.FromSeconds(factor.y);
                    if (HUDManager.InstanceRaw)
                        HUDManager.Instance.timeXInCome.text = string.Format("{0}H {1}M {2}S", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
                }
                else if (eventType.timeInfo.destinyIfHave != -1)
                {
                    GameManager.Instance.Database.removeTime(eventType.timeInfo);
                }
            }
            if (HUDManager.InstanceRaw)
                HUDManager.Instance.factorGoldToBuy.text = (getFactorIncome().x < 2) ? "x2" : "x4";
            if (HUDManager.InstanceRaw)
                HUDManager.Instance.factorGoldToBuyActive("LimitFactor", getFactorIncome().x < 4);
            if (eventType.timeInfo.id.Contains("DiscountCreature"))
            {
                var factor = GameManager.Instance.getPercentDiscount();
                if (HUDManager.InstanceRaw)
                    HUDManager.Instance.timeDisCountCreature.transform.parent.gameObject.SetActive(factor.y > 0);
                if (factor.y > 0)
                {

                    var timeSpan = TimeSpan.FromSeconds(factor.y);
                    HUDManager.Instance.timeDisCountCreature.text = string.Format("{0}H {1}M {2}S", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
                }
                else
                {
                    GameManager.Instance.Database.removeTime(eventType.timeInfo);
                }
            }
            if (eventType.timeInfo.id.Contains("ReduceTimeEgg"))
            {
                var factor = GameManager.Instance.getPercentReduceTimeEgg();
                if (HUDManager.InstanceRaw)
                    HUDManager.Instance.timeEggReduce.transform.parent.gameObject.SetActive(factor.y > 0);
                if (factor.y > 0)
                {

                    var timeSpan = TimeSpan.FromSeconds(factor.y);
                    if (HUDManager.InstanceRaw)
                        HUDManager.Instance.timeEggReduce.text = string.Format("{0}H {1}M {2}S", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
                }
                else
                {
                    GameManager.Instance.Database.removeTime(eventType.timeInfo);
                }

            }
            if (eventType.timeInfo.counterTime >= eventType.timeInfo.destinyIfHave && eventType.timeInfo.autoRemoveIfToDestiny)
            {
                GameManager.Instance.Database.removeTime(eventType.timeInfo);
            }
        }


        public ItemWithQuantity[] claimItem(BaseItemGame item, string quantity = "1", float bonus = 0,int level = 0)
        {
            if (typeof(IUsageItem).IsAssignableFrom(item.GetType()) && level == 0)
            {
                if (((IUsageItem)item).useWhenClaim())
                {
                    if (typeof(IExtractItem).IsAssignableFrom(item.GetType()))
                    {
                        var items = ((IExtractItem)item).ExtractHere();
                        for (int i = 0; i < items.Length; ++i)
                        {
                            //  var itemExist = GameManager.Instance.Database.getItem(items[i].item.ItemID);
                            // items[i].quantity = ((BigInteger.Parse(items[i].quantity.clearDot()) * (int)((1 + bonus) * 100) ) / 100).ToString();
                            //  itemExist.addQuantity(items[i].quantity);
                            claimItem(items[i].item, items[i].quantity, 0, level: level +1);
                        }
                        return items;
                    }
                    if (typeof(ItemBoosterObject).IsAssignableFrom(item.GetType()))
                    {
                        ((ItemBoosterObject)item).executeBooster();
                    }
                }
            }
            else
            {
                var itemExist = GameManager.Instance.Database.getItem(item.ItemID);
                itemExist.addQuantity(quantity);
            }
            return new ItemWithQuantity[] { new ItemWithQuantity() { item = item, quantity = quantity } };
        }
        public void RequestInappForItem(string id, System.Action<bool> result)
        {
#if UNITY_EDITOR
            // result.Invoke(true);
#endif
            if (!inappPending.Exists(x => x.id == id))
            {
                inappPending.Add(new InappPendingInfo() { id = id, result = result });
            }
            Debug.Log("request inapp" + id);
            InAppPurchasing.PurchaseWithId(id);

        }
        public void OnEzEvent(AddCreatureEvent eventType)
        {
            if (eventType.change < 0)
            {
                Database.checkTimeItem($"Egg{eventType.zoneid}");
            }
            ES3.dirty[SaveDataConstraint.WORLD_DATA] = true;
        }
        public IEnumerator delayAction(float sec, System.Action action)
        {
            yield return new WaitForSeconds(sec);
            action?.Invoke();
        }
        [System.NonSerialized]
        private bool isShowingADS = false;
        public void WatchRewardADS(string id, System.Action<bool> result = null)
        {
            TimeCounter.Instance.breakTime = 120;
            LogEvent("WATCH_ADS:" + id);
            //if(ES3.Load("BlockADS", 0) > 0)
            //{
            //    result?.Invoke(true);
            //}
#if UNITY_EDITOR
            result?.Invoke(true);

#else
            resultWatch.Add(result);
            isShowingADS = true;
            Advertising.ShowRewardedAd();
#endif
        }

        public class LoadADSStatus
        {
            public System.Action<bool> resultLoad;
            public float time;
        }
        protected List<System.Action<bool>> resultWatch = new List<Action<bool>>();
        protected List<LoadADSStatus> resultLoad = new List<LoadADSStatus>();
        public bool loadingADS = false;
        protected bool resulting = false;
        public void LoadRewardADS(string id, System.Action<bool> result = null)
        {
            LogEvent("WATCH_ADS:" + id);
#if UNITY_EDITOR
            StartCoroutine(delayAction(1, () =>
            {
                result?.Invoke(UnityEngine.Random.Range(0, 2) == 0);
            }));
#else
                  if (!Advertising.IsRewardedAdReady())
            {
                resultLoad.Add(new LoadADSStatus() { time = 5,resultLoad = result});
                Advertising.LoadRewardedAd();
            }
            else
            {
                result?.Invoke(true);
            }
#endif


        }

        public void checkADSReady()
        {
            for (int i = resultLoad.Count - 1; i >= 0; --i)
            {
                resultLoad[i].time-=Time.deltaTime;
                if (Advertising.IsRewardedAdReady())
                {
                    resultLoad[i].resultLoad?.Invoke(true);
                    resultLoad.RemoveAt(i);
                }
                else if (resultLoad[i].time <= 0)
                {
                    resultLoad[i].resultLoad?.Invoke(false);
                    resultLoad.RemoveAt(i);
                }
            }
        }
        private void LateUpdate()
        {
            checkADSReady();
        }

        public bool isRewardADSReady(string id)
        {
#if UNITY_EDITOR
            return UnityEngine.Random.Range(0, 2) == 0;
#endif
            if (string.IsNullOrEmpty(id))
            {
                return Advertising.IsRewardedAdReady();
            }
            else
            {
                return Advertising.IsRewardedAdReady();
            }

        }


        public void LogEvent(string eventString)
        {

        }
        public IEnumerator actionOnEndFrame(System.Action action)
        {
            yield return new WaitForEndOfFrame();
            action?.Invoke();
        }
        public void OnEzEvent(GameDatabaseInventoryEvent eventType)
        {
            if (BigInteger.Parse(eventType.item.changeQuantity) < 0)
            {
                Database.checkTimeItem(eventType.item.itemID);
            }
            if (eventType.item.item.ItemID.Contains("TicketSpin"))
            {
               if( BigInteger.Parse(eventType.item.changeQuantity) > 0)
                {
                    if (eventType.item.QuantityBig >= eventType.item.item.limitInInventory.getCurrentUnit())
                    {
                        var time = TimeCounter.Instance.timeCollection.Value.Find(x => x.id.Contains("TicketSpin"));
                        if(time != null)
                        {
                            GameManager.Instance.Database.removeTime(time);
                        }
                    }
                }
            }
            if (eventType.item.item.ItemID.Contains("CoinBank"))
            {
                var exist = GameManager.Instance.Database.getItem(eventType.item.item.ItemID);
                var quantity = exist.quantity;
                var moneyAdd = quantity.toInt() * getTotalGoldGrowthCurrentZone().toBigInt() * ((quantity.toInt() > 300 )? 1 : (int)getFactorIncome().x);
                if (eventType.item.changeQuantity.toDouble() > 0 && eventType.item.changeQuantity.toDouble() < GameDatabase.Instance.timeMinToShowBoxBank)
                {
                    StartCoroutine(actionOnEndFrame(() =>
                    {
                        exist.addQuantity((-quantity.toInt()).ToString(), false);
                        var coin = GameManager.Instance.Database.getItem("Coin");
                        coin.addQuantity(moneyAdd.toString(), false);
                    }));

                }
                else if (eventType.item.changeQuantity.toDouble() > 0)
                {
                    if (HUDManager.InstanceRaw && moneyAdd > 0)
                    {
                        HUDManager.Instance.boxBank.show(moneyAdd.toString());
                    }
                    StartCoroutine(actionOnEndFrame(() =>
                    {
                        exist.addQuantity((-quantity.toInt()).ToString(), false);
                    }));
                }
            }
        }

        public void OnEzEvent(RemoveTimeEvent eventType)
        {
            if (HUDManager.InstanceRaw)
            {
                HUDManager.Instance.factorGoldToBuy.text = (getFactorIncome().x < 2) ? "x2" : "x4";
                HUDManager.Instance.factorGoldToBuyActive("LimitFactor", getFactorIncome().x < 4);
                if (GameManager.readyForThisState("Main"))
                {
                    if (GameManager.InstanceRaw)
                    {
                        var quantitySecIncome = System.Numerics.BigInteger.Parse(GameManager.Instance.getTotalGoldGrowthCurrentZone()) * (int)GameManager.Instance.getFactorIncome().x;
                        HUDManager.Instance.quanityHour.text = quantitySecIncome.ToString().ToKMBTA();
                    }
                }
            }
      
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using EazyEngine.Tools;
using ScriptableObjectArchitecture;
using Sirenix.OdinInspector;

namespace Pok {
    public class GameManager : PersistentSingleton<GameManager>,EzEventListener<TimeEvent>
    {

        public static Dictionary<string, int> stateReady = new Dictionary<string, int>();

        public TimeCounterInfoCollection timeCollection;
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
            stateReady[state]++;
        }
        public static void removeDirtyState(string state)
        {
            if (stateReady.ContainsKey(state))
            {
                stateReady[state]--;
            }
        }
        protected GameDatabaseInstanced _database;
        public Vector2 resolution = new Vector2(1080, 1920);
        public GameDatabaseInstanced _defaultDatabase;
        public GameDatabaseInstanced Database
        {
            get
            {
                return _database == null ? _database = LoadDatabaseGame() : _database;
            }
        }
        protected string _zoneChoosed;
        public string  ZoneChoosed
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

        public static Vector3 ScaleFactor
        {
            get
            {
                return new Vector3(1.5f,1.5f,1);
            }
        }
        
        protected override void Awake()
        {
            base.Awake();
           if(!GameDatabase.Instance.isInit)
            {
                GameDatabase.Instance.onInit();
            }
       
            _database = LoadDatabaseGame();
            _database.InitDatabase();
            _zoneChoosed = ZoneChoosed;
        }

        public string getTotalGoldGrowthCurrentZone()
        {
            var creatures = GameManager.Instance.Database.getAllCreatureInstanceInZone(GameManager.Instance.ZoneChoosed);
            var total = System.Numerics.BigInteger.Parse("0");
            foreach(var creature  in creatures)
            {
               var original = GameDatabase.Instance.CreatureCollection.Find(x => x.ItemID == creature.id);
                total += System.Numerics.BigInteger.Parse( original.goldAFKReward[GameManager.Instance.ZoneChoosed]);
            }
            return total.ToString();
        }
        public GameDatabaseInstanced LoadDatabaseGame()
        {
            var pDataBase = ES3.Load<GameDatabaseInstanced>("Database", ES3.Deserialize<GameDatabaseInstanced>(ES3.Serialize(_defaultDatabase)));
            return pDataBase;
        }

        public int getLevelItem(string item)
        {
           var originalItem =GameDatabase.Instance.getItemInventory(item);
            if(originalItem.categoryItem == CategoryItem.COMMON)
            {
                var pInfo = Database.getItem(originalItem.ItemID);
                return pInfo.CurrentLevel;
            }
            else if(originalItem.categoryItem == CategoryItem.CREATURE )
            {
                return Database.creatureInfos.Find(x => x.id == item).level;
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
                return Database.creatureInfos.Find(x => x.id == item).boughtNumber;
            }
            return 0;
        }
        private void OnEnable()
        {
            EzEventManager.AddListener(this);
        }

        private void OnDisable()
        {
            EzEventManager.RemoveListener(this);
        }
        private void OnDestroy()
        {
            ES3.Save("Database", Database);
        }
        public void SaveGame()
        {
       
        }

        public void OnEzEvent(TimeEvent eventType)
        {
            var eventString = eventType.timeInfo.id;
            if (eventString.Contains("[Restore]"))
            {
                eventString = eventString.Remove(0, 9);
                var timeID = eventString.Split('/');
                 var itemExist = timeID.Length == 1 ?  GameManager.Instance.Database.getItem(eventString) : GameManager.Instance.Database.getCreatureItem(timeID[1], timeID[0]);
                if (itemExist != null && eventType.timeInfo.destinyIfHave != -1 && eventType.timeInfo.counterTime >= eventType.timeInfo.destinyIfHave)
                {
                    Database.checkTimeItem(timeID.Length == 2 ? timeID[1] : eventString);
                }
            }
         
        }
        public void claimItem(BaseItemGame item)
        {
            if (typeof(IExtractItem).IsAssignableFrom(item.GetType()))
            {
               var items = ((IExtractItem)item).ExtractHere();
                for (int i = 0; i < items.Length; ++i)
                {
                    Debug.Log("Add " + items[i].item.ItemID + "quantity" + items[i].quantity);
                }
            }
        }
    }
}

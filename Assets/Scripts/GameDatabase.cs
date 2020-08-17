using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Pok
{
    [System.Serializable]
    public class CreatureCollectionTree
    {
        public CreatureItem creatureLeader;
        public MapObject map;
        public I2String className;
        public int amountCreature;
        public AssetReference prefabMix;
    }

    [CreateAssetMenu(fileName = "GameDatabase", menuName = "Pok/GameDatabase", order = 0)]
    public class GameDatabase : SerializedScriptableObject
    {
        static GameDatabase _instance;
        public static GameDatabase Instance
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    if(_instance == null)
                    {
                        _instance = UnityEditor.AssetDatabase.LoadAssetAtPath<GameDatabase>("Assets/Pok/Data/GameDatabase.asset");
                    }
                    return _instance;
                }
                else
                {
                    if(_instance == null)
                    {
                        _instance = AddressableHolder.Instance.FindAsset<GameDatabase>();
                        _instance?.onInit();
                    }
                    return _instance;
                }

#else
                     if(_instance == null)
                    {
                        _instance = AddressableHolder.Instance.FindAsset<GameDatabase>();
                        _instance?.onInit();
                    }
                return _instance;
#endif
            }
        }
        [System.NonSerialized]
        public bool isInit = false;
        public void onInit()
        {
            foreach(var map in MapCollection)
            {
                map.onInit();
            }
            foreach (var shop in ShopCollection)
            {
                shop.onInit();
            }
            CreatureCollection.Find(x => x.ItemID == "Pok1").initRank(0);
            for(int i = 0; i < CreatureCollection.Count; ++i)
            {
                GameManager.addDirtyState("Main");
                CreatureCollection[i].getModelForState((o) =>
                {
                    GameManager.removeDirtyState("Main");
                });
                GameManager.addDirtyState("Main");
                CreatureCollection[i].getSpriteForState((o) =>
                {
                    GameManager.removeDirtyState("Main");
                });
            }
            isInit = true;
        }
        public List<ZoneObject> ZoneCollection = new List<ZoneObject>();
        public List<MapObject> MapCollection = new List<MapObject>();
        public List<BaseItemGame> ItemInventoryCollection = new List<BaseItemGame>();
        public List<CreatureItem> CreatureCollection = new List<CreatureItem>();
        public List<CreatureCollectionTree> treeCreature = new List<CreatureCollectionTree>();
        [InfoBox("Shop")]
        public List<ShopDatabase> ShopCollection = new List<ShopDatabase>();
        [InfoBox("ti le quay thuong 12/12")]
        public List<WheelConfig> wheelMainConfig = new List<WheelConfig>();
        [InfoBox("cac item trong box magic case")]
        public List<ItemMagicCaseConfig> containerMagicCase = new List<ItemMagicCaseConfig>();
        [InfoBox("cac item trong box thuong quang cao")]
        public List<ItemMagicCaseConfig> containerRewardADS = new List<ItemMagicCaseConfig>();
        [InfoBox("Thoi gian xuat hien box magicase")]
        public double timeAFKShowBoxTreasure = 7200;
        [InfoBox("cac nhiem vu")]
        public List<ItemMissionObject> missionContainer = new List<ItemMissionObject>();
        [InfoBox("Thoi gian afk xuat hien box bank")]
        public double timeMinToShowBoxBank = 7200;
        public List<MapObject> getAllMapInZone(string zoneID)
        {
            return MapCollection;
        }
 

    
        public CreatureItem[] getAllCreatureInMap(string mapID)
        {
            var mapObject = MapCollection.Find(x => x.ItemID == mapID);
            if (mapObject)
            {
               return mapObject.childCreatures;
            }
            return new CreatureItem[0];
        }
        public BaseItemGame getItemInventory(string id)
        {
            return ItemInventoryCollection.Find(x => x.ItemID == id);
        }
        public void addItemInventory(BaseItemGame pItem)
        {
            if (ItemInventoryCollection.Exists(x => x == pItem)) return;
            ItemInventoryCollection.Add(pItem);
        }


    }
}


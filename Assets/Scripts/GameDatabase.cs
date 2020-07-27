using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pok
{
    [System.Serializable]
    public class CreatureCollectionTree
    {
        public CreatureItem creatureLeader;
        public MapObject map;
        public I2String className;
        public int amountCreature;
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
            isInit = true;
        }
        public List<ZoneObject> ZoneCollection = new List<ZoneObject>();
        public List<MapObject> MapCollection = new List<MapObject>();
        public List<BaseItemGame> ItemInventoryCollection = new List<BaseItemGame>();
        public List<CreatureItem> CreatureCollection = new List<CreatureItem>();
        public List<CreatureCollectionTree> treeCreature = new List<CreatureCollectionTree>();
        public List<ShopDatabase> ShopCollection = new List<ShopDatabase>();
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


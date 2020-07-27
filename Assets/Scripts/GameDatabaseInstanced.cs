using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pok
{
    public enum BehaviorDatabase { NEWITEM, CHANGE_QUANTITY_ITEM };
    public struct GameDatabaseInventoryEvent
    {
        public BaseItemGameInstanced item;
        public BehaviorDatabase behavior;

        public GameDatabaseInventoryEvent(BehaviorDatabase pBehavior, BaseItemGameInstanced pItem)
        {
            behavior = pBehavior;
            item = pItem;
        }

    }
    public struct AddCreatureEvent
    {
        public CreatureInstanceSaved creature;
        public string zoneid;
        public bool manualByHand;
    }

    [System.Serializable]
    public class TimeModule
    {
        public System.DateTime time;
    }


    [System.Serializable]
    public class CreatureInfoSaved
    {
        public string id;
        public bool isUnLock = false;
        public int level;
        public int boughtNumber = 0;
    }
    [System.Serializable]
    public class ZoneInfoSaved
    {
        public string id;
        public bool isUnLock;
        public Dictionary<string, string> leaderSelected = new Dictionary<string, string>();
        public string curentUnlock;
    }
    [System.Serializable]
    public class ZoneInstanceSaved
    {
        public string id;
        public List<MapInstanceSaved> maps = new List<MapInstanceSaved>();

        public void onInit()
        {
            for (int i = 0; i < maps.Count; ++i)
            {
                maps[i].zoneParent = this;
                maps[i].onInit();
            }
        }
        public List<CreatureInstanceSaved> GetCreaturesInZone(string creature)
        {
            for (int i = 0; i < maps.Count; ++i)
            {
                var creatures = maps[i].creatures.FindAll(x => x.id == creature);
                if (creatures.Count > 0)
                {
                    return creatures;
                }
            }
            return new List<CreatureInstanceSaved>();
        }
        public int GetCreatureInZone(string creature)
        {
            int total = 0;
            for (int i = 0; i < maps.Count; ++i)
            {
                total += maps[i].creatureCount(creature);
            }
            return total;
        }
    }
    [System.Serializable]
    public class MapInstanceSaved
    {
        public string id;
        [System.NonSerialized]
        public ZoneInstanceSaved zoneParent;
        [System.NonSerialized]
        public bool isUnlocked;
        public List<CreatureInstanceSaved> creatures = new List<CreatureInstanceSaved>();
        public void onInit()
        {
            for (int i = 0; i < creatures.Count; ++i)
            {
                creatures[i].mapParent = this;
                creatures[i].onInit();
            }
        }
        public int creatureCount(string id)
        {
            return creatures.Count(x => x.id == id);
        }
    }

    [System.Serializable]
    public class CreatureInstanceSaved
    {
        public string instanceID;
        public string id;
        public void onInit()
        {
        }
        [System.NonSerialized]
        public MapInstanceSaved mapParent;
    }
    [System.Serializable]
    public class PackageCreatureInstanceSaved : CreatureInstanceSaved
    {
        public string creature;
    }
    [System.Serializable]
    public class WorldDataInstanced
    {
        public List<ZoneInstanceSaved> zones = new List<ZoneInstanceSaved>();
        public void onInit()
        {
            for (int i = 0; i < zones.Count; ++i)
            {
                zones[i].onInit();
            }
        }

        public MapInstanceSaved addCreature(CreatureInstanceSaved creature, string address)
        {
            var zone = zones.Find(x => x.id == address);
            if (zone != null)
            {
                var creatureOriginal = GameDatabase.Instance.CreatureCollection.Find(a => a.ItemID == creature.id);
                if (creatureOriginal.categoryItem == CategoryItem.PACKAGE_CREATURE)
                {
                    var creaturePackage = (PackageCreatureInstanceSaved)creature;
                    var creatureInside = GameDatabase.Instance.CreatureCollection.Find(x => x.ItemID == creaturePackage.creature);
                    var map = zone.maps.Find(x => x.id == creatureInside.parentMap.ItemID);
                    creature.mapParent = map;
                    map.creatures.Add(creature);
                    return map;
                }
                else
                {
                    var map = zone.maps.Find(x => x.id == creatureOriginal.parentMap.ItemID);
                    creature.mapParent = map;
                    map.creatures.Add(creature);
                    return map;
                }


            }
            return null;
        }
    }


    [System.Serializable]
    public class GameDatabaseInstanced
    {

        //instance
        public WorldDataInstanced worldData = new WorldDataInstanced();

        public List<BaseItemGameInstanced> inventory = new List<BaseItemGameInstanced>();
        public List<TimeCounterInfo> timeRestore = new List<TimeCounterInfo>();

        //class
        public List<ZoneInfoSaved> zoneInfos = new List<ZoneInfoSaved>();
        public List<CreatureInfoSaved> creatureInfos = new List<CreatureInfoSaved>();


        public void InitDatabase()
        {
            GameManager.Instance.timeCollection.Clear();
            worldData.onInit();
            for (int i = 0; i < timeRestore.Count; ++i)
            {
                timeRestore[i].firstTimeAdd = -timeRestore[i].CounterTime;
                timeRestore[i].counterTime = 0;
                GameManager.Instance.timeCollection.Add(timeRestore[i]);
            }
            for (int i = 0; i < inventory.Count; ++i)
            {
                inventory[i].onInit();
            }


            for (int i = 0; i < GameDatabase.Instance.ZoneCollection.Count; ++i)
            {
                // if (zoneInfos.Count <= i)
                {
                    var zone = zoneInfos.Find(x => x.id == GameDatabase.Instance.ZoneCollection[i].ItemID);
                    if (zone == null)
                    {
                        zoneInfos.Add(zone = new ZoneInfoSaved()
                        {
                            id = GameDatabase.Instance.ZoneCollection[i].ItemID,
                            isUnLock = (i == 0),

                        });
                    }
                    if (!zone.leaderSelected.ContainsKey(GameDatabase.Instance.MapCollection[0].ItemID))
                    {
                        zone.leaderSelected.Add(GameDatabase.Instance.MapCollection[0].ItemID, GameDatabase.Instance.treeCreature.Find(x => x.map == GameDatabase.Instance.MapCollection[0]).creatureLeader.ItemID);
                    }



                    // if (zone.isUnLock)
                    {
                        var zoneInstance = worldData.zones.Find(x => x.id == zone.id);

                        if (zoneInstance == null)
                        {
                            worldData.zones.Add(zoneInstance = new ZoneInstanceSaved() { id = zone.id, maps = new List<MapInstanceSaved>() });
                        }
                        if (zoneInstance != null)
                        {
                            var maps = GameDatabase.Instance.getAllMapInZone(zone.id);
                            for (int j = 0; j < maps.Count; ++j)
                            {
                                var mapExist = zoneInstance.maps.Find(x => x.id == maps[j].ItemID);
                                if (mapExist == null)
                                {
                                    zoneInstance.maps.Add(new MapInstanceSaved()
                                    {
                                        id = maps[j].ItemID,
                                        zoneParent = zoneInstance
                                    });
                                }
                            }
                        }

                    }
                }
            }
            for (int i = 0; i < GameDatabase.Instance.CreatureCollection.Count; ++i)
            {
                if (creatureInfos.Count <= i)
                {
                    creatureInfos.Add(new CreatureInfoSaved()
                    {
                        id = GameDatabase.Instance.CreatureCollection[i].ItemID,
                        isUnLock = (i == 0)
                    });
                }


            }
            for (int i = 0; i < GameDatabase.Instance.ItemInventoryCollection.Count; ++i)
            {
                var item = GameDatabase.Instance.ItemInventoryCollection[i];
                checkTimeItem(item.ItemID);
            }
        }
        public CreatureInfoSaved[] getAllInfoCreatureInAddress(string zoneId, string map)
        {
            var zoneinfo = zoneInfos.Find(x => x.id == zoneId);
            if (zoneinfo.leaderSelected.ContainsKey(map))
            {
                string leader = zoneinfo.leaderSelected[map];
                var tree = GameDatabase.Instance.treeCreature.Find(x => x.creatureLeader.ItemID == leader);
                var listCreature = new List<CreatureItem>();
                tree.creatureLeader.getChild(listCreature, tree.amountCreature);
                var listCreatureResult = new List<CreatureInfoSaved>();
                for (int i = 0; i < listCreature.Count; ++i)
                {
                    listCreatureResult.Add(GameManager.Instance.Database.creatureInfos.Find(x => x != null && x.id == listCreature[i].ItemID));
                }
                return listCreatureResult.ToArray();
            }
            return new CreatureInfoSaved[0];
        }

        public CreatureItem[] getAllCreatureInfoInZone(string zoneID)
        {
            var zone = zoneInfos.Find(x => x.id == zoneID);
            List<CreatureItem> creatures = new List<CreatureItem>();
            bool endFind = false;
            for(int i =0; i < zone.leaderSelected.Count; ++i)
            {
                var leader =   zone.leaderSelected.Values.ElementAt(i);
                var tree = GameDatabase.Instance.treeCreature.Find(x => x.creatureLeader.ItemID == leader);
                var subList = new List<CreatureItem>();
                tree.creatureLeader.getChild(subList, tree.amountCreature);
                foreach(var element in subList)
                {
                    if (!creatures.Exists(x => x.ItemID == element.ItemID)) 
                    {
                        creatures.Add(element);
                    }
                    if(element.ItemID == zone.curentUnlock)
                    {
                        endFind = true;
                        break;
                    }
                }
                if (endFind)
                {
                    break;
                }
            }
            return creatures.ToArray();
        }

        public List<CreatureInstanceSaved> getAllCreatureInstanceInZone(string zoneID)
        {
            var maps = GameManager.Instance.Database.worldData.zones.Find(x => x.id == zoneID).maps;
            var listCreatures = new List<CreatureInstanceSaved>();
            foreach(var map in maps)
            {
                listCreatures.addFromList(map.creatures.ToArray());
            }
            return listCreatures;
        }
        public List<MapInstanceSaved> getAllMapInZone(string zoneID)
        {
            return GameManager.Instance.Database.worldData.zones.Find(x => x.id == zoneID).maps;
        }
        public List<MapInstanceSaved> getAllMapActiveInZone(string zoneID)
        {
            var zone = GameManager.Instance.Database.worldData.zones.Find(x => x.id == zoneID);
            var zoneInfos = GameManager.Instance.Database.zoneInfos.Find(x => x.id == zoneID);
            List<MapInstanceSaved> maps = new List<MapInstanceSaved>();
            for (int i = 0; i < zone.maps.Count; ++i)
            {
                if (zoneInfos.leaderSelected.ContainsKey(zone.maps[i].id))
                {
                    maps.Add(zone.maps[i]);
                }
            }
            return maps;
        }

        public void checkTimeItem(string itemID)
        {
            var item = GameDatabase.Instance.getItemInventory(itemID);
            if (item.categoryItem == CategoryItem.COMMON)
            {
                var itemExist = getItem(item.ItemID);
                executeTime(itemExist);
            }
            else if (item.categoryItem == CategoryItem.CREATURE || item.categoryItem == CategoryItem.PACKAGE_CREATURE)
            {
                for (int i = 0; i < zoneInfos.Count; ++i)
                {
                    if (zoneInfos[i].isUnLock)
                    {
                        var itemExist = getCreatureItem(item.ItemID, zoneInfos[i].id);
                        executeTime(itemExist, zoneInfos[i].id);
                    }
                }

            }
        }
        public void executeTime(BaseItemGameInstanced itemExist, string adress = "")
        {
            var item = itemExist.item;
            if (itemExist != null)
            {
                if ((item.attribute & AttributeItem.RestoreAble) == AttributeItem.RestoreAble)
                {
                    bool restore = true;
                    if ((item.attribute & AttributeItem.Limit) == AttributeItem.Limit)
                    {
                        if (itemExist.Quantity >= item.limitInInventory.getUnit(itemExist.CurrentLevel))
                        {
                            restore = false;
                        }
                    }
                    if (restore)
                    {
                        var existTime = GameManager.Instance.Database.timeRestore.Exists(x => x.id == "[Restore]" + adress + (string.IsNullOrEmpty(adress) ? "" : "/") + item.ItemID);

                        if (!existTime)
                        {
                                var timer = new TimeCounterInfo() { firstTimeAdd = TimeCounter.CounterValue, counterTime = 0, id = "[Restore]" + adress + (string.IsNullOrEmpty(adress) ? "" : "/") + item.ItemID, destinyIfHave = (double)itemExist.item.timeToRestore.getUnit(itemExist.CurrentLevel) };
                                GameManager.Instance.Database.timeRestore.Add(timer);
                                GameManager.Instance.timeCollection.Add(timer);
                        }
                        else
                        {
                            var time = GameManager.Instance.Database.timeRestore.Find(x => x.id == "[Restore]" + adress + (string.IsNullOrEmpty(adress) ? "" : "/") + item.ItemID);
                            if (time != null)
                            {
                                time.destinyIfHave = (double)itemExist.item.timeToRestore.getUnit(itemExist.CurrentLevel);
                            }
                            bool continueRestore = true;
                            var creautres = getCreatureExist(itemExist.item.ItemID, adress);
                            var map = ((CreatureItem)item).parentMap;
                            if (item.isLimit && itemExist.Quantity < item.limitInInventory.getUnit(itemExist.CurrentLevel))
                            {
                                if (map.limitSlot > creatureInfos.Count)
                                {
                                    continueRestore = true;
                                }
                            }
                            if (continueRestore)
                            {
                                int quantity = (int)(time.CounterTime / item.timeToRestore.getUnit(itemExist.CurrentLevel));
                                if (quantity > 0)
                                {
                                    time.firstTimeAdd = TimeCounter.CounterValue;
                                    itemExist.Quantity += quantity;
                                }
                            }
                            else
                            {
                                GameManager.Instance.Database.timeRestore.Remove(time);
                            }
                        }
                    }
                }
            }
        }
        public List<CreatureInstanceSaved> getCreatureExist(string idCreature, string zoneID)
        {
            for (int i = 0; i < worldData.zones.Count; ++i)
            {
                if (worldData.zones[i].id == zoneID)
                {
                    return worldData.zones[i].GetCreaturesInZone(idCreature);
                }

            }
            return new List<CreatureInstanceSaved>();
        }
        public int getQuantityCreatureExist(string idCreature, string zoneID)
        {
            for (int i = 0; i < worldData.zones.Count; ++i)
            {
                if (worldData.zones[i].id == zoneID)
                {
                    return worldData.zones[i].GetCreatureInZone(idCreature);
                }

            }
            return 0;
        }

        public void addItem(string itemID, long quantity, string address)
        {
            if (string.IsNullOrEmpty(address)) return;
            var pItemOriginal = GameDatabase.Instance.getItemInventory(itemID);
            if (pItemOriginal)
            {
                CreatureInstanceSaved creatureNew = null;// new CreatureInstanceSaved() { instanceID = GameManager.Instance.GenerateID.ToString(), id = itemID };
                if (pItemOriginal.categoryItem == CategoryItem.PACKAGE_CREATURE)
                {
                    int levelCreature = creatureInfos.Find(x => x.id == itemID).level;
                    var package = (PackageCreatureObject)pItemOriginal;
                    if (package.creatureExtra.Length > 0)
                    {
                        var creatureFind = System.Array.Find(package.creatureExtra, x => x.levelRequire == levelCreature);
                        if (creatureFind != null)
                        {
                            creatureNew = new PackageCreatureInstanceSaved() { creature = creatureFind.unit.ItemID, instanceID = GameManager.Instance.GenerateID.ToString(), id = itemID };
                            GameManager.Instance.GenerateID++;
                        }

                    }
                }
                if (creatureNew != null)
                {
                    worldData.addCreature(creatureNew, address);
                    EazyEngine.Tools.EzEventManager.TriggerEvent(new AddCreatureEvent() { creature = creatureNew, zoneid = address,manualByHand= false });
                }
            }
        }

        public BaseItemGameInstanced getCreatureItem(string itemID, string zone)
        {
            var pItemOriginal = (BaseItemGame)GameDatabase.Instance.CreatureCollection.Find(x => x.ItemID == itemID);
            pItemOriginal = pItemOriginal != null ? pItemOriginal : GameDatabase.Instance.getItemInventory(itemID);
            if (pItemOriginal)
            {
                if (pItemOriginal.categoryItem == CategoryItem.CREATURE || pItemOriginal.categoryItem == CategoryItem.PACKAGE_CREATURE)
                {
                    var itemnew = new BaseItemGameInstanced() { quantity = getQuantityCreatureExist(pItemOriginal.ItemID, zone), address = $"{zone}", itemID = pItemOriginal.ItemID, item = pItemOriginal };
                    return itemnew;
                }
            }
            return null;
        }
        public BaseItemGameInstanced getItem(string itemID)
        {
            var pItemOriginal = GameDatabase.Instance.getItemInventory(itemID);
            if (pItemOriginal)
            {
                if (pItemOriginal.categoryItem == CategoryItem.COMMON)
                {
                    var itemExist = inventory.Find(x => x.item.ItemID == itemID);
                    if (itemExist == null)
                    {
                        var item = new BaseItemGameInstanced() { item = pItemOriginal, quantity = 0, itemID = pItemOriginal.itemID };
                        inventory.Add(item);
                        return item;
                    }
                    return itemExist;
                }
            }
            return null;
        }
    }
}

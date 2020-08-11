using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;
using System.Numerics;

namespace Pok
{
    public class BoxShopCreature : BaseBox<BoxShopCreature, ItemShopUI, ShopItemInfo>
    {
        public UIElement container;
        public void show()
        {
            //  executeInfos(GameDatabase.Instance.ZoneCollection.ToArray());
            reloadData();
            _scrollView?.ResetPosition();
            container.show();
        }

        public void reloadData()
        {
            var zone = GameDatabase.Instance.ZoneCollection.Find(x => x.ItemID == GameManager.Instance.ZoneChoosed);
            var creatureActive = GameManager.Instance.Database.getAllCreatureInfoInZone(zone.ItemID);
            List<CreatureItem> tempList = new List<CreatureItem>(creatureActive);
            int removeIndex = 0;
            for(int i = tempList.Count-1 ; i >= 0; --i)
            {
                if(i == 0)
                {
                    break;
                }
                if(removeIndex == 4)
                {
                    break;
                }
                var element = tempList[i];
                tempList.RemoveAt(i);
                if (GameManager.Instance.Database.creatureInfos.Find(x => x.id == element.ItemID).isUnLock)
                {
                    removeIndex++;
                }
            }
            creatureActive = tempList.ToArray();
            var currentShop = zone.shopCreature;
            List<ShopItemInfo> shopElements = new List<ShopItemInfo>();
            for (int i = 0; i < creatureActive.Length; ++i)
            {
                shopElements.Add(System.Array.Find(currentShop.items, x => x.itemSell == creatureActive[i]));
            }
            executeInfos(shopElements.ToArray());
           for(int i  = 0; i < items.Count; ++i)
            {
                items[i].showBtnAds(i == items.Count - 1);
            }
        }

        public void buyWay1(object data)
        {
            buy(data, 0);
        }
        public void buyWay2(object data)
        {
            buy(data, 1);
        }
        public void buyWay3(object data)
        {
            if (GameManager.Instance.isRewardADSReady("BuyCreature"))
            {
                GameManager.Instance.WatchRewardADS("BuyCreature", (o) =>
                {
                    if (o)
                    {
                        claim(data);
                    }
                });
            }
        }
        public void claim(object data)
        {
            var creature = ((CreatureItem)((ShopItemInfo)data).itemSell);
            var zone = GameDatabase.Instance.ZoneCollection.Find(x => x.ItemID == GameManager.Instance.ZoneChoosed);
            var pack = zone.getPackage();
            var mapParent = GameManager.Instance.Database.worldData.zones.Find(x => x.id == GameManager.Instance.ZoneChoosed).maps.Find(x => creature.parentMap && x.id == creature.parentMap.ItemID);
            var exists = GameManager.Instance.Database.getAllCreatureInstanceInAdress(GameManager.Instance.ZoneChoosed, mapParent.id);
            var newCreature = new PackageCreatureInstanceSaved() { creature = creature.ItemID, id = pack.package.ItemID, instanceID = GameManager.Instance.GenerateID.ToString(), mapParent = mapParent };
            if (exists.Count >= 16)
            {
                HUDManager.Instance.showBoxFullSlot();
            }
            var creatures = GameManager.Instance.Database.getAllInfoCreatureInAddress(GameManager.Instance.ZoneChoosed, mapParent.id);
            System.Array.Find(creatures, x => x.id == creature.ItemID).boughtNumber++;
            GameManager.Instance.GenerateID++;
            GameManager.Instance.Database.worldData.addCreature(newCreature, GameManager.Instance.ZoneChoosed);
            EzEventManager.TriggerEvent(new AddCreatureEvent() { change = 1, creature = newCreature, zoneid = GameManager.Instance.ZoneChoosed, manualByHand = false });
            reloadData();
        }
        public void buy(object data,int index)
        {
            var shopitem = ((ShopItemInfo)data);
            var exchanges = shopitem.getCurrentPrice()[index].exchangeItems;
            for (int i = 0; i < exchanges.Length; ++i)
            {
                var exist = GameManager.Instance.Database.getItem(exchanges[i].item.ItemID);
                if (exist.QuantityBig < BigInteger.Parse(exchanges[i].quantity.clearDot()))
                {
                    HUDManager.Instance.showBoxNotEnough(exist.item);
                    return;
                }
            }
            for (int i = 0; i < exchanges.Length; ++i)
            {
                var exist = GameManager.Instance.Database.getItem(exchanges[i].item.ItemID);
                exist.addQuantity("-" + exchanges[i].quantity.clearDot());
            }
            claim(data);
        }
    }
}

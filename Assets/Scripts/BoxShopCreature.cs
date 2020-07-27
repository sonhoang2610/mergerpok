using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Pok
{
    public class BoxShopCreature : BaseBox<BoxShopCreature,ItemShopUI,ShopItemInfo>
    {
        public UIElement container;
        public void show()
        {
            //  executeInfos(GameDatabase.Instance.ZoneCollection.ToArray());
            var zone = GameDatabase.Instance.ZoneCollection.Find(x => x.ItemID == GameManager.Instance.ZoneChoosed);
            var creatureActive = GameManager.Instance.Database.getAllCreatureInfoInZone(zone.ItemID);
            var currentShop = zone.shopCreature;
            List<ShopItemInfo> shopElements = new List<ShopItemInfo>();
            for(int i = 0; i < creatureActive.Length; ++i)
            {
                shopElements.Add(System.Array.Find( currentShop.items,x=>x.itemSell== creatureActive[i]));
            }
            executeInfos(shopElements.ToArray());
            container.show();
        }

        public void buyWay1(object data)
        {
            Debug.Log("buy Way 1" + ((ShopItemInfo)data).itemSell.ItemID);
        }
        public void buyWay2(object data)
        {
            Debug.Log("buy Way 2" + ((ShopItemInfo)data).itemSell.ItemID);
        }

    }
}

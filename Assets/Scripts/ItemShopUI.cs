using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;
using System.Globalization;
using System.Numerics;

namespace Pok
{
    public class ItemShopUI : BaseItem<ShopItemInfo>
    {
        public UI2DSprite icon;
        public UILabel namelbl, infoGold, numberBought;
        public UILabel[] price;
        public UI2DSprite[] iconExchange;

        public override void setInfo(ShopItemInfo pInfo)
        {
            base.setInfo(pInfo);
            pInfo.itemSell.getSpriteForState((o) =>
            {
                icon.sprite2D = o;
                icon.MakePixelPerfectClaimIn(new Vector2Int(icon.width, icon.height));
            });
            namelbl.text = pInfo.itemSell.displayNameItem.Value;
            infoGold.text = $"gold { ((CreatureItem)pInfo.itemSell).goldAFKReward[GameManager.Instance.ZoneChoosed].ToKMBTA()}/sec";
            numberBought.text = $"Bought: {GameManager.Instance.getNumberBoughtItem(pInfo.itemSell.ItemID)}";
            pInfo.itemSell.getSpriteForState((o) =>
            {
                icon.sprite2D = o;
                icon.MakePixelPerfectClaimIn(new Vector2Int(icon.width, icon.height));
            });
            var payments = pInfo.getCurrentPrice(1);
            for(int i = 0; i < payments.Length; ++i)
            {
                price[i].text = payments[i].exchangeItems[0].quantity.ToKMBTA();
                var itemChange = iconExchange[i];
                payments[i].exchangeItems[0].item.getSpriteForState((o) => {
                    itemChange.sprite2D = o;
                    itemChange.MakePixelPerfectClaimIn(new Vector2Int(itemChange.width, itemChange.height));
                });

            }
        }
    }
}

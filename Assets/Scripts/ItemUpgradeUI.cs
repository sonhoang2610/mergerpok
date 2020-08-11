using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok {
    public class ItemUpgradeUI : SimpleItemShop
    {
        public UILabel content;
        public UIButton btnBuy;
        public override void setInfo(ShopItemInfo pInfo)
        {
            base.setInfo(pInfo);
            var itemExist = GameManager.Instance.getLevelItem(pInfo.itemSell.ItemID);
            btnBuy.gameObject.SetActive( !(pInfo.limitUpgrade != -1 && itemExist >= pInfo.limitUpgrade));
            content.text = pInfo.itemSell.GetUpgradeString();
        }
    }
}

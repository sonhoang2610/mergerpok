using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Pok
{
    public class PackageItemShop : SimpleItemShop
    {
        public UILabel[] childContents;
        public UI2DSprite[] childIcons;

        public override void setInfo(ShopItemInfo pInfo)
        {
            base.setInfo(pInfo);
            var extras = ((IExtractItem)pInfo.itemSell).ExtractHere();
            for (int i = 0; i < extras.Length; ++i)
            {
                var label = childContents[i];
                var sprite = childIcons[i];
                extras[i].item.getSpriteForState((o) =>
                {
                    sprite.sprite2D = o;
                    //sprite.MakePixelPerfectClaimIn(new Vector2Int(sprite.width, sprite.height));
                });
                label.text = (System.Numerics.BigInteger.Parse( extras[i].quantity.clearDot())*(int)((1 +pInfo.bonusForItem(pInfo)) *100)/100).ToString() .ToKMBTA();
            }
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{
    public class BoxMultiplyBonus : MonoBehaviour
    {
        public UI2DSprite[] pokIcon;
        public GameObject onetimeOffer, boxTime;
        public UILabel price;
        public UILabel namePo;

        protected Vector2Int cacheSizeIcon;
        private void Awake()
        {
            cacheSizeIcon = new Vector2Int(pokIcon[0].width, pokIcon[0].height);
        }
        public void showData(ShopItemInfo info)
        {
         
            var random= UnityEngine.Random.Range(0, 2);
            onetimeOffer.gameObject.SetActive(random == 0);
            boxTime.gameObject.SetActive(!(random == 0));
            if(random != 0)
            {
                TimeCounter.Instance.addTimer(new TimeCounterInfo() { autoRemoveIfToDestiny = true, id = $"[MultiplyBonus]{info.itemSell.ItemID}", destinyIfHave = 10800 });

            }
            info.itemSell.getSpriteForState((o) =>
            {
                foreach (var icon in pokIcon)
                {
                    icon.sprite2D = o;
                    icon.MakePixelPerfectClaimIn(cacheSizeIcon);
                }
            });
            var localize = EasyMobile.InAppPurchasing.GetProductLocalizedData(EasyMobile.InAppPurchasing.GetIAPProductById(info.itemSell.ItemID).Name);
            price.text = localize != null ? localize .localizedPriceString : "";
            namePo.text = info.itemSell.displayNameItem.Value;
        }
    }
}

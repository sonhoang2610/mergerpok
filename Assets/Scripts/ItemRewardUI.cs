using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Pok
{
    public class ItemRewardInfo
    {
        public ItemWithQuantity itemReward;
        public AssetReferenceSprite iconOverride;
    }
    public class ItemRewardUI : BaseItem<ItemRewardInfo>
    {
        public UI2DSprite icon;
        public UILabel quantity;

        protected Vector2Int cacheSizeIcon;
        private void Awake()
        {
            cacheSizeIcon = new Vector2Int(icon.width, icon.height);
        }

        public override void setInfo(ItemRewardInfo pInfo)
        {
            base.setInfo(pInfo);
            System.Action<Sprite> action = (o) => {
                icon.sprite2D = o;
                icon.MakePixelPerfectClaimIn(cacheSizeIcon);
            };
            if (pInfo.iconOverride != null)
            {
                pInfo.iconOverride.loadAssetWrapped(action);
            }
            else
            {
                pInfo.itemReward.item.getSpriteForState(action);
            }
            quantity.text = pInfo.itemReward.quantity.ToKMBTA();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{
    public class ItemPackRewardADSInfo
    {
        public BaseItemGame item;
        public BaseItemGame itemClaim;
        public string content;
        public string quantity;
    }
    public class ItemPackRewardADS : BaseItem<ItemPackRewardADSInfo>
    {
        public UI2DSprite icon;
        public UILabel content;

        public Color colorChoose, ColorUnchoose;
        public override void setInfo(ItemPackRewardADSInfo pInfo)
        {
            base.setInfo(pInfo);
            var state = "Default";
            if (pInfo.item.ItemID.Contains("SuperInCome"))
            {
                if (GameManager.Instance.getFactorIncome().x >= 2)
                {
                    state = "X4";
                }
            }
            pInfo.item.getSpriteForState((o) =>
            {
                icon.sprite2D = o;
                icon.MakePixelPerfectClaimIn(new Vector2Int(100, 100));
            }, state);
            content.text = pInfo.content;
        }
        public void choose(bool pBool)
        {
            icon.color = pBool ? colorChoose : ColorUnchoose;
            GetComponent<UIWidget>().color = pBool ? colorChoose : ColorUnchoose;
            //content.color = pBool ? colorChoose : ColorUnchoose;
        }
    }
}

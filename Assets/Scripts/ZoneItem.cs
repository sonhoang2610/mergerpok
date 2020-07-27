using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Pok
{
    public class ZoneItem : BaseItem<ZoneObject>
    {
        public UILabel nameZone;
        public UI2DSprite icon;


        public override void setInfo(ZoneObject pInfo)
        {
            base.setInfo(pInfo);
            var state = "Default";
            var zoneinfo = GameManager.Instance.Database.zoneInfos.Find(x => x.id == pInfo.ItemID);
            if (!zoneinfo.isUnLock)
            {
                state = "DefaultDisable";
            }
            pInfo.getSpriteForState((o) =>
            {
                icon.sprite2D = o;
                icon.MakePixelPerfectClaimIn(new Vector2Int(icon.width, icon.height));
            }, state);
            nameZone.text = pInfo.displayNameItem.Value;


        }
    }
}

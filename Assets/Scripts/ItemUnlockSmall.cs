using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{
    public class CreatureInfoSatus
    {
        public CreatureInfoSaved mainInfo;
        public bool current;
        public bool isUnlock = false;
    }
    public class ItemUnlockSmall : BaseItem<CreatureInfoSatus>
    {
        public UI2DSprite icon;
        public UI2DSprite statusbar;
        public Vector2Int claim;
        public Sprite colorBarPrevious, colorBarCurrent, ColorBarlocked;
        public override void setInfo(CreatureInfoSatus pInfo)
        {
            base.setInfo(pInfo);
            statusbar.sprite2D = pInfo.current ? colorBarCurrent : (pInfo.isUnlock ? colorBarPrevious : ColorBarlocked);
            var originalItem = GameDatabase.Instance.CreatureCollection.Find(x => x.ItemID == pInfo.mainInfo.id);
            originalItem.getSpriteForState((o) => {
                icon.sprite2D = o;
                icon.MakePixelPerfectClaimIn(claim);
            });
            if (!pInfo.isUnlock)
            {
                icon.color = Color.black;
            }
            else
            {
                icon.color = Color.white;
            }
        }
    }
}

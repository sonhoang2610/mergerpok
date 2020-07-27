using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{
    public class ItemSelectEra : BaseItem<CreatureItem>
    {
        public UI2DSprite icon;
        public Vector2Int claim;

        public override void setInfo(CreatureItem pInfo)
        {
            base.setInfo(pInfo);
            pInfo.getSpriteForState((o) =>
            {
                icon.sprite2D = o;
                icon.MakePixelPerfectClaimIn(claim);
            });
        }
    }
}
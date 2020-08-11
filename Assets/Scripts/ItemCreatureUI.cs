using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{
    public class ItemCreatureUI : BaseItem<CreatureItem>
    {
        public UI2DSprite icon;
        public UILabel namePok;

        protected Vector2Int cacheSize;
        private void Awake()
        {
            cacheSize = new Vector2Int(icon.width, icon.height);
        }
        public override void setInfo(CreatureItem pInfo)
        {
            base.setInfo(pInfo);
            pInfo.getSpriteForState((o) =>
            {
                if (o && icon)
                {
                    icon.sprite2D = o;
                    icon.MakePixelPerfectClaimIn(cacheSize);
                }
            });
            namePok.text = pInfo.displayNameItem.Value;
        }
    }
}
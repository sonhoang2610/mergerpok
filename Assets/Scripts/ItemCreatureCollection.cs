using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;

namespace Pok
{
    public class ItemCreatureCollection : BaseItem<CreatureItem>
    {
        public UILabel label;
        public UI2DSprite sprite;
        public float claimHeight = 0;
        public override void setInfo(CreatureItem pInfo)
        {
            base.setInfo(pInfo);
            pInfo.getSpriteForState((o) =>
            {
                sprite.sprite2D = o;
                var cacheScale = sprite.transform.localScale;
                sprite.transform.localScale = new Vector3(1, 1, 1);
                sprite.MakePixelPerfect();
                float factor = sprite.height / claimHeight;
                sprite.height  = (int)claimHeight;
                sprite.width = (int)( sprite.width/factor);
                sprite.transform.localScale = cacheScale;
            });
            label.text = pInfo.displayNameItem.Value;
        }
    }
}

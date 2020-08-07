using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{
    public class ItemPackageContainUI : BaseItem<BaseItemGame>
    {
        public UI2DSprite icon;
        public UILabel content;
        public override void setInfo(BaseItemGame pInfo)
        {
            base.setInfo(pInfo);
            pInfo.getSpriteForState((o) => {
                if (icon)
                {
                    icon.sprite2D = o;
                    icon.MakePixelPerfectClaimIn(new Vector2Int(icon.width, icon.height));
                }
            }, "Content");
            content.text = pInfo.getContent();
        }

     
    }
}

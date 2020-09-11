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
        protected UIWidget widgetParent;
        private void Awake()
        {
            cacheSize = new Vector2Int(icon.width, icon.height);
            widgetParent = gameObject.GetComponent<UIWidget>();
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
        private void Update()
        {
            if (widgetParent)
            {
                if(icon.depth <= widgetParent.depth)
                {
                    icon.depth = widgetParent.depth + 1;
                }
                if (namePok.depth <= widgetParent.depth)
                {
                    namePok.depth = widgetParent.depth + 1;
                }
            }
        }
    }
}
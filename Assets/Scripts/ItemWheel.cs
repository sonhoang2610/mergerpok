using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{
    public class ItemWheel : MonoBehaviour , IElementReset
    {
        public UI2DSprite skin;
        bool isFixSkin = false;
        protected WheelConfig[] cacheInfos;

        protected Vector2Int cacheSize;
        private void Awake()
        {
            cacheSize = new Vector2Int(skin.width, skin.height);
        }

        public void setInfos(WheelConfig[] infos)
        {
            cacheInfos = infos;
            randomSkin();
        }

        public void randomSkin()
        {
            if (!isFixSkin)
            {
                int index = Random.Range(0, cacheInfos.Length);
                cacheInfos[index].getICon().loadAssetWrapped((o) =>
                {
                    skin.sprite2D = o;
                    skin.MakePixelPerfectClaimIn(cacheSize);
                });
            }
            else
            {
                cacheFix.getICon().loadAssetWrapped((o) =>
                {
                    skin.sprite2D = o;
                    skin.MakePixelPerfectClaimIn(cacheSize);
                });
            }
        }
        protected WheelConfig cacheFix;
        public void fixSkin(WheelConfig info)
        {
            isFixSkin = true;
            cacheFix = info;
          
        }

        public void resetToBegin()
        {
            isFixSkin = false;
        }
    }
}

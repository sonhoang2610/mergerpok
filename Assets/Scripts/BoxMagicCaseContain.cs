using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{
    public class BoxMagicCaseContain : BaseNormalBox<ItemPackageContainUI,BaseItemGame>
    {

        public UIButton btnOpen;
        private void OnEnable()
        {
            var ListItem = new List<BaseItemGame>();
            for(int i = 0; i < GameDatabase.Instance.containerMagicCase.Count; ++i)
            {
                ListItem.Add(GameDatabase.Instance.containerMagicCase[i].item);
            }
            executeInfos(ListItem.ToArray());
            btnOpen.isEnabled = GameManager.Instance.isRewardADSReady("MagicCase");
            if (!btnOpen.isEnabled)
            {
                loadAds();
            }
        }
        public void loadAds()
        {
            GameManager.Instance.LoadRewardADS("MagicCase", (o) => {
                if (!gameObject.activeSelf) return;
                if (o)
                {
                    btnOpen.isEnabled = true;
                }
                else
                {
                    btnOpen.isEnabled = false;
                    if (gameObject.activeSelf)
                    {
                        loadAds();
                    }
                }
            });
        }

        public void showBoxMagicCase()
        {
            HUDManager.Instance.boxMagicCaseContain.close();
            GameManager.Instance.WatchRewardADS("MagicCase", (o) =>
            {
                if (o)
                {
                    HUDManager.Instance.boxMagicCase.show();
                }
            });
        }
    }
}

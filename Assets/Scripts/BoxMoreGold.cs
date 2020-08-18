using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Pok
{
    public class BoxMoreGold : MonoBehaviour
    {
        public UILabel factor1, factor2,time1,time2;
        public UIButton btnGet;
        public float factor = 1;
        private void OnEnable()
        {
  
            factor = (GameManager.Instance.getFactorIncome().x < 2) ? 2 : 4;
            time1.text = factor == 2 ? "3H 0M" : "3M 0S";
            time2.text = factor == 2 ? "For 3 Hours!" : "For 3 Minutes!";
            factor1.text = string.Format("earn x{0} gold!", factor);
            factor2.text = string.Format("x{0}", factor);
            btnGet.isEnabled = GameManager.Instance.isRewardADSReady("MoreGold");
            if (!btnGet.isEnabled)
            {
                loadAds();
            }
        }

        public void loadAds()
        {
            GameManager.Instance.LoadRewardADS("MoreGold", (o) => {
                if (!gameObject.activeSelf) return;
                if (o)
                {
                    btnGet.isEnabled = true;
                }
                else
                {
                    btnGet.isEnabled = false;
                    if (gameObject.activeSelf)
                    {
                        loadAds();
                    }
                }
            });
        }

        public void getGold()
        {
            GetComponent<UIElement>().close();
            GameManager.Instance.WatchRewardADS("MoreGold", (o) =>
            {
                if (o)
                {
                    GameManager.Instance.addFactorSuperIncome(factor, 180*(factor == 2 ? 60 : 1));
                }
            });
        }
    }
}

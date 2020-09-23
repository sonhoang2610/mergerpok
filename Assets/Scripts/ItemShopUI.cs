using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;
using System.Globalization;
using System.Numerics;

namespace Pok
{
    public class ItemShopUI : BaseItem<ShopItemInfo>
    {
        public UI2DSprite icon;
        public UILabel namelbl, infoGold, numberBought;
        public UILabel[] price;
        public UI2DSprite[] iconExchange;
        public GameObject btnAds;
        public override void setInfo(ShopItemInfo pInfo)
        {
            base.setInfo(pInfo);
            pInfo.itemSell.getSpriteForState((o) =>
            {
                icon.sprite2D = o;
                icon.MakePixelPerfectClaimIn(new Vector2Int(icon.width, icon.height));
            });
            namelbl.text = pInfo.itemSell.displayNameItem.Value;
            infoGold.text = $"gold { ((CreatureItem)pInfo.itemSell).getGoldAFK(GameManager.Instance.ZoneChoosed).ToKMBTA()}/sec";
            numberBought.text = $"Bought: {GameManager.Instance.getNumberBoughtItem(pInfo.itemSell.ItemID)}";
            pInfo.itemSell.getSpriteForState((o) =>
            {
                icon.sprite2D = o;
                icon.MakePixelPerfectClaimIn(new Vector2Int(icon.width, icon.height));
            });
            var payments = pInfo.getCurrentPrice();
            for(int i = 0; i < payments.Length; ++i)
            {
                var numberBought = GameManager.Instance.Database.creatureInfos.Find(x => x.id == pInfo.itemSell.ItemID).BoughtNumberVariant[GameManager.Instance.ZoneChoosed][i];
                price[i].text = PokUltis.calculateCreaturePrice(i, numberBought, (CreatureItem)pInfo.itemSell).ToKMBTA();
                var itemChange = iconExchange[i];
                payments[i].exchangeItems[0].item.getSpriteForState((o) => {
                    itemChange.sprite2D = o;
                    itemChange.MakePixelPerfectClaimIn(new Vector2Int(itemChange.width, itemChange.height));
                });

            }
        }
        protected Coroutine checkPanel;
        bool activeAds = false;
        public void showBtnAds(bool pBool)
        {
            activeAds = pBool;
            if (GameManager.InstanceRaw && GameManager.Instance.TimeCountDownAdsCreature <= 0)
            {
                btnAds.gameObject.SetActive(activeAds);
                if (activeAds)
                {
                    if (checkPanel != null)
                    {
                        StopCoroutine(checkPanel);
                        checkPanel = null;
                    }
                    checkPanel = StartCoroutine(checkPanelInit(btnAds.gameObject));
                }
            }
        }
        private void Update()
        {
            if(GameManager.InstanceRaw && GameManager.Instance.TimeCountDownAdsCreature <= 0 )
            {
                btnAds.gameObject.SetActive(activeAds);
                if (activeAds)
                {
                    if (checkPanel != null)
                    {
                        StopCoroutine(checkPanel);
                        checkPanel = null;
                    }
                    checkPanel = StartCoroutine(checkPanelInit(btnAds.gameObject));
                }
            }
        }
        public IEnumerator checkPanelInit(GameObject widget)
        {
            int index = 0;
            bool loop = false;
            do
            {
                loop = false;
                var widgets = widget.GetComponentsInChildren<UIWidget>();
                for (int i = 0; i < widgets.Length; ++i)
                {
                    if (!widgets[i].panel)
                    {
                        loop = true;
                    }
                }
                yield return new WaitForEndOfFrame();
                index++;
            } while (loop && index< 100);
            NGUITools.BringForward(widget);
            checkPanel = null;
        }
    }
}

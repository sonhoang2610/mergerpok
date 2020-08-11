using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{
    public class SimpleItemShop : BaseItemShop
    {
        public PaymentUI[] exchanges;
        public GameObject attachMent;

        public override void setInfo(ShopItemInfo pInfo)
        {
            base.setInfo(pInfo);
            var paymens = pInfo.getCurrentPrice();
            for (int i = 0; i < exchanges.Length; ++i)
            {
                if (i >= paymens.Length)
                {
                    exchanges[i].container.gameObject.SetActive(false);
                }
                else
                {
                    exchanges[i].container.gameObject.SetActive(true);
                    var exchange = exchanges[i];
                    if (paymens[i].exchangeItems[0].Normal)
                    {
                        paymens[i].exchangeItems[0].item.getSpriteForState((o => { exchange.icon.sprite2D = o; exchange.icon.MakePixelPerfectClaimIn(new Vector2Int(exchange.icon.width, exchange.icon.height)); }));
                        int discount = pInfo.discountEvent == null ? 100 : (int)( (1 - pInfo.discountEvent(pInfo, i, 0)) * 100);
                        exchange.price.text = (System.Numerics.BigInteger.Parse( paymens[i].exchangeItems[0].quantity.clearDot())*discount/100).ToString().ToKMBTA();
                    }
                    else if(paymens[i].exchangeItems[0].IAP)
                    {
                        exchange.price.text = paymens[i].exchangeItems[0].quantity + "$";
                        exchange.icon.sprite2D = null;
                    }
                    else if (paymens[i].exchangeItems[0].WATCH_ADS)
                    {
                        exchange.price.text = "Watch";
                        exchange.icon.sprite2D = null;
                    }

                }
            }
            var exist= GameManager.Instance.Database.getItem(pInfo.itemSell.ItemID);
            if(exist != null && exist.item.limitInInventory != null && exist .QuantityBig>= exist.item.limitInInventory.getCurrentUnit())
            {
                for (int i = 0; i < exchanges.Length; ++i)
                {
                    exchanges[i].container.gameObject.SetActive(false);
                }
            }
        }
    }
}

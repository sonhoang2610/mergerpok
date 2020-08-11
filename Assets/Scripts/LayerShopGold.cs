using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace Pok
{
    [System.Serializable]
    public class PaymentUI
    {
        public GameObject container;
        public UI2DSprite icon;
        public UILabel price;
    }
    public class BaseItemShop : BaseItem<ShopItemInfo>
    {
        public bool independence = false;
        [ShowIf("independence")]
        public string nameShop;
        [ShowIf("independence")]
        public string itemName;

        public UI2DSprite icon;
        public UILabel nameItem,des,bonuslabel;

        public void buySelfWayOne()
        {
            var payments = _info.getCurrentPrice();
            for (int i = 0; i < payments[0].exchangeItems.Length; ++i)
            {
                if (payments[0].exchangeItems[i].IAP)
                {
                    GameManager.Instance.RequestInappForItem(_info.itemSell.ItemID.ToLower(), (o) => {
                        if (o)
                        {
                            var itemclaimeds = GameManager.Instance.claimItem(_info.itemSell);
                            List<ItemRewardInfo> itemRewards = new List<ItemRewardInfo>();
                            for (int g = 0; g < itemclaimeds.Length; ++g)
                            {
                                itemRewards.Add(new ItemRewardInfo() { itemReward = itemclaimeds[g] });
                            }
                            HUDManager.Instance.boxReward.show(itemRewards.ToArray(), "BUY SUCCESS");
                        }
                    });
                    return;
                }else if (payments[0].exchangeItems[i].WATCH_ADS)
                {
                    GameManager.Instance.WatchRewardADS(_info.itemSell.ItemID.ToLower(), (o) => {
                        if (o)
                        {
                            var itemclaimeds = GameManager.Instance.claimItem(_info.itemSell);
                            List<ItemRewardInfo> itemRewards = new List<ItemRewardInfo>();
                            for (int g = 0; g < itemclaimeds.Length; ++g)
                            {
                                itemRewards.Add(new ItemRewardInfo() { itemReward = itemclaimeds[g] });
                            }
                            HUDManager.Instance.boxReward.show(itemRewards.ToArray(), "BUY SUCCESS");
                        }
                    });
                    return;
                }
                else
                {
                    var exist = GameManager.Instance.Database.getItem(payments[0].exchangeItems[i].item.ItemID);
                    if (exist.QuantityBig < BigInteger.Parse(payments[0].exchangeItems[i].quantity.clearDot()))
                    {
                        HUDManager.Instance.showBoxNotEnough(exist.item);
                        return;
                    }
                    else
                    {
                        exist.addQuantity((-BigInteger.Parse(payments[0].exchangeItems[i].quantity.clearDot())).ToString());
                    }
                }


            }
            GameManager.Instance.claimItem(_info.itemSell);
        }

        private void OnEnable()
        {
            if (independence)
            {
                StartCoroutine(autoLoad());
            }
        }
        public IEnumerator autoLoad()
        {
           while(!GameManager.readyForThisState("Main"))
            {
                yield return new WaitForEndOfFrame();
            }
            var shop =GameDatabase.Instance.ShopCollection.Find(x => x.nameShop == nameShop);
            if (shop)
            {
                var item = System.Array.Find(shop.items, x => x.itemSell.ItemID == itemName);
                if (item != null)
                {
                    setInfo(item);
                }
            }
        }


        public override void setInfo(ShopItemInfo pInfo)
        {
            base.setInfo(pInfo);
            if (bonuslabel)
            {
                bonuslabel.gameObject.SetActive(pInfo.bonusForItem(pInfo) > 0);
                bonuslabel.text = "+"+ (int)(pInfo.bonusForItem(pInfo) * 100) + "%" + " Bonus";
            }
            pInfo.itemSell.getSpriteForState((o) =>
            {
                icon.sprite2D = o;
                icon.MakePixelPerfectClaimIn(new Vector2Int(icon.width, icon.height));
            });
            if (des)
            {
                des.text = pInfo.itemSell.Desc;
            }
            if (nameItem)
            {
                nameItem.text = pInfo.LabelName;
            }
      
        }
    }
    public interface IShop
    {
        GameObject getContainer();
        string shopID();
        bool isInitDone();
    }
    public class BaseLayerShop<T0> : BaseNormalBox<T0, ShopItemInfo> , IShop where T0 : BaseItemShop 
    {
        public string nameShop;
        [System.NonSerialized]
        public bool _isInitDone = false;

        protected Coroutine checkStateCoroutine;

        public virtual float getDiscountForItem(ShopItemInfo item,int paymentWay,int exchangeIndex)
        {
            return 0;
        }
      public virtual float getBonusForItem(ShopItemInfo item)
        {
            return 1;
        }
        protected virtual void OnEnable()
        {
            checkStateCoroutine = StartCoroutine(checkState());
        }
        protected virtual void OnDisable()
        {
            if (checkStateCoroutine != null)
            {
                StopCoroutine(checkStateCoroutine);
            }
        }

        protected IEnumerator checkState()
        {
            while (!GameManager.readyForThisState("Main"))
            {
                yield return new WaitForEndOfFrame();

            }
            reload();
        }

        public virtual void reload()
        {
           var shop =  GameDatabase.Instance.ShopCollection.Find(x => x.nameShop == nameShop+GameManager.Instance.ZoneChoosed);
            if(shop == null)
            {
                shop = GameDatabase.Instance.ShopCollection.Find(x => x.nameShop == nameShop );
            }
            if (shop)
            {
                var items = System.Array.FindAll(shop.items, x => x.isVisibleItem);
                for(int i = 0; i < items.Length; ++i)
                {
                    items[i].bonusForItem = getBonusForItem;
                    items[i].discountEvent = getDiscountForItem;
                }
                executeInfos(items);
            }
        }

        public bool isInitDone()
        {
            return _isInitDone;
        }

        public virtual void buyWayOne(object data)
        {
            ShopItemInfo item = (ShopItemInfo)data;
            var payments = item.getCurrentPrice();
            for(int i = 0; i < payments[0].exchangeItems.Length; ++i)
            {
                if(payments[0].exchangeItems[i].IAP)
                {
                    GameManager.Instance.RequestInappForItem(item.itemSell.ItemID.ToLower(),(o)=> {
                        if (o)
                        {
                            var itemclaimeds = GameManager.Instance.claimItem(item.itemSell,"1",item.bonusForItem(item));
                            List<ItemRewardInfo> itemRewards = new List<ItemRewardInfo>();
                            for(int g = 0; g < itemclaimeds.Length; ++g)
                            {
                                itemRewards.Add(new ItemRewardInfo() { itemReward = itemclaimeds[g] });
                            }
                            HUDManager.Instance.boxReward.show(itemRewards.ToArray(),"BUY SUCCESS");
                        }
                    });
                    return;
                }
                else
                {
                    var exist = GameManager.Instance.Database.getItem(payments[0].exchangeItems[i].item.ItemID);
                    if (exist.QuantityBig < BigInteger.Parse(payments[0].exchangeItems[i].quantity.clearDot()))
                    {
                        HUDManager.Instance.showBoxNotEnough(exist.item);
                        return;
                    }
                    else
                    {
                        int discount =(int)( (1-getDiscountForItem(item, 0, i))*100);
                        exist.addQuantity((-BigInteger.Parse(payments[0].exchangeItems[i].quantity.clearDot())* discount/100).ToString());
                    }
                }
             
             
            }
            claimItem(item.itemSell, getBonusForItem(item));
        }

        public virtual void claimItem(BaseItemGame item,float bonus = 0)
        {
            GameManager.Instance.claimItem(item,"1",bonus);
        }

         public string shopID()
        {
            return nameShop;
        }

        public GameObject getContainer()
        {
            return gameObject;
        }
    }
    public class LayerShopGold : BaseLayerShop<SimpleItemShop>
    {
        public EnvelopContent boxOut;
        protected override void OnEnable()
        {
            base.OnEnable();
            boxOut.Execute();
        }
        public override void reload()
        {
            base.reload();
            _isInitDone = true;
        }
    }
}

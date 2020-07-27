using System.Collections;
using System.Collections.Generic;
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
        public UI2DSprite icon;
        public UILabel nameItem,des;
      

        public override void setInfo(ShopItemInfo pInfo)
        {
            base.setInfo(pInfo);
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
        bool isInitDone();
    }
    public class BaseLayerShop<T0> : BaseNormalBox<T0, ShopItemInfo> , IShop where T0 : BaseItemShop 
    {
        public string nameShop;
        [System.NonSerialized]
        public bool _isInitDone = false;

        protected Coroutine checkStateCoroutine;
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

        IEnumerator checkState()
        {
            while (!GameManager.readyForThisState("Main"))
            {
                yield return new WaitForEndOfFrame();

            }
            reload();
        }

        public virtual void reload()
        {
           var shop =  GameDatabase.Instance.ShopCollection.Find(x => x.nameShop == nameShop);
            if (shop)
            {
                executeInfos(shop.items);
            }
        }

        public bool isInitDone()
        {
            return _isInitDone;
        }

        public void buyWayOne(object data)
        {
            ShopItemInfo item = (ShopItemInfo)data;
            var payments = item.getCurrentPrice();
            for(int i = 0; i < payments[0].exchangeItems.Length; ++i)
            {
                Debug.Log("Sub " + payments[0].exchangeItems[i].item.ItemID + "quantity" + payments[0].exchangeItems[i].quantity);
            }
            GameManager.Instance.claimItem(item.itemSell);
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

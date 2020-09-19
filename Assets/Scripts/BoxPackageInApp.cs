using EazyEngine.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{
    public class BoxPackageInApp : MonoBehaviour, EzEventListener<TimeEvent>, EzEventListener<RemoveTimeEvent>
    {
        public GameObject boxTime;
        public UILabel price;
        public UILabel namePackage;
        public UIElement container;
        public GameObject attachMent;
        public GameObject loading;
        protected ShopItemInfo _info;
        protected CreatureItem _creature;
        protected string listentTime;
        public void showData(ShopItemInfo info)
        {
            //DateTime timeNow = DateTime.now
            //if (TimeCounter.Instance.getCurrentTime())
                _info = info;
            if (loading)
            {
                loading.gameObject.SetActive(false);
            }
            var timeExist = TimeCounter.Instance.timeCollection.Value.Find(x => x.id == $"[Package]{info.itemSell.ItemID}/{GameManager.Instance.ZoneChoosed}");
            if (timeExist == null)
            {
                var random = UnityEngine.Random.Range(0, 2);
                listentTime = $"[Package]{info.itemSell.ItemID}";
                TimeCounter.Instance.addTimer(new TimeCounterInfo() { autoRemoveIfToDestiny = true, id = $"[Package]{info.itemSell.ItemID}/{GameManager.Instance.ZoneChoosed}", destinyIfHave = 86400 });
                listentTime = $"[Package]{info.itemSell.ItemID}/{GameManager.Instance.ZoneChoosed}";
                HUDManager.Instance.checkExistPackage();
            }
            else
            {
                listentTime = timeExist.id;
            }
            info.itemSell.getModelForState((o) =>
            {
                var  model = attachMent.AddChild(o);
                model.transform.localPosition = o.transform.localPosition;
                NGUITools.BringForward(model);
            },"Box");
            var localize = EasyMobile.InAppPurchasing.GetProductLocalizedData(EasyMobile.InAppPurchasing.GetIAPProductById(info.itemSell.ItemID.ToLower()).Name);
            price.text = localize != null ? localize.localizedPriceString : "1$";
            namePackage.text = info.itemSell.displayNameItem.Value;
            container.show();
        }

        public void buy()
        {
            if (loading)
            {
                loading.gameObject.SetActive(true);
            }
            GameManager.Instance.RequestInappForItem(_info.itemSell.ItemID.ToLower(), (o) =>
            {
                if (loading)
                {
                    loading.gameObject.SetActive(false);
                }
                if (o)
                {
                 
                    var items  = GameManager.Instance.claimItem(_info.itemSell);
                    var itemRewards = new List<ItemRewardInfo>();
                    for (int i = 0; i < items.Length; ++i)
                    {
                        itemRewards.Add(new ItemRewardInfo() { itemReward = items[i] });
                    }
                    HUDManager.Instance.boxReward.show(itemRewards.ToArray(), _info.itemSell.displayNameItem.Value);
                    container.close();
                    var timeExist = TimeCounter.Instance.timeCollection.Value.Find(x => x.id == $"[Package]{_info.itemSell.ItemID}/{GameManager.Instance.ZoneChoosed}");
                    if (timeExist != null)
                    {
                         GameManager.Instance.Database.removeTime(timeExist);
                    }
                }
            });
        }

        private void OnEnable()
        {
            EzEventManager.AddListener<RemoveTimeEvent>(this);
            EzEventManager.AddListener<TimeEvent>(this);
        }
        private void OnDisable()
        {
            EzEventManager.RemoveListener<RemoveTimeEvent>(this);
            EzEventManager.RemoveListener<TimeEvent>(this);
        }

        public void OnEzEvent(TimeEvent eventType)
        {
            if (!string.IsNullOrEmpty(listentTime) && eventType.timeInfo.id == listentTime)
            {
                var sec = eventType.timeInfo.destinyIfHave - eventType.timeInfo.CounterTime;
                if (sec < 0)
                {
                    sec = 0;
                }
                var timeSpan = TimeSpan.FromSeconds(sec);

                boxTime.GetComponentInChildren<UILabel>().text = string.Format("{0}H {1}M {2}S", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            }
        }

        public void OnEzEvent(RemoveTimeEvent eventType)
        {
            if (!string.IsNullOrEmpty(listentTime) && eventType.timeInfo.id == listentTime)
            {
                container.close();
            }
        }
    }
}
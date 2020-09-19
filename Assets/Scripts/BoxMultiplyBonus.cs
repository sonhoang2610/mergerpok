using EazyEngine.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{
    public class BoxMultiplyBonus : MonoBehaviour,EzEventListener<TimeEvent>, EzEventListener<RemoveTimeEvent>
    {
        public UI2DSprite[] pokIcon;
        public GameObject onetimeOffer, boxTime;
        public UILabel price;
        public UILabel namePo;
        public UIElement container;
        public GameObject loading;
        protected Vector2Int cacheSizeIcon = Vector2Int.zero;
        protected ShopItemInfo _info;
        protected CreatureItem _creature;
        protected string listentTime;
        public void showData(ShopItemInfo info,CreatureItem creature)
        {
            if (cacheSizeIcon == Vector2Int.zero)
            {
                cacheSizeIcon = new Vector2Int(pokIcon[0].width, pokIcon[0].height);
            }
            _creature = creature;
            _info = info;
            if (loading)
            {
                loading.gameObject.SetActive(false);
            }
           var timeExist = TimeCounter.Instance.timeCollection.Value.Find(x => x.id == $"[MultiplyBonus]{info.itemSell.ItemID}/{creature.ItemID}/{GameManager.Instance.ZoneChoosed}");
            if (timeExist == null)
            {
                var random = UnityEngine.Random.Range(0,3);
                onetimeOffer.gameObject.SetActive(random == 0);
                boxTime.gameObject.SetActive(!(random == 0));
                if (random != 0)
                {
                    listentTime = $"[MultiplyBonus]{info.itemSell.ItemID}/{creature.ItemID}/{GameManager.Instance.ZoneChoosed}";
                    TimeCounter.Instance.addTimer(new TimeCounterInfo() { autoRemoveIfToDestiny = true, id = $"[MultiplyBonus]{info.itemSell.ItemID}/{creature.ItemID}/{GameManager.Instance.ZoneChoosed}", destinyIfHave = 10800 });
                    listentTime = $"[MultiplyBonus]{info.itemSell.ItemID}/{creature.ItemID}/{GameManager.Instance.ZoneChoosed}";
                    HUDManager.Instance.checkExistMultiplyBonus();
                }
            }
            else
            {
                listentTime = timeExist.id;
                onetimeOffer.gameObject.SetActive(false);
                boxTime.gameObject.SetActive(true);
            }
            creature.getSpriteForState((o) =>
            {
                foreach (var icon in pokIcon)
                {
                    icon.sprite2D = o;
                    icon.MakePixelPerfectClaimIn(cacheSizeIcon);
                }
            });
            var localize = EasyMobile.InAppPurchasing.GetProductLocalizedData(EasyMobile.InAppPurchasing.GetIAPProductById(info.itemSell.ItemID).Name);
            price.text = localize != null ? localize .localizedPriceString : "1$";
            namePo.text = creature.displayNameItem.Value;
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
              
                    for(int i =0; i < 6; ++i)
                    {
                        var creature = _creature; ;
                        var zone = GameDatabase.Instance.ZoneCollection.Find(x => x.ItemID == GameManager.Instance.ZoneChoosed);
                        var pack = zone.getPackage();
                        var mapParent = GameManager.Instance.Database.worldData.zones.Find(x => x.id == GameManager.Instance.ZoneChoosed).maps.Find(x => creature.parentMap && x.id == creature.parentMap.ItemID);
                        var newCreature = new PackageCreatureInstanceSaved() { creature = creature.ItemID, id = pack.package.ItemID, instanceID = GameManager.Instance.GenerateID.ToString(), mapParent = mapParent };
                        GameManager.Instance.GenerateID++;
                        GameManager.Instance.Database.worldData.addCreature(newCreature, GameManager.Instance.ZoneChoosed);
                        EzEventManager.TriggerEvent(new AddCreatureEvent()
                        {
                            change = 1,
                            creature = newCreature,
                            zoneid = GameManager.Instance.ZoneChoosed,
                            manualByHand = false,
                        });
                    }
                    var itemRewards = new List<ItemRewardInfo>();
                    itemRewards.Add(new ItemRewardInfo() { itemReward =new ItemWithQuantity() {item = _creature,quantity = "6" } });
                    HUDManager.Instance.boxReward.show(itemRewards.ToArray(), _info.itemSell.displayNameItem.Value);
                    var timeExist = TimeCounter.Instance.timeCollection.Value.Find(x => x.id == $"[MultiplyBonus]{_info.itemSell.ItemID}/{_creature.ItemID}/{GameManager.Instance.ZoneChoosed}");
                    if (timeExist != null)
                    {
                        GameManager.Instance.Database.removeTime(timeExist);
                    }
                    container.close();
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

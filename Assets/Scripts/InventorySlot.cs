using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using EazyEngine.Tools;
using System;

namespace Pok
{
    public enum QuantityType
    {
        NONE,
        ADD_DOT,
        ADD_TEXT,
    }

    public class InventorySlot : BaseItem<BaseItemGameInstanced>, EzEventListener<GameDatabaseInventoryEvent>, EzEventListener<TimeEvent>, EzEventListener<RemoveTimeEvent>
    {
        public bool independence;
        [ShowIf("independence")]
        public string itemID;
        [ShowIf("independence")]
        public bool variantZoneItem = false;
        [ShowIf("independence")]
        public string stateReadyToLoad = "Main";
        // public QuantityType typeQuantity;
        public UILabel quantityLbl;
        public UI2DSprite iconSprite;
        public UI2DSprite process, processQuantity;
        public UILabel timerLabel;
        public bool reserveTime;
        public string format = @"mm\:ss";
        protected Coroutine enableCorountine;

        public string ItemID { get => itemID + (variantZoneItem ? GameManager.Instance.ZoneChoosed : ""); set => itemID = value; }

        public IEnumerator onEnable()
        {
            while (!GameManager.readyForThisState(stateReadyToLoad))
            {
                yield return new WaitForEndOfFrame();
            }
            EzEventManager.AddListener<GameDatabaseInventoryEvent>(this);
            EzEventManager.AddListener<TimeEvent>(this);
            EzEventManager.AddListener<RemoveTimeEvent>(this);
            if (independence)
            {
                var infoItem = GameManager.Instance.Database.getItem(ItemID );
                if (infoItem == null)
                {
                    infoItem = GameManager.Instance.Database.getCreatureItem(ItemID, GameManager.Instance.ZoneChoosed);
                }
                setInfo(infoItem);
            }

        }

        public void updateTime(TimeEvent eventTime)
        {
            if (eventTime.timeInfo.destinyIfHave != -1)
            {
                var localprocess = Mathf.Clamp((float)(eventTime.timeInfo.CounterTime / eventTime.timeInfo.destinyIfHave), 0, 1);
                if (process)
                {
                    process.fillAmount = localprocess;
                }
                if (timerLabel)
                {
                    var sec = (eventTime.timeInfo.destinyIfHave - eventTime.timeInfo.CounterTime).Clamp(eventTime.timeInfo.destinyIfHave, 0);
                    timerLabel.text = TimeSpan.FromSeconds(!reserveTime ? eventTime.timeInfo.CounterTime : (sec+1)).ToString(format);
                }
            }
        }
        protected virtual void OnEnable()
        {

            enableCorountine = StartCoroutine(onEnable());


        }

        protected virtual void OnDisable()
        {

            EzEventManager.RemoveListener<TimeEvent>(this);
            EzEventManager.RemoveListener<RemoveTimeEvent>(this);

            EzEventManager.RemoveListener<GameDatabaseInventoryEvent>(this);
            if (enableCorountine != null)
            {
                StopCoroutine(enableCorountine);
                enableCorountine = null;
            }
        }

        public override void setInfo(BaseItemGameInstanced pInfo)
        {
            base.setInfo(pInfo);
            if (iconSprite)
            {
                pInfo.item.getSpriteForState((x) => { iconSprite.sprite2D = x; });
                var SpriteInfo = pInfo.item.getSpriteInfoForState();
                if (SpriteInfo.size != Vector2.zero)
                {
                    iconSprite.width = (int)SpriteInfo.size.x;
                    iconSprite.height = (int)SpriteInfo.size.y;
                }
            }
            if (quantityLbl)
            {
                quantityLbl.text = pInfo.getQuantity().ToKMBTA();
                if ((pInfo.item.attribute & AttributeItem.Limit) == AttributeItem.Limit)
                {
                    quantityLbl.text = pInfo.getQuantity().ToKMBTA() + "/" + pInfo.item.limitInInventory.getUnit(pInfo.CurrentLevel).ToString();
                }
                else
                {
                    quantityLbl.text = pInfo.getQuantity().ToKMBTA();
                }
            }
            if ((pInfo.item.attribute & AttributeItem.Limit) == AttributeItem.Limit)
            {
                if (processQuantity)
                {
                    processQuantity.fillAmount = float.Parse(pInfo.getQuantity()) / (float)pInfo.item.limitInInventory.getUnit(pInfo.CurrentLevel);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnEzEvent(GameDatabaseInventoryEvent eventType)
        {
            if ((eventType.item.item.ItemID == ItemID && independence) || (!independence && _info != null && _info.item.ItemID == eventType.item.item.ItemID))
            {
                if (quantityLbl)
                {
                    if ((_info.item.attribute & AttributeItem.Limit) == AttributeItem.Limit)
                    {
                        quantityLbl.text = _info.getQuantity().ToKMBTA() + "/" + _info.item.limitInInventory.getUnit(_info.CurrentLevel).ToString();
                    }
                    else
                    {
                        quantityLbl.text = _info.getQuantity().ToKMBTA();
                    }
                }
                if ((_info.item.attribute & AttributeItem.Limit) == AttributeItem.Limit)
                {
                    if (processQuantity)
                    {
                        processQuantity.fillAmount = float.Parse(_info.getQuantity()) / (float)_info.item.limitInInventory.getUnit(_info.CurrentLevel);
                    }
                }
            }
        }

        public void OnEzEvent(TimeEvent eventType)
        {
            var item = GameDatabase.Instance.getItemInventory(ItemID);
            string extra = (item.categoryItem == CategoryItem.CREATURE || item.categoryItem == CategoryItem.PACKAGE_CREATURE) ? (GameManager.Instance.ZoneChoosed + "/") : "";

            if ("[Restore]" + extra + ItemID == eventType.timeInfo.id)
            {
                updateTime(eventType);
            }
        }

        public void OnEzEvent(RemoveTimeEvent eventType)
        {
            var item = GameDatabase.Instance.getItemInventory(ItemID);
            string extra = (item.categoryItem == CategoryItem.CREATURE || item.categoryItem == CategoryItem.PACKAGE_CREATURE) ? (GameManager.Instance.ZoneChoosed + "/") : "";

            if ("[Restore]" + extra + ItemID == eventType.timeInfo.id)
            {
                if (process)
                {
                    process.fillAmount = 1;
                }
                if (timerLabel)
                {
                    timerLabel.text = "FULL";
                }
            }
        }
    }
}

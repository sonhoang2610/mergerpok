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
    
    public class InventorySlot : BaseItem<BaseItemGameInstanced>,EzEventListener<GameDatabaseInventoryEvent>
    {
        public bool independence;
        [ShowIf("independence")]
        public string itemID;
        [ShowIf("independence")]
        public string stateReadyToLoad = "Main";
       // public QuantityType typeQuantity;
        public UILabel quantityLbl;
        public UI2DSprite iconSprite;
        public bool recoverItem;
        public UI2DSprite process;

        protected Coroutine enableCorountine;
        protected bool registerUpdateTime; 
        public IEnumerator onEnable()
        {
            while (!GameManager.readyForThisState(stateReadyToLoad))
            {
                yield return new WaitForEndOfFrame();
            }
            if (independence)
            {
                var infoItem = GameManager.Instance.Database.getItem( itemID);
                setInfo(infoItem);
            }
 
        }

        public void updateTime(TimeEvent eventTime){
            if (eventTime.timeInfo.destinyIfHave != -1)
            {
               var localprocess = Mathf.Clamp((float)(eventTime.timeInfo.CounterTime / eventTime.timeInfo.destinyIfHave), 0, 1);
                if (process)
                {
                    process.fillAmount = localprocess;
                }
            }
        }
        protected virtual void OnEnable()
        {
            if (process)
            {
                var item = GameDatabase.Instance.getItemInventory(itemID);
                string extra = (item.categoryItem == CategoryItem.CREATURE || item.categoryItem == CategoryItem.PACKAGE_CREATURE) ? (GameManager.Instance.ZoneChoosed + "/") : "";
                registerUpdateTime = TimeCounter.Instance.registerUpdateTime("[Restore]"+ extra + itemID, updateTime);
            }
            enableCorountine = StartCoroutine(onEnable());
            EzEventManager.AddListener<GameDatabaseInventoryEvent>(this);
        }

        protected virtual void OnDisable()
        {
            if (registerUpdateTime && !TimeCounter.InstanceRaw.IsDestroyed() && !GameManager.InstanceRaw.IsDestroyed())
            {
                var item = GameDatabase.Instance.getItemInventory(itemID);
                string extra = (item.categoryItem == CategoryItem.CREATURE || item.categoryItem == CategoryItem.PACKAGE_CREATURE) ? (GameManager.Instance.ZoneChoosed + "/") : "";
                TimeCounter.Instance.unRegisterUpdateTime("[Restore]"+extra + itemID, updateTime);
            }
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
            }
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void OnEzEvent(GameDatabaseInventoryEvent eventType)
        {
            if((eventType.item.item.ItemID == itemID && independence) || (!independence && _info != null && _info.item.ItemID == eventType.item.item.ItemID) )
            {
                if (quantityLbl)
                {
                    quantityLbl.text = eventType.item.getQuantity().ToKMBTA();
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using Sirenix.Serialization;
using UnityEngine.AddressableAssets;
using EazyEngine.Tools;
using System.Numerics;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum CategoryItem
{
   NONE,
   COMMON,
   CREATURE,
   PACKAGE_CREATURE,
}
[Flags]
public enum AttributeItem
{
    None = 0,
    Everything = RestoreAble | Limit | LimitLevel,
    RestoreAble = 1,
    Limit = 2,
    LimitLevel= 4,
}
namespace Pok
{
    [System.Serializable]
    public class BaseItemGameInstanced
    {
        [System.NonSerialized]
        public BaseItemGame item;
        public string itemID;
        public string quantity = "1";
        public bool EmptySlot = true;
        [System.NonSerialized]
        public string changeQuantity = "0";
        public int boughtNumber;
        private long beforeQuantity = 0;


        public string address;
        public bool onInit()
        {
            item = GameDatabase.Instance.getItemInventory(itemID);
      
            return item;
        }

        public void onEnable()
        {
            if (item != null && QuantityBig > 0)
            {
                item.machine?.onEnable(this);
            }
        }
        public string getQuantity()
        {
            return quantity.clearDot();
        }

        public void setQuantity(string valueStr, bool pingNow = true)
        {
            var quantityNumber= System.Numerics.BigInteger.Parse(quantity);
            var value = System.Numerics.BigInteger.Parse(valueStr);
            if(value < 0)
            {
                value = 0;
            }
            var localChangeQuantity = System.Numerics.BigInteger.Parse(changeQuantity);
            if ((item.attribute & AttributeItem.Limit) == AttributeItem.Limit)
            {
                if (value > item.limitInInventory.getCurrentUnit())
                {
                    value = item.limitInInventory.getCurrentUnit();
                }
            }
            localChangeQuantity = value - quantityNumber;
            changeQuantity = localChangeQuantity.toString();
            quantityNumber = value;
            quantity = quantityNumber.toString();
            if (quantityNumber <= 0)
            {
                quantityNumber = 0;
                EmptySlot = true;
                item.machine?.onRemoved(this);
            }
            if (item.categoryItem == CategoryItem.COMMON)
            {
                if (localChangeQuantity > 0 && EmptySlot)
                {
                    EmptySlot = false;
                    item.machine?.onAdded(this);
                    if (pingNow)
                    {
                        EzEventManager.TriggerEvent(new GameDatabaseInventoryEvent(BehaviorDatabase.NEWITEM, this));
                    }
                    else
                    {
                        GameManager.Instance.StartCoroutine(GameManager.Instance.actionOnEndFrame(() => {
                            EzEventManager.TriggerEvent(new GameDatabaseInventoryEvent(BehaviorDatabase.NEWITEM, this));
                        }));
                    }
                }
                else if (localChangeQuantity != 0)
                {
                    item.machine?.onDirtyChange(this);
                    if (pingNow)
                    {
                        EzEventManager.TriggerEvent(new GameDatabaseInventoryEvent(BehaviorDatabase.CHANGE_QUANTITY_ITEM, this));
                    }
                    else
                    {
                        GameManager.Instance.StartCoroutine(GameManager.Instance.actionOnEndFrame(() => {
                            EzEventManager.TriggerEvent(new GameDatabaseInventoryEvent(BehaviorDatabase.CHANGE_QUANTITY_ITEM, this));
                        }));
                    }
                
                }
            
            }
            else
            {
                if (localChangeQuantity > 0)
                {
                    for(int i = 0; i < localChangeQuantity; ++i)
                    {
                        if(!GameManager.Instance.Database.addItem(item.ItemID, address))
                        {
                            break;
                        }
                    }
                    
                }
            }
        }

        public void addQuantity(string add,bool pingNow = true)
        {
            setQuantity((BigInteger.Parse(quantity) + BigInteger.Parse(add)).toString(),pingNow);
            ES3.dirty = true; 
        }

        public long QuantityLong
        {
            get
            {
                return long.Parse(quantity);
            }
        }
        public BigInteger QuantityBig { 
            get
            {
                return BigInteger.Parse(quantity);
            }
        }
        //public virtual long Quantity
        //{
        //    get => quantity;
        //    set
        //    {
        //        if ((item.attribute & AttributeItem.Limit) == AttributeItem.Limit)
        //        {
        //            if (value > item.limitInInventory.getUnit(CurrentLevel))
        //            {
        //                value = item.limitInInventory.getUnit(CurrentLevel);
        //            }
        //        }
        //        changeQuantity = value - quantity;
        //        quantity = value;
        //        if(quantity < 0)
        //            quantity = 0;
        //        if (item.categoryItem == CategoryItem.COMMON)
        //        {
        //            if (changeQuantity > 0 && EmptySlot)
        //            {
        //                EzEventManager.TriggerEvent(new GameDatabaseInventoryEvent(BehaviorDatabase.NEWITEM, this));
        //            }
        //            else if (changeQuantity != 0)
        //            {
        //                EzEventManager.TriggerEvent(new GameDatabaseInventoryEvent(BehaviorDatabase.CHANGE_QUANTITY_ITEM, this));
        //            }
        //        }
        //        else
        //        {
        //            GameManager.Instance.Database.addItem(item.ItemID, changeQuantity,address);
        //        }
        //    }
        //}
        [SerializeField]
        protected int level = 0;
 
        public int CurrentLevel
        {
            get
            {
                return item.variantLevel ? ES3.Load<int>($"Level_{item.ItemID}_{GameManager.Instance.ZoneChoosed}", item.defaultLevel) : level;
            }
            set
            {
                if (!item.variantLevel)
                {
                    bool dirty = false;
                    if (level != value)
                    {
                        dirty = true;
                    }
                    level = value;
                    if (dirty)
                    {
                        if (item.machine != null)
                        {
                            item.machine.onDirtyChange(this);
                        }
                    }
                }
                else
                {
                    var levelVariant = ES3.Load<int>($"Level_{item.ItemID}_{GameManager.Instance.ZoneChoosed}", item.defaultLevel);
                    bool dirty = false;
                    if (levelVariant != value)
                    {
                        dirty = true;
                    }
                    ES3.Save<int>($"Level_{item.ItemID}_{GameManager.Instance.ZoneChoosed}", value);
                    if (dirty)
                    {
                        if (item.machine != null)
                        {
                            item.machine.onDirtyChange(this);
                        }
                    }
                }
            }
        }
        protected long BeforeQuantity { get => beforeQuantity; set => beforeQuantity = value; }
    }




    [System.Serializable]
    public class GenericBehavior<T>
    {
        public string State = "Default";
        public T Icon;
        public UnityEngine.Vector2 size;
    }
    [System.Serializable]
    public class IconBehavior : GenericBehavior<AssetReferenceSprite>
    {
    }
    [System.Serializable]
    public class GameObjectBehavior : GenericBehavior<AssetReferenceGameObject>
    {
    }
    [System.Serializable]
    [CreateAssetMenu(fileName = "Item",menuName = "Pok/NormalItem")]
    public class BaseItemGame : SerializedScriptableObject
    {
#if UNITY_EDITOR
        [MenuItem("Assets/AddDatabaseInventory", true)]
         static bool CheckDatabaseInventory()
        {
            return Selection.activeObject is BaseItemGame;
        }
        [MenuItem("Assets/AddDatabaseInventory")]
        static void AddDatabaseInventory()
        {
            for(int i = 0; i < Selection.objects.Length; ++i)
            {
                GameDatabase.Instance.addItemInventory((BaseItemGame)Selection.objects[i]);
       
            }
            GameDatabase.Instance.ItemInventoryCollection.RemoveAll(x => x == null);
            EditorUtility.SetDirty(GameDatabase.Instance);
            AssetDatabase.SaveAssets();
        }
#endif

        public bool variantItem = false;
        public bool variantLevel = false;
        public int defaultLevel = 0;
        public string itemID;
        public I2String displayNameItem;
        public I2String descriptionItem;
        public CategoryItem categoryItem;
        public AttributeItem attribute;
        public List<IconBehavior> icons = new List<IconBehavior>() { new IconBehavior()};
        public List<GameObjectBehavior> model = new List<GameObjectBehavior>() { new GameObjectBehavior() };
        public int score;
        public Dictionary<string, object> blackBoardVariable = new Dictionary<string, object>();
        public System.Func<string> updateString { get; set; }
        public string GetUpgradeString()
        {
            return updateString != null ? updateString() : "";
        }
        public virtual string getContent()
        {
            return Desc;
        }
        public bool isLimit
        {
            get
            {
                return (attribute & AttributeItem.Limit) == AttributeItem.Limit;
            }
        }
        [System.NonSerialized,OdinSerialize]
        [ShowIf("isLimit")]
        public UnitDefineLevelInt limitInInventory;
        public bool isRestoreAble
        {
            get
            {
                return (attribute & AttributeItem.RestoreAble) == AttributeItem.RestoreAble;
            }
        }

        public bool isLimitLevel
        {
            get
            {
                return (attribute & AttributeItem.LimitLevel) == AttributeItem.LimitLevel;
            }
        }
        [ShowIf("isRestoreAble")]
        public UnitDefineLevelFloat timeToRestore;
        [ShowIf("isRestoreAble")]
        public UnitDefineLevelInt quantityRestore = new UnitDefineLevelInt() { startUnit = 1};

        [ShowIf("isLimitLevel")]
        public int limitLevel = 1;
        public virtual string Desc
        {
            get
            {
                return descriptionItem.Value;
            }
        }
        

        public string ItemID { get {
                return itemID;
            } set => itemID = value; }

        public void getModelForState(System.Action<GameObject> result, string pState = "Default")
        {
            var pSpriteAsset = model.Find(x => x.State == pState);
            if (pSpriteAsset != null)
            {
                pSpriteAsset.Icon.loadAssetWrapped(result);
            }
            else
            {
                result(null);
            }
        }
        public void getSpriteForState(System.Action<Sprite> result, string pState = "Default")
        {
            var pSpriteAsset = icons.Find(x => x.State == pState);
            if(pSpriteAsset == null)
            {
                pSpriteAsset = icons.Find(x => x.State == "Default");
            }
            if(pSpriteAsset != null)
            {
                pSpriteAsset.Icon.loadAssetWrapped(result);
            }
            else
            {
                result(null);
            }
        }
        public IconBehavior getSpriteInfoForState(string pState = "Default")
        {
            return icons.Find(x => x.State == pState);
        }

        public IMachineItem machine;
    }

}
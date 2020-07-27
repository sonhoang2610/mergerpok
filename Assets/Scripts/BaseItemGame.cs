using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using Sirenix.Serialization;
using UnityEngine.AddressableAssets;
using EazyEngine.Tools;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum CategoryItem
{
   NONE,
   COMMON,
   CREATURE,
   PACKAGE_CREATURE
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
        public long quantity = 1;
        public string bigQuantity = "0";
        [System.NonSerialized]
        public bool isRequire = false;
        public bool EmptySlot = true;
        protected bool isFree = true;
        [System.NonSerialized]
        public long changeQuantity;
        public int boughtNumber;
        private long beforeQuantity = 0;


        public string address;
        public void onInit()
        {
            item = GameDatabase.Instance.getItemInventory(itemID);
        }
        public bool IsFree
        {
            get
            {
                return isFree;
            }
            set
            {
                isFree = value;
            }
        }
        public string getQuantity()
        {
            return quantity.ToString();
        }
        public virtual long Quantity
        {
            get => quantity;
            set
            {
                if ((item.attribute & AttributeItem.Limit) == AttributeItem.Limit)
                {
                    if (value > item.limitInInventory.getUnit(CurrentLevel))
                    {
                        value = item.limitInInventory.getUnit(CurrentLevel);
                    }
                }
                changeQuantity = value - quantity;
                quantity = value;
                if(quantity < 0)
                    quantity = 0;
                if (item.categoryItem == CategoryItem.COMMON)
                {
                    if (changeQuantity > 0 && EmptySlot)
                    {
                        EzEventManager.TriggerEvent(new GameDatabaseInventoryEvent(BehaviorDatabase.NEWITEM, this));
                    }
                    else if (changeQuantity != 0)
                    {
                        EzEventManager.TriggerEvent(new GameDatabaseInventoryEvent(BehaviorDatabase.CHANGE_QUANTITY_ITEM, this));
                    }
                }
                else
                {
                    GameManager.Instance.Database.addItem(item.ItemID, changeQuantity,address);
                }
            }
        }
        [SerializeField]
        protected int level = 0;
        public int CurrentLevel
        {
            get
            {
                return level;
            }
            set
            {
                level = value;
            }
        }
        protected long BeforeQuantity { get => beforeQuantity; set => beforeQuantity = value; }
    }




    [System.Serializable]
    public class GenericBehavior<T>
    {
        public string State = "Default";
        public T Icon;
        public Vector2 size;
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
                EditorUtility.SetDirty(GameDatabase.Instance);
                AssetDatabase.SaveAssets();
            }
           
        }
#endif
        public string itemID;
        public I2String displayNameItem;
        public I2String descriptionItem;
        public CategoryItem categoryItem;
        public AttributeItem attribute;
        public List<IconBehavior> icons = new List<IconBehavior>() { new IconBehavior()};
        public List<GameObjectBehavior> model = new List<GameObjectBehavior>() { new GameObjectBehavior() };
        public int score;

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
    }

}
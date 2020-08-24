using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FlowCanvas.Macros;
using Sirenix.OdinInspector;


namespace Pok
{
    using Sirenix.Serialization;
    using System;
    using System.Numerics;

    public interface ILevelSetter
    {
        void setLevel(int pLevel);
    }
    [System.Serializable]
    public class UnitDefineLevelLong : UnitDefineLevel<SpecifiedLevelStepUnitLong, long>
    {
        public override long addGrowth(long growth, long step)
        {
            return growth + step;
        }

        public override long getStepConstrainEachLevel(int level)
        {
            return startUnit + level * stepUnit;
        }
        
    }
    [System.Serializable]
    public class UnitDefineLevelInt : UnitDefineLevel<SpecifiedLevelStepUnitInt, int>
    {
        public override int addGrowth(int growth, int step)
        {
            return growth + step;
        }

        public override int getStepConstrainEachLevel(int level)
        {
            return startUnit + level*stepUnit;
        }
        
    }
    public class UnitDefineLevelFloat : UnitDefineLevel<SpecifiedLevelStepUnitFloat, float>
    {
        public override float addGrowth(float growth, float step)
        {
            return growth + step;
        }

        public override float getStepConstrainEachLevel(int level)
        {
            return startUnit + level * stepUnit;
        }
        
    }
    [System.Serializable]
    public abstract class UnitDefineLevel<T,T1> : ILevelSetter  where T : SpecifiedLevelStepUnitGeneric<T1> 
    {
        [ShowIf("@(!isSpecifiedUnit || algrothimTypeUpgrade == AlgrothimPriceItem.StepConstrainEachLevel)")]
        public T1 startUnit = default(T1);

        public AlgrothimPriceItem algrothimTypeUpgrade;
        [ShowIf("algrothimTypeUpgrade", AlgrothimPriceItem.StepConstrainEachLevel)]
        public T1 stepUnit;
        [ShowIf("algrothimTypeUpgrade", AlgrothimPriceItem.ConstrainDefineFromeSpecifiedLevel)]
        public bool isSpecifiedUnit = false;
        [ShowIf("algrothimTypeUpgrade", AlgrothimPriceItem.ConstrainDefineFromeSpecifiedLevel)]
        public T[] unitDefines;

      

        protected System.Func<int> CurrentLevel;
        public abstract T1 getStepConstrainEachLevel(int level);
        public abstract T1 addGrowth(T1 growth,T1 step);
        public T1 getUnit(int pLevel)
        {
            if (algrothimTypeUpgrade == AlgrothimPriceItem.StepConstrainEachLevel)
            {
                return getStepConstrainEachLevel(pLevel);
            }
            if (unitDefines == null || unitDefines.Length == 0)
            {
                return startUnit;
            }
            T1 pGrowth = default(T1);
            int indexPrice = unitDefines.Length - 1;
            int pLEvel = pLevel;
            while (pLEvel > 0)
            {
                if (indexPrice < unitDefines.Length && indexPrice >= 0)
                {
                    if (pLEvel >= unitDefines[indexPrice].levelRequire)
                    {
                        if (pLEvel == unitDefines[indexPrice].levelRequire && isSpecifiedUnit)
                        {
                            return unitDefines[indexPrice].unit;
                        }
                        if (isSpecifiedUnit)
                        {
                            pGrowth = unitDefines[indexPrice].unit;
                        }
                        else
                        {
                            pGrowth = addGrowth(pGrowth,unitDefines[indexPrice].unit);
                           // pGrowth += unitDefines[indexPrice].unit;
                        }
                        pLEvel--;
                    }
                    else
                    {
                        indexPrice--;
                    }
                }
                else
                {
                    pLEvel = 0;
                }

            }
            if (isSpecifiedUnit)
            {
                return pGrowth;
            }
            else
            {
                return addGrowth(startUnit, pGrowth);
            }

        }
        public T1 getCurrentUnit()
        {
            return getUnit(CurrentLevel != null  ? CurrentLevel.Invoke() : 0);
        }
        public void setLevel(int pLevel)
        {
            CurrentLevel = () => pLevel;
        }
        public void setLevel(System.Func<int> pLevel)
        {
            CurrentLevel = pLevel;
        }

    }
    public enum AlgrothimPriceItem
    {
        StepConstrainEachLevel,
        ConstrainDefineFromeSpecifiedLevel,
    }

    public class SpecifiedLevelStepUnitGeneric<T>
    {
        public int levelRequire;
        public T unit;
    }
    [System.Serializable]
    public class SpecifiedLevelStepUnitInt : SpecifiedLevelStepUnitGeneric<int>
    {

    }
    [System.Serializable]
    public class SpecifiedLevelStepUnitString : SpecifiedLevelStepUnitGeneric<string>
    {

    }

    [System.Serializable]
    public class UnitDefineLevelStringBigNumber : UnitDefineLevel<SpecifiedLevelStepUnitString, string>
    {
        public override string addGrowth(string growth, string step)
        {
            return (BigInteger.Parse(growth) + BigInteger.Parse(step)).ToString();
        }

        public override string getStepConstrainEachLevel(int level)
        {
            return( BigInteger.Parse(startUnit) + BigInteger.Parse(stepUnit) * level).ToString();
        }
    }
    [System.Serializable]
    public class SpecifiedLevelStepUnitLong : SpecifiedLevelStepUnitGeneric<long>
    {

    }
    [System.Serializable]
    public class SpecifiedLevelStepUnitFloat : SpecifiedLevelStepUnitGeneric<float>
    {
    }
    [System.Serializable]
    public class SpecifiedLevelStepUnitPayment: SpecifiedLevelStepUnitGeneric<List<PaymentWay>>
    {
    }
    [System.Serializable]
    public class UnitDefineLevelPaymentWay : UnitDefineLevel<SpecifiedLevelStepUnitPayment, List<PaymentWay>>
    {
        public override List<PaymentWay> addGrowth(List<PaymentWay> growth, List<PaymentWay> step)
        {
            List<PaymentWay> payments = new List<PaymentWay>();
            mergerPayment(payments, growth);
            mergerPayment(payments, step);
            return payments;
        }
        public void mergerPayment(List<PaymentWay> payments, List<PaymentWay> growth)
        {
            for (int i = 0; i < growth.Count; ++i)
            {
                if (!payments.Exists(x => x.groupID == growth[i].groupID))
                {
                    payments.Add(growth[i]);
                }
                else
                {
                    var paymentWayIndex = payments.FindIndex(x => x.groupID == growth[i].groupID);
                    var paymentWay = payments[paymentWayIndex];
                    paymentWay.exchangeItems = mergerItemQuantity(payments[paymentWayIndex].exchangeItems, growth[i].exchangeItems);
                    payments[paymentWayIndex] = paymentWay;
                }
            }
        }
        public ItemWithQuantity[] mergerItemQuantity(ItemWithQuantity[] a, ItemWithQuantity[] b)
        {
            List<ItemWithQuantity> result = new List<ItemWithQuantity>();
            result.addFromList(a);
            for (int i = 0; i < b.Length; ++i)
            {
                if(!result.Exists(x=>x.item == b[i].item))
                {
                    result.Add(b[i]);
                }
                else
                {
                    var findedIndex = result.FindIndex(x => x.item == b[i].item);
                    result[findedIndex] = result[findedIndex].addQuantity(b[i].quantity);
                }
            }
            return result.ToArray();
        }

        public override List<PaymentWay> getStepConstrainEachLevel(int level)
        {
            List<PaymentWay> newList = new List<PaymentWay>();
         //   newList.addFromList(stepUnit.ToArray());
            for(int i = 0; i < stepUnit.Count; ++i)
            {
                var copy = stepUnit[i];
                var newListItem = new List<ItemWithQuantity>();
                newListItem.addFromList(copy.exchangeItems);
                copy.exchangeItems = newListItem.ToArray();
                newList.Add(copy);
            }
            for (int i = 0; i < newList.Count; ++i)
            {
                for(int j = 0; j < newList[i].exchangeItems.Length; ++j)
                {
                    var abc = newList[i].exchangeItems[j];
                    abc.Mul(level);
                    newList[i].exchangeItems[j] = abc;
                }
            }
            var result = addGrowth(newList, startUnit);
            return result;
        }
        
    }

    [System.Serializable]
    public struct PaymentWay
    {
        public int groupID;
        public ItemWithQuantity[] exchangeItems;

    }
    public enum TypePayment
    {
        Normal,
        IAP,
        WATCH_ADS,
    }
    [System.Serializable]
    public struct ItemWithQuantity
    {
        public TypePayment typePayment;
        public bool IAP
        {
            get
            {
                return typePayment == TypePayment.IAP;
            }
        }
        public bool WATCH_ADS
        {
            get
            {
                return typePayment == TypePayment.WATCH_ADS;
            }
        }
        public bool Normal
        {
            get
            {
                return typePayment == TypePayment.Normal;
            }
        }
        [ShowIf("Normal")]
        public BaseItemGame item;
        [HideIf("WATCH_ADS")]
        public string quantity ;

        public ItemWithQuantity addQuantity(string add)
        {
            var a = quantity.clearDot(); var b = add.clearDot();
            if (quantity.Length < 12 && add.Length < 12)
            {
                quantity = (long.Parse(a) + long.Parse(b)).ToString();
            }
            else
            {
                quantity = (System.Numerics.BigInteger.Parse(a) + System.Numerics.BigInteger.Parse(b)).ToString();
            }
            return this;
        }
        public ItemWithQuantity Mul(int add)
        {
            var a = quantity.clearDot(); 
            if (quantity.Length < 12 )
            {
                quantity = (long.Parse(a) * add).ToString();
            }
            else
            {
                quantity = (System.Numerics.BigInteger.Parse(a) * add).ToString();
            }
            return this;
        }
    }


    public enum IncreaseBaseOn
    {
        LevelItem,
        CountBoughtItem,
    }

    [System.Serializable]
    public class ShopItemInfo
    {
        public string LabelName
        {
            get
            {
                if (itemSell == null)
                {
                    return "none";
                }
                return itemSell.displayNameItem.Value;
            }
        }

        public bool IsVisibleItem { get  {
                if(!string.IsNullOrEmpty(conditionShow.goalPok))
                {
                   return conditionShow.checkBoolCondition();
                }
                return isVisibleItem;
            }  set => isVisibleItem = value; }

        public bool isVisibleItem = true;
        public ObjectToCheckInfo conditionShow;
        public int limitUpgrade = -1;
        public BaseItemGame itemSell;
        public IncreaseBaseOn typeIncrease;
        public UnitDefineLevelPaymentWay paymentWays;

        public Func<ShopItemInfo, int, int, float> discountEvent = (a,b,c)=> { return 0; };
        public Func<ShopItemInfo, float> bonusForItem = (a) => { return 0; };
        public void onInit()
        {
            if(typeIncrease == IncreaseBaseOn.LevelItem)
            {
                paymentWays.setLevel(() => { return GameManager.Instance.getLevelItem(itemSell.ItemID)+1; });
            }
            else
            {
                paymentWays.setLevel(() => { return GameManager.Instance.getNumberBoughtItem(itemSell.ItemID); });
            }
        }
        public int sortGroupPrice(PaymentWay pA, PaymentWay pB)
        {
            return pA.groupID.CompareTo(pB.groupID);
        }
        public PaymentWay[] getCurrentPrice()
        {
            return paymentWays.getCurrentUnit().ToArray();
        }
        public PaymentWay[] getCurrentPrice(int manual)
        {
            return paymentWays.getUnit(manual).ToArray();
        }

        public bool isPaymentByIAP()
        {
            return getCurrentPrice()[0].exchangeItems[0].IAP;
        }
        public BigInteger firstQuantity()
        {
            return BigInteger.Parse( getCurrentPrice()[0].exchangeItems[0].quantity.clearDot());
        }
    }
    [CreateAssetMenu(fileName = "Shop", menuName = "Pok/Shop")]
    public class ShopDatabase : SerializedScriptableObject
    {
        public string nameShop;
        [ListDrawerSettings(AddCopiesLastElement = true, ListElementLabelName = "LabelName")]
        public ShopItemInfo[] items;
#if UNITY_EDITOR
        [Button("chiu")]
        public void chiu()
        {
            for(int i = 1; i < items.Length; ++i)
            {
                Sirenix.Utilities.Editor.Clipboard.Copy<PaymentWay>(items[0].paymentWays.startUnit[1]);
                items[i].paymentWays.startUnit = new List<PaymentWay>() { items[i].paymentWays.startUnit[0],  Sirenix.Utilities.Editor.Clipboard.Paste<PaymentWay>()};
                Sirenix.Utilities.Editor.Clipboard.Copy<PaymentWay>(items[0].paymentWays.stepUnit[1]);
                items[i].paymentWays.stepUnit = new List<PaymentWay>() { items[i].paymentWays.stepUnit[0], Sirenix.Utilities.Editor.Clipboard.Paste<PaymentWay>() };
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
        public void onInit()
        {
            foreach(var item in items)
            {
                item.onInit();
            }
        }
        public bool ContainItem(BaseItemGame pItem)
        {
            for (int i = 0; i < items.Length; ++i)
            {
                if (items[i].itemSell == pItem)
                {
                    return true;
                }
            }
            return false;
        }

        public ShopItemInfo getInfoItem(string pItemID)
        {
            for (int i = 0; i < items.Length; ++i)
            {
                if (items[i].itemSell.ItemID == pItemID)
                {
                    return items[i];
                }
            }
            return null;
        }
        
    }
}

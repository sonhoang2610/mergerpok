using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Pok
{
    using EazyEngine.Tools;
    using Sirenix.OdinInspector;

    public interface IElmentInfoExtract
    {
        ItemWithQuantity[] extraItems();
    }
    public interface IExtractItem
    {
        ItemWithQuantity[] ExtractHere(bool isNew = true);
        int CacheExtraItemCount();
        void disableExtracItem();
        bool alwayExtra();
    }
    [System.Serializable]
    public class ItemRateDropInfo
    {
        public BaseItemGame item;
        public string quantityFrom= "0",quantityTo="0";
        public float percent = 100;
    }
    public enum ExtraType
    {
        Random,
        Queue,
    }
    public class ItemPackage<T> : BaseItemGame, IExtractItem where T : IElmentInfoExtract
    {

        public bool alwayExtras = false;
        public T item;
        public bool alwayExtra()
        {
            return alwayExtras;
        }
        protected ItemWithQuantity[] cacheExtra;
        public ItemWithQuantity[] ExtractHere(bool isNew = true)
        {

            if (!isNew && cacheExtra != null)
            {
                return cacheExtra;
            }
            List<ItemWithQuantity> pItemResult = new List<ItemWithQuantity>();
            pItemResult.addFromList(item.extraItems());
            //int pCount = Random.Range((int)randomQuantity.x, (int)randomQuantity.y == 0 ? ((int)randomQuantity.y + 1) : (int)randomQuantity.y);
            //if (extraType == ExtraType.Random)
            //{
            //    do
            //    {
            //        List<ItemRateDropInfo> pItems = new List<ItemRateDropInfo>();
            //        pItems.AddRange(items);
            //        int pRandom = Random.Range(0, 100);
            //        int pCurrent = 0;
            //        int indexBreak = 0;
            //        while (true)
            //        {
            //            if (pCurrent >= 100 || pItems.Count == 0)
            //            {
            //                break;
            //            }
            //            int index = Random.Range(0, pItems.Count);
            //            if (index >= pItems.Count)
            //            {
            //                continue;
            //            }
            //            pCurrent += (int)pItems[index].percent;
            //            if (pRandom <= pCurrent)
            //            {
            //                if (!typeof(IExtractItem).IsAssignableFrom(pItems[index].item.GetType()) || !((IExtractItem)pItems[index].item).alwayExtra())
            //                {
            //                    var percent = Random.Range(0, 1);
                                
            //                    pItemResult.Add(new BaseItemGameInstanced() { item = pItems[index].item, bigQuantity = ((System.Numerics.BigInteger.Parse(pItems[index].quantityTo) - System.Numerics.BigInteger.Parse(pItems[index].quantityTo)) * percent).ToString() });
            //                }
            //                else
            //                {
            //                    pItemResult.AddRange(((IExtractItem)pItems[index].item).ExtractHere());
            //                }
            //                break;
            //            }
            //            else
            //            {
            //                pItems.RemoveAt(index);
            //            }
            //            indexBreak++;
            //            if (indexBreak > 1000000)
            //            {
            //                break;
            //            }
            //        }
            //        pCount--;
            //    } while (pCount > 0);
            //}
            //else
            //{
            //    int indexQueue = 0;
            //    for(int i = 0; i < pCount; ++i)
            //    {
            //        int percent = Random.Range(0, 100);
            //        if(percent < items[indexQueue].percent)
            //        {
            //            var pQuantity = ((System.Numerics.BigInteger.Parse(items[indexQueue].quantityTo) - System.Numerics.BigInteger.Parse(items[indexQueue].quantityTo)) * percent).ToString();
            //            if (!typeof(IExtractItem).IsAssignableFrom(items[indexQueue].item.GetType()) || !((IExtractItem)items[indexQueue].item).alwayExtra())
            //            {
            //                pItemResult.Add(new BaseItemGameInstanced()
            //                {
            //                    item = items[indexQueue].item,
            //                    bigQuantity = pQuantity
            //                });
            //            }
            //            else
            //            {
            //                pItemResult.AddRange(((IExtractItem)items[indexQueue].item).ExtractHere());
            //            }
                 
            //            indexQueue++;
            //            if(indexQueue >= items.Length)
            //            {
            //                indexQueue = 0;
            //            }
            //        }
            //    }
            //}
            cacheExtra = pItemResult.ToArray();
           return pItemResult.ToArray();
        }

        public int CacheExtraItemCount()
        {
            return cacheExtra== null ? 0: cacheExtra.Length;
        }

        public void disableExtracItem()
        {
            cacheExtra = null;
        }
    }
}

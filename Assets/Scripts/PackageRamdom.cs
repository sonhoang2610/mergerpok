using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace Pok
{
    [System.Serializable]
    public class ItemWithQuantityRamdom
    {
        public BaseItemGame item;
        public string quantityFrom, quantityTo;
    }
    [System.Serializable]
    public class ItemPackageRandomInfo : IElmentInfoExtract
    {
        public ItemWithQuantityRamdom[] items;
        public ItemWithQuantity[] extraItems()
        {
            List<ItemWithQuantity> localitems = new List<ItemWithQuantity>();
            for(int i = 0; i < items.Length; ++i)
            {
                var from = BigInteger.Parse(items[i].quantityFrom.clearDot());
                var to = BigInteger.Parse(items[i].quantityTo.clearDot());
                var floatPercent = UnityEngine.Random.Range(0, 100);
                var localQuantity = from + ((to - from) * floatPercent)/100;
                localitems.Add(new ItemWithQuantity() { item = items[i].item, quantity = localQuantity.ToString() });
            }
            return localitems.ToArray();
        }
        public string getContent()
        {
            string content = "";
            for(int i = 0; i < items.Length; ++i)
            {
                content += items[i].quantityFrom.clearDot().ToKMBTA() + " -> " + items[i].quantityTo.clearDot().ToKMBTA() + " ";
            }
            return content;
        }
    }
    [CreateAssetMenu(fileName = "PackRandom", menuName = "Pok/PackRandom")]
    public class PackageRamdom : ItemPackage<ItemPackageRandomInfo>
    {

    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{
    [System.Serializable]
    public class ItemPackageSimpleInfo : IElmentInfoExtract
    {
        public ItemWithQuantity[] items;

        public ItemWithQuantity[] extraItems()
        {
            return items;
        }

        public string getContent()
        {
            return "";
        }
    }
    [System.Serializable]
    [CreateAssetMenu(fileName = "PackageSimple",menuName ="Pok/PackageSimple")]
    public class ItemPackageSimple : ItemPackage<ItemPackageSimpleInfo>
    {
    }
}

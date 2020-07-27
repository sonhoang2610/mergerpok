using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{
    public class ItemPackageSimple : IElmentInfoExtract
    {
        public ItemWithQuantity[] items;

        public ItemWithQuantity[] extraItems()
        {
            return items;
        }
    }
    [System.Serializable]
    public class ItePackageSimple : ItemPackage<ItemPackageSimple>
    {
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{
    [System.Serializable]
    public class ItemPackageGoldInfo : IElmentInfoExtract
    {
        public int secAFK = 3600;
        public ItemWithQuantity[] extraItems()
        {
          return  new ItemWithQuantity[] { new ItemWithQuantity() { item = GameDatabase.Instance.getItemInventory("Coin"), quantity = GameManager.Instance.getTotalGoldGrowthCurrentZone() } };
        }
    }
    public class ItemPackageGold : ItemPackage<ItemPackageGoldInfo>
    {
        
    }
}

using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{
    [System.Serializable]
    public class ItemPackageGoldInfo : IElmentInfoExtract
    {
        public bool isRandom = false;
        [HideIf("isRandom")]
        public int secAFK = 3600;
        [ShowIf("isRandom")]
        public Vector2Int secAFKRandom = new Vector2Int(3600,3600);
        public ItemWithQuantity[] extraItems()
        {
            if (!isRandom)
            {
                return new ItemWithQuantity[] { new ItemWithQuantity() { item = GameDatabase.Instance.getItemInventory("Coin"), quantity = (System.Numerics.BigInteger.Parse(GameManager.Instance.getTotalGoldGrowthCurrentZone()) * secAFK).ToString() } };
            }
            else
            {
                var sec = Random.Range(secAFKRandom.x, secAFKRandom.y);
                return new ItemWithQuantity[] { new ItemWithQuantity() { item = GameDatabase.Instance.getItemInventory("Coin"), quantity = (System.Numerics.BigInteger.Parse(GameManager.Instance.getTotalGoldGrowthCurrentZone()) * sec).ToString() } };
            }
        }
        public string getContent()
        {
            if (!isRandom)
            {
                return (System.Numerics.BigInteger.Parse(GameManager.Instance.getTotalGoldGrowthCurrentZone()) * secAFK).ToString() ;
            }
            else
            {
                var from = (System.Numerics.BigInteger.Parse(GameManager.Instance.getTotalGoldGrowthCurrentZone()) * secAFKRandom.x).ToString();
                var to = (System.Numerics.BigInteger.Parse(GameManager.Instance.getTotalGoldGrowthCurrentZone()) * secAFKRandom.y).ToString();
                return from.ToKMBTA() + " -> " + to.ToKMBTA();
            }
        }
    }
    [CreateAssetMenu(fileName = "PackGold" ,menuName ="Pok/PackGold")]
    public class ItemPackageGold : ItemPackage<ItemPackageGoldInfo>
    {
    }
}

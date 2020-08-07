using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{
    [System.Serializable]
    public class ItemPackageRandomCreatureInfo : IElmentInfoExtract
    {
        public Vector2Int indexRamdom = new Vector2Int(2, 5);
        public ItemWithQuantity[] extraItems()
        {
            int random = Random.Range(indexRamdom.x, indexRamdom.y+1);
            return new ItemWithQuantity[] { new ItemWithQuantity() { item = getCreatureDown(random),quantity = "1" } };
        }
        public CreatureItem getCreatureDown(int index)
        {
            var creatureActive = GameManager.Instance.Database.getAllCreatureInfoInZone(GameManager.Instance.ZoneChoosed,true);
            int indexDown = (creatureActive.Length - (index+1)).Clamp(creatureActive.Length, 0);
            return creatureActive[indexDown];
        }
        public string getContent()
        {
            return "Random Pok";
        }
    }
    [CreateAssetMenu(fileName = "PackRandomCreature", menuName = "Pok/PackRandomCreature")]
    public class PackRandomCreature : ItemPackage<ItemPackageRandomCreatureInfo>
    {

    }
}

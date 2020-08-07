using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{

    [CreateAssetMenu(fileName = "ItemPackageCreature", menuName = "Pok/ItemPackageCreature")]
    public class PackageCreatureObject : CreatureItem
    {
        public CreatureItem[] creatureExtra;
    }
}

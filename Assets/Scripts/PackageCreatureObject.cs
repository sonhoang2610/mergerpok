using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{
    [System.Serializable]
    public class SpecifiedLevelStepUnitCreature : SpecifiedLevelStepUnitGeneric<CreatureItem>
    {

    }

    [CreateAssetMenu(fileName = "ItemPackageCreature", menuName = "Pok/ItemPackageCreature")]
    public class PackageCreatureObject : CreatureItem
    {
        public SpecifiedLevelStepUnitCreature[] creatureExtra;
    }
}

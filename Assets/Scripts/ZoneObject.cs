using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Pok
{
    [System.Serializable]
    public class PackageInZone
    {
        public string id;
        public PackageCreatureObject package;
    }
    [CreateAssetMenu(fileName ="Zone",menuName = "Pok/Zone")]
    public class ZoneObject : BaseItemGame
    {
        public PackageInZone[] packages;
        public ShopDatabase shopCreature;
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Sirenix.OdinInspector;


namespace Pok
{
    [CreateAssetMenu(fileName = "Map",menuName = "Pok/Map")]
    public class MapObject : BaseItemGame
    {
        public CreatureItem[] childCreatures;
        public int limitSlot = 16;
        public void onInit()
        {
            foreach (var creature in childCreatures)
            {
                creature.parentMap = this;
                creature.onInit();
            }
        }
    }
}

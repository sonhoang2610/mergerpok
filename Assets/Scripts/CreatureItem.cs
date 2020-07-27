using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Pok
{
    [CreateAssetMenu(fileName = "Creature",menuName = "Pok/Creature")]
    public class CreatureItem : BaseItemGame
    {
        [System.NonSerialized]
        public MapObject parentMap;
        [System.NonSerialized]
        public CreatureItem parentCreature;
        public CreatureItem[] creatureChilds;
        public Dictionary<string, string> goldAFKReward = new Dictionary<string, string>() {
            {"Zone1","0" },   {"Zone2","0" },   {"Zone3","0" },   {"Zone4","0" },   {"Zone5","0" },   {"Zone6","0" }
        };
       public void onInit()
        {
            foreach (var creature in creatureChilds)
            {
                creature.parentCreature = this;
            }
        }

        public void getChild(List<CreatureItem> childList,int size)
        {
            if (childList.Count >= size) return;
            childList.Add(this);
            foreach(var child in creatureChilds)
            {
                child.getChild(childList, size);
            }
        }
        //public void getChildEndClass(List<CreatureItem> childList)
        //{
        //    childList.Add(this);
        //   if(creatureChildss.Count == 1)
        //    {
        //        creatureChilds[0].getChildEndClass(childList);
        //    }
        //}
    }
}

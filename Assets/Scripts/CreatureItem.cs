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

        public int RankChild
        {
            get;set;
        }
        public string getGoldAFK(string zone)
        {
            if (!goldAFKReward.ContainsKey(zone))
            {
                return "0";
            }
            return goldAFKReward[zone].clearDot();
        }
       public void onInit()
        {
            foreach (var creature in creatureChilds)
            {
                creature.parentCreature = this;
            }
        }
        public void getParents(List<CreatureItem> parentList)
        {
            if (parentCreature == null) return;
            parentList.Add(parentCreature);
            parentCreature.getParents(parentList);
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

        public void initRank(int startLevel)
        {
            RankChild = startLevel;
            for(int i = 0; i < creatureChilds.Length; ++i)
            {
                creatureChilds[i].initRank(RankChild + 1);
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

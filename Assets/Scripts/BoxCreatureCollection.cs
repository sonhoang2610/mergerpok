using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;
using Sirenix.OdinInspector;

namespace Pok
{
    [System.Serializable]
    public  class CreatureCollectionGroup{
        [ValueDropdown("ValuesFunction")]
        public string leaderID;
        public ItemCreatureCollection[] items;

        private IList<ValueDropdownItem<string>> ValuesFunction()
        {
            var list = new ValueDropdownList<string>();
            for(int i = 0; i < GameDatabase.Instance.treeCreature.Count; ++i)
            {
                list.Add(GameDatabase.Instance.treeCreature[i].creatureLeader.ItemID, GameDatabase.Instance.treeCreature[i].creatureLeader.ItemID);
            }
            return list;
        }
    }
    public class BoxCreatureCollection : Singleton<BoxCreatureCollection>
    {
        public CreatureCollectionGroup[] groups;
        public UIElement box;

        public void show()
        {
            box.show();
        }
        public void cloase()
        {
            box.close();
        }
        private void OnEnable()
        {
            foreach(var group in groups)
            {
               var treeElement = GameDatabase.Instance.treeCreature.Find(x => x.creatureLeader.ItemID == group.leaderID);
                var ListCreature = new List<CreatureItem>();
                treeElement.creatureLeader.getChild(ListCreature, group.items.Length);
                for(int i = 0; i < ListCreature.Count; ++i)
                {
                    group.items[i].setInfo(ListCreature[i]);
                }
            }
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

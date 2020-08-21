using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{
    public class BoxSelectEra : BaseBox<BoxSelectEra, ItemSelectEra,CreatureItem>
    {
        public UIElement container;
        public EazyGroupTabNGUI tabs;
        protected int indexSelect;

        public System.Action _onClose;
        public CreatureItem selectCreature;
     
        public void show(CreatureItem[] itemInfos)
        {
            container.show();
            executeInfos(itemInfos);
            int selectab = -1;
            for(int i = 0; i < items.Count; ++i)
            {
                tabs.GroupTab.Add(items[i].GetComponent<EazyTabNGUI>());
                var listChild = new List<CreatureItem>();
                var countWay = items[i]._info.countWayFromThisChild();
                bool enableTab = false;
                var selectedleader = GameManager.Instance.Database.zoneInfos.FindAll(x => x.leaderSelected.ContainsValue(items[i]._info.ItemID));
                if(selectedleader.Count < countWay)
                {
                    enableTab = true;
                }
         
            
                items[i].GetComponent<Collider>().enabled = enableTab;
                items[i].setEnable(enableTab);
                if (enableTab && selectab == -1)
                {
                    selectab = i;
                }
            }

            if(items.Count > 2)
            {
                for (int i = 0; i < items.Count; ++i)
                {
                    items[i].transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                }
                attachMent.GetComponent<UIGrid>().cellWidth = 156.5f;
            }
            else
            {
                for (int i = 0; i < items.Count; ++i)
                {
                    items[i].transform.localScale = new Vector3(1, 1, 1);
                }
                attachMent.GetComponent<UIGrid>().cellWidth = 225.1f;
            }
            attachMent.GetComponent<UIGrid>().Reposition();
            if (selectab != -1)
            {
                tabs.changeTab(selectab);
            }
        }
        
        public void selectTab(int index)
        {
            indexSelect = index;
            selectCreature = items[index]._info;
        }

        public void ok()
        {
            container.close();
            _onClose?.Invoke();
        }
    }
}

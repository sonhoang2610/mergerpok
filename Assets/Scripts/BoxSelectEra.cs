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
               var info = GameManager.Instance.Database.creatureInfos.Find(x => x.id == items[i]._info.ItemID);
                if (!info.isUnLock && selectab != -1)
                {
                    selectab = i;
                }
            }

            if(items.Count > 2)
            {
                for (int i = 0; i < items.Count; ++i)
                {
                    items[i].transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                }
                attachMent.GetComponent<UIGrid>().cellWidth = 200.0f;
            }
            else
            {
                for (int i = 0; i < items.Count; ++i)
                {
                    items[i].transform.localScale = new Vector3(1, 1, 1);
                }
                attachMent.GetComponent<UIGrid>().cellWidth = 415.6f;
            }
            if(selectab != -1)
            {
                tabs.changeTab(selectab);
            }
        }
        
        public void selectTab(int index)
        {
            indexSelect = index;
        }

        public void ok()
        {
            container.close();
            _onClose?.Invoke();
        }
    }
}

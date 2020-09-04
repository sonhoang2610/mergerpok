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
        int selectab = -1;
        public void show(CreatureItem[] itemInfos)
        {
            container.show();
            executeInfos(itemInfos);
             selectab = -1;
            var itemActive = getActiveItems();
            tabs.GroupTab.Clear();
            for (int i = 0; i < itemActive.Count; ++i)
            {
                tabs.GroupTab.Add(itemActive[i].GetComponent<EazyTabNGUI>());
                var listChild = new List<CreatureItem>();
                var countWay = itemActive[i]._info.countWayFromThisChild();
                bool enableTab = false;
                var selectedleader = GameManager.Instance.Database.zoneInfos.FindAll(x => x.leaderSelected.ContainsValue(itemActive[i]._info.ItemID));
                if(selectedleader.Count < countWay)
                {
                    enableTab = true;
                }


                itemActive[i].GetComponent<Collider>().enabled = enableTab;
                itemActive[i].setEnable(enableTab);
                if (enableTab && selectab == -1)
                {
                    selectab = i;
                }
            }

            if(itemActive.Count > 2)
            {
                for (int i = 0; i < itemActive.Count; ++i)
                {
                    itemActive[i].transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                }
                attachMent.GetComponent<UIGrid>().cellWidth = 156.5f;
            }
            else
            {
                for (int i = 0; i < itemActive.Count; ++i)
                {
                    itemActive[i].transform.localScale = new Vector3(1, 1, 1);
                }
                attachMent.GetComponent<UIGrid>().cellWidth = 225.1f;
            }
            attachMent.GetComponent<UIGrid>().Reposition();
            tabs.reloadTabs();
            if (selectab != -1)
            {
                tabs.changeTab(selectab);
            }
        }

        public void selectTab(int index)
        {
            indexSelect = index;
            selectCreature = getActiveItems()[index]._info;
        }

        public void ok()
        {
       
            int random = Random.Range(0, 2);
            if(random != 0)
            {
               if(selectCreature.RankChild < 15)
                {
                    GameManager.Instance.StartCoroutine(delayShowBoxMultiplyBonus(0, selectCreature));
                }
                else
                {
                    GameManager.Instance.StartCoroutine(delayShowBoxMultiplyBonus(1, selectCreature));
                }
            }
            container.close();
            _onClose?.Invoke();
        }
        public IEnumerator delayShowBoxMultiplyBonus(int index,CreatureItem creature)
        {
            yield return new WaitForSeconds(2);
            if(index == 0)
            {
                HUDManager.Instance.boxMultiplyBonus.showData(GameDatabase.Instance.packageMultiplyBonus1, creature);
            }
            else
            {
                HUDManager.Instance.boxMultiplyBonus.showData(GameDatabase.Instance.packageMultiplyBonus2, creature);
            }

        }
    }
}

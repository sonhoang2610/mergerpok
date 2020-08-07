using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;

namespace Pok
{
    public class ChooseMapLayer : BaseBox<ChooseMapLayer, MapItemInstanced, MapInstanceSaved>,EzEventListener<UnlockNewEra>
    {
        public EnvelopContent contentResize;
        public EazyGroupTabNGUI tabs;

        public void OnEzEvent(UnlockNewEra eventType)
        {
            var maps = GameManager.Instance.Database.getAllMapActiveInZone(GameManager.Instance.ZoneChoosed);
            executeInfos(maps.ToArray());
            contentResize.Execute();
            tabs.GroupTab.Clear();
            for (int i = 0; i < items.Count; ++i)
            {
                tabs.GroupTab.Add(items[i].GetComponent<EazyTabNGUI>());
                NGUITools.BringForward(items[i].gameObject);
            }
            tabs.reloadTabs();
            tabs.changeTab(0);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EzEventManager.AddListener(this);
            var maps = GameManager.Instance.Database.getAllMapActiveInZone(GameManager.Instance.ZoneChoosed);
            executeInfos(maps.ToArray());
            contentResize.Execute();
            tabs.GroupTab.Clear();
            for (int i = 0; i < items.Count; ++i)
            {
                tabs.GroupTab.Add(items[i].GetComponent<EazyTabNGUI>());
              //  var objecttab = items[i].gameObject;
                //objecttab.GetComponent<UIWidget>().onFindPanel = delegate
                //{
                //    NGUITools.BringForward(objecttab);
                //};
                //NGUITools.BringForward(items[i].gameObject);
            }
            tabs.reloadTabs();
           // tabs.changeTab(0);
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            EzEventManager.RemoveListener(this);
        }


    }
}

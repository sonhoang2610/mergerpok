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
            var itemActive = getActiveItems();
            for (int i = 0; i < itemActive.Count; ++i)
            {
                tabs.GroupTab.Add(itemActive[i].GetComponent<EazyTabNGUI>());
                NGUITools.BringForward(itemActive[i].gameObject);
            }
            tabs.reloadTabs();
            StartCoroutine(checkChangeTab(() =>
            {
                var indexTab = tabs.GroupTab.FindIndex(x => x.GetComponent<MapItemInstanced>()._info.id == eventType.mapID);
                tabs.changeTab(indexTab);
            }));
        }

        public IEnumerator checkChangeTab(System.Action action)
        {
            while(!MainScene.InstanceRaw || MainScene.Instance.MovingMap)
            {
                yield return new WaitForEndOfFrame();
            }
            action?.Invoke();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EzEventManager.AddListener(this);
            var maps = GameManager.Instance.Database.getAllMapActiveInZone(GameManager.Instance.ZoneChoosed);
            executeInfos(maps.ToArray());
            contentResize.Execute();
            tabs.GroupTab.Clear();
            var itemActive = getActiveItems();
            for (int i = 0; i < itemActive.Count; ++i)
            {
                tabs.GroupTab.Add(itemActive[i].GetComponent<EazyTabNGUI>());
            }
            tabs.reloadTabs();
            var oldIndexTab = ES3.Load<int>($"ChooseMap{GameManager.Instance.ZoneChoosed}", 0);
            if(oldIndexTab < tabs.GroupTab.Count)
            {
                tabs.changeTab(oldIndexTab);
            }
            else
            {
                ES3.Save<int>($"ChooseMap{GameManager.Instance.ZoneChoosed}", 0);
                tabs.changeTab(0);
            }
          
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            EzEventManager.RemoveListener(this);
        }


    }
}

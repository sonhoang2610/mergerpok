using EazyEngine.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{
    public class LayerContainerMision : BaseNormalBox<ItemMission,ItemMissionObject>
    {
        private void OnEnable()
        {
            executeInfos(GameDatabase.Instance.missionContainer.ToArray());
        }

        public void startMission(object pObject)
        {
  
            ES3.Save("WatchADSMission", false);
            ItemMissionObject mission = (ItemMissionObject)pObject;
            TimeCounter.Instance.addTimer(new TimeCounterInfo() { id = $"[Mission]{mission.ItemID}", destinyIfHave = mission.timeMission });
            GetComponentInParent<BoxManagerMission>().layerProcessing.show();
            GetComponent<UIElement>().close();
            EzEventManager.TriggerEvent(new MissionEvent() { type = TypeEvent.ADD });
        }
    }
}

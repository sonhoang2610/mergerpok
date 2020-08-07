using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;

namespace Pok
{
    public class BoxManagerMission : MonoBehaviour,EzEventListener<MissionEvent>
    {

        public UIElement layerStart,layerProcessing,layerContainer;

        private void OnEnable()
        {
            StartCoroutine(init());
            EzEventManager.AddListener(this);
        }

        private void OnDisable()
        {
            EzEventManager.RemoveListener(this);
        }

        public IEnumerator init()
        {
            yield return new WaitForEndOfFrame();
           var processing = TimeCounter.Instance.timeCollection.Value.Exists(x => x.id.Contains("[Mission]"));
            layerStart.setActive(!processing);
            layerProcessing.setActive(processing);
            layerContainer.setActive(false);
        }

        public void OnEzEvent(MissionEvent eventType)
        {
            if(eventType.type == TypeEvent.REMOVE)
            {
                layerContainer.setActive(true);
                layerProcessing.setActive(false);
            }
        }
    }
}

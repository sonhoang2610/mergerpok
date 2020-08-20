using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;

namespace Pok
{
    public enum ConditionType
    {
        POK_UNLOCK
    }
    [System.Serializable]
    public class ObjectToCheckInfo
    {
        public GameObject checkObject;
        public ConditionType typeCondition;
        public string goalPok;
        public bool forAllZone = false;
        public void checkCondition()
        {
            if(GameManager.Instance.ZoneChoosed != "Zone1" && !forAllZone)
            {
                HUDManager.Instance.ActiveObject(checkObject, typeCondition.ToString(), true);
                return;
            }
            var zone = GameManager.Instance.Database.zoneInfos.Find(x => x.id == GameManager.Instance.ZoneChoosed);
            if (string.IsNullOrEmpty(zone.curentUnlock))
            {
                HUDManager.Instance.ActiveObject(checkObject, typeCondition.ToString(), false);
            }
            else
            {
                string index = zone.curentUnlock.Replace("Pok", "");
                HUDManager.Instance.ActiveObject(checkObject, typeCondition.ToString(),int.Parse( index )>= int.Parse(goalPok.Replace("Pok","")));
            }
          
        }
        public bool checkBoolCondition()
        {

            var zone = GameManager.Instance.Database.zoneInfos.Find(x => x.id == GameManager.Instance.ZoneChoosed);
            if (string.IsNullOrEmpty(zone.curentUnlock))
            {
                return false;
            }
            else
            {
                string index = zone.curentUnlock.Replace("Pok", "");
                return int.Parse(index) >= int.Parse(goalPok.Replace("Pok", ""));
            }

        }
    }
    public class CheckActiveObject : MonoBehaviour,EzEventListener<AddCreatureEvent>
    {
        public ObjectToCheckInfo[] checkObjects;
        private void OnEnable()
        {
            StartCoroutine(onEnable());
            EzEventManager.AddListener(this);
        }

        private void OnDisable()
        {
            EzEventManager.RemoveListener(this);
        }

        public IEnumerator onEnable()
        {
            while (GameManager.readyForThisState("Main"))
            {
                yield return new WaitForEndOfFrame();
            }
            foreach(var checkObject in checkObjects)
            {
                checkObject.checkCondition();
            }
        }

        public void OnEzEvent(AddCreatureEvent eventType)
        {
            foreach (var checkObject in checkObjects)
            {
                checkObject.checkCondition();
            }
        }
    }
}

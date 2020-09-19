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
            var zone = GameManager.Instance.Database.zoneInfos.Find(x => x.Id == GameManager.Instance.ZoneChoosed);
            if (string.IsNullOrEmpty(zone.CurentUnlock))
            {
                HUDManager.Instance.ActiveObject(checkObject, typeCondition.ToString(), false);
            }
            else
            {
                 string index = zone.CurentUnlock.Replace("Pok", "");
                 HUDManager.Instance.ActiveObject(checkObject, typeCondition.ToString(), !string.IsNullOrEmpty(index) && int.Parse(index) >= int.Parse(goalPok.Replace("Pok", "")));
                
            }
          
        }
        public bool checkBoolCondition()
        {

            var zone = GameManager.Instance.Database.zoneInfos.Find(x => x.Id == GameManager.Instance.ZoneChoosed);
            if (string.IsNullOrEmpty(zone.CurentUnlock))
            {
                return false;
            }
            else
            {
                if (!string.IsNullOrEmpty(zone.CurentUnlock))
                {
                    string index = zone.CurentUnlock.Replace("Pok", "");
                    return int.Parse(index) >= int.Parse(goalPok.Replace("Pok", ""));
                }
                return false;
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

        public void reload()
        {
            foreach (var checkObject in checkObjects)
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

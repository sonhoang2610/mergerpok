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
        public void checkCondition()
        {
            var creature = GameManager.Instance.Database.creatureInfos.Find(x => x.id == goalPok);
            HUDManager.Instance.ActiveObject(checkObject, typeCondition.ToString(), creature != null && creature.isUnLock);
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using EazyEngine.Tools;
using System;

namespace Pok
{
    public class TimeListener : MonoBehaviour, EzEventListener<TimeEvent>,EzEventListener<RemoveTimeEvent>
    {
        public string listenID;
        public UILabel timeLbl;
        public UnityEvent onTimeOut;

        public void OnEzEvent(TimeEvent eventType)
        {
            if (eventType.timeInfo.id == listenID)
            {
                var sec = eventType.timeInfo.destinyIfHave - eventType.timeInfo.CounterTime; 
                if(sec < 0)
                {
                    sec = 0;
                }
                var timeSpan = TimeSpan.FromSeconds(sec);
                
                timeLbl.text = string.Format("{0}H {1}M {2}S", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            }
        }

        public void OnEzEvent(RemoveTimeEvent eventType)
        {
            if (eventType.timeInfo.id == listenID)
            {
                onTimeOut.Invoke();
            }
        }

        private void OnEnable()
        {
            EzEventManager.AddListener<TimeEvent>(this);
            EzEventManager.AddListener<RemoveTimeEvent>(this);
        }
        private void OnDisable()
        {
            EzEventManager.RemoveListener<TimeEvent>(this);
            EzEventManager.RemoveListener<RemoveTimeEvent>(this);
        }
    }
}

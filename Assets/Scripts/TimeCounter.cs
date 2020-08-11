using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;
using System;
using LitJson;
using Sirenix.OdinInspector;

namespace Pok
{
    [System.Serializable]
    public class TimeCounterInfo
    {
        public string id;
        [System.NonSerialized]
        public double firstTimeAdd;
        public double destinyIfHave = -1;
        public double counterTime;
        public double changeTime;
        public bool pause = false;

        public void pauseTime(bool pBool)
        {
            pause = pBool;
        }
        [System.NonSerialized]
        public System.Action<TimeEvent> onUpdateTime;
        [ShowInInspector]
        public double CounterTime
        {
            get => counterTime; set
            {
                changeTime = value - counterTime;
                counterTime = value;
                //if (changeTime >= 1)
                //{
                    onUpdateTime?.Invoke(new TimeEvent() { timeInfo = this });
                    EzEventManager.TriggerEvent(new TimeEvent() { timeInfo = this });
                //}
            }
        }
    }
    public enum TimeType
    {
        RestoreCreature,
        RestoreInventory
    }
    public struct TimeEvent
    {
        public TimeCounterInfo timeInfo;
    }
    public struct RemoveTimeEvent
    {
        public TimeCounterInfo timeInfo;
    }
    public class TimeCounter : PersistentSingleton<TimeCounter>
    {
        private static double counterValue;
        private DateTime firstTime;
        private bool getRealTime = false, minimize = false, addTimeAfk = false;
        private System.DateTime lasTimeAFK;
        public TimeCounterInfoCollection timeCollection;
        public float minimizeTime { get; set; }
        public static double CounterValue { get => counterValue; set => counterValue = value; }

        public void addTimer(TimeCounterInfo info)
        {
            if (!timeCollection.Value.Exists(x => x.id == info.id))
            {
                GameManager.Instance.Database.addTime(info);
                info.firstTimeAdd = CounterValue;
                timeCollection.Add(info);
            }
            else
            {
                timeCollection.Value.Find(x => x.id == info.id).destinyIfHave = info.destinyIfHave;
            }
        }

        public bool registerUpdateTime(string id, System.Action<TimeEvent> update)
        {
            var time = timeCollection.Value.Find(x => x.id == id);
            if (time != null)
            {
                time.onUpdateTime += update;
                return true;
            }
            return false;
        }
        public bool unRegisterUpdateTime(string id, System.Action<TimeEvent> update)
        {
            var time = timeCollection.Value.Find(x => x.id == id);
            if (time != null)
            {
                time.onUpdateTime -= update;
                return true;
            }
            return false;
        }
        protected override void Awake()
        {
            base.Awake();
            Application.runInBackground = true;
            CounterValue = 0;
            firstTime = System.DateTime.Now;
            GameManager.addDirtyState("Main");
            StartCoroutine(TimeExtension.GetNetTime((time, error) =>
           {
                // string.IsNullOrEmpty(error)
                getRealTime = true;
               var pJson = JsonMapper.ToObject(time);
               firstTime = TimeExtension.UnixTimeStampToDateTime(double.Parse(JsonMapper.ToObject(pJson["data"].ToJson())["timestamp"].ToJson())).ToLocalTime();
               var lastime = ES3.Load<TimeModule>("LastTimeGame", defaultValue: null);
               if (lastime != null)
               {
                   lasTimeAFK = lastime.time;
                   for (int i = 0; i < timeCollection.Count; ++i)
                   {
                       var timeElement = timeCollection[i];
                       timeElement.counterTime += (firstTime - lasTimeAFK).TotalSeconds;
                   }
        
               }
               GameManager.Instance.Database.checkTimeForAll();
               GameManager.removeDirtyState("Main");
           }));
        }
        public bool getCurrentTime(ref DateTime time)
        {
            time = firstTime.AddSeconds(CounterValue);
            return getRealTime;
        }
        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                minimizeTime = 0;
                minimize = true;
            }
            else
            {
                minimize = false;
            }
        }

        private void OnDestroy()
        {
            System.DateTime time = DateTime.Now;
            if (getCurrentTime(ref time))
            {
                ES3.Save<TimeModule>("LastTimeGame", new TimeModule() { time = time });
            }
            else
            {
                ES3.DeleteKey("LastTimeGame");
            }
        }
        private void OnApplicationQuit()
        {
            System.DateTime time = DateTime.Now;
            if (getCurrentTime(ref time))
            {
                ES3.Save<TimeModule>("LastTimeGame", new TimeModule() { time = time });
            }
            else
            {
                ES3.DeleteKey("LastTimeGame");
            }
        }
        public void saveLastTime()
        {

        }
        private void LateUpdate()
        {
            if (!GameManager.readyForThisState("Main")) return;
            CounterValue += Time.deltaTime;
            if (!minimize)
            {
                for (int i = 0; i < timeCollection.Count; ++i)
                {
                    var time = timeCollection[i];
                    if (!time.pause)
                    {
                        time.CounterTime = CounterValue - time.firstTimeAdd;
                    }
                    else
                    {
                        time.firstTimeAdd = CounterValue - time.CounterTime;
                    }
                }
            }
            else
            {
                minimizeTime += Time.deltaTime;
            }
        }
    }
}

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
        [System.NonSerialized]
        public System.Action<TimeEvent> onUpdateTime;
        [ShowInInspector]
        public double CounterTime { get => counterTime; set {
                counterTime = value;
                onUpdateTime?.Invoke(new TimeEvent() { timeInfo = this});
                EzEventManager.TriggerEvent(new TimeEvent() {timeInfo = this });
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
    public class TimeCounter : PersistentSingleton<TimeCounter>
    {
        private static double counterValue;
        private DateTime firstTime;
        private double minimizedSeconds;
        private bool getRealTime = false, minimize = false,addTimeAfk = false;
        private System.DateTime lasTimeAFK;
        public TimeCounterInfoCollection timeCollection;

        public static double CounterValue { get => counterValue; set => counterValue = value; }

        public void addTimer(TimeCounterInfo info)
        {
            timeCollection.Add(info);
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
            StartCoroutine( TimeExtension.GetNetTime((time, error) =>
            {
                GameManager.removeDirtyState("Main");
                getRealTime = true;
                var pJson = JsonMapper.ToObject(time);
                firstTime = TimeExtension.UnixTimeStampToDateTime(double.Parse(JsonMapper.ToObject(pJson["data"].ToJson())["timestamp"].ToJson())).ToLocalTime();
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
                minimize = true;
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
            else
            {
                minimize = false;
                var time = ES3.Load<TimeModule>("LastTimeGame", defaultValue: null);
                if (time != null && minimizedSeconds == 0)
                {
                    lasTimeAFK = time.time;
                    addTimeAfk = true;
                }
                minimizedSeconds = 0;
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
        private void Update()
        {
            CounterValue += Time.deltaTime;
            if (getRealTime && addTimeAfk)
            {
                addTimeAfk = false;
                CounterValue += (double)((firstTime - lasTimeAFK).TotalSeconds);
            }
            if (!minimize)
            {
                for (int i = 0; i < timeCollection.Count; ++i)
                {
                    var time = timeCollection[i];
                        time.CounterTime = CounterValue - time.firstTimeAdd;
                }
            }
        }
    }
}

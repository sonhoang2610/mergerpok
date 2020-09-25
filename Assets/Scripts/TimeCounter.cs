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
        public bool autoRemoveIfToDestiny = false;
        public bool resetOnStart = false;
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
        private DateTime firstTime;
        private bool getRealTime = false, minimize = false, addTimeAfk = false;
        private System.DateTime lasTimeAFK;
        public TimeCounterInfoCollection timeCollection;
        public double minimizeTime { get; set; }
        public static double CounterValue { get => Time.realtimeSinceStartup; }

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
            breakTime = 120;
            firstTime = System.DateTime.Now;
            GameManager.addDirtyState("Main");
            StartCoroutine(TimeExtension.GetNetTime((time, error) =>
           {
               if (string.IsNullOrEmpty(error))
               {
                   getRealTime = true;
                   var pJson = JsonMapper.ToObject(time);
                   firstTime = TimeExtension.UnixTimeStampToDateTime(double.Parse(pJson["now"].ToJson())).ToLocalTime();
                   var lastime = ES3.Load<TimeModule>("LastTimeGame", defaultValue: null);
                   if (lastime != null)
                   {
                       lasTimeAFK = lastime.time;
                       for (int i = 0; i < timeCollection.Count; ++i)
                       {
                           var timeElement = timeCollection[i];
                           timeElement.firstTimeAdd -= (firstTime - lasTimeAFK).TotalSeconds;
                       }
                       if ((firstTime - lasTimeAFK).TotalSeconds > GameDatabase.Instance.timeAFKShowBoxTreasure)
                       {
                           StartCoroutine(timingShowBoxTreaure());
                       }
                   }
               }
               else
               {
                   StartCoroutine(tryGetRealTime());
               }
               for(int i = timeCollection.Value.Count -1; i >=0 ; --i)
               {
                   timeCollection.Value[i].firstTimeAdd -= timeCollection.Value[i].counterTime > 0? timeCollection.Value[i].counterTime  : 0;
                   if (timeCollection.Value[i].resetOnStart)
                   {
                       GameManager.Instance.Database.removeTime(timeCollection.Value[i]);
                   }
             
               }
               GameManager.Instance.Database.checkTimeForAll();
               GameManager.removeDirtyState("Main");
           }));
            StartCoroutine(UpdateLastTimeInGame());
        }

        public IEnumerator tryGetRealTime()
        {
            yield return new WaitForSeconds(6);
            yield return TimeExtension.GetNetTime((time, error) =>
            {
                if (!string.IsNullOrEmpty(error))
                {
                    StartCoroutine(tryGetRealTime());
                }
                else
                {
                    getRealTime = true;
                    var pJson = JsonMapper.ToObject(time);
                    firstTime = TimeExtension.UnixTimeStampToDateTime(double.Parse(pJson["now"].ToJson())).ToLocalTime().AddSeconds(-Time.realtimeSinceStartup);
                }
            });
        }

        public IEnumerator timingShowBoxTreaure()
        {
            while (!GameManager.readyForThisState("Main"))
            {
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
            MainScene.Instance.showBoxTreasure();
        }
        public bool getCurrentTime(ref DateTime time)
        {
            time = firstTime.AddSeconds(CounterValue);
            return getRealTime;
        }
        [System.NonSerialized]
        public float breakTime = 0;
        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                minimizeTime = CounterValue;
                minimize = true;
                breakTime = 120;
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = 1;
                if (focus)
                {
                    if (GameManager.Instance.IsShowingADS)
                    {
                        GameManager.Instance.IsShowingADS = false;
                    }
                }
                //var time = TimeCounter.Instance.timeCollection.Value.Find(x => x.id.Contains("[Block]SwitchAppADS"));
                //if (time == null && (CounterValue - GameManager.lastTimeShowAds) > 120)
                //{
                //    GameManager.Instance.IsShowingADS = true;
                //    GameManager.Instance.StartCoroutine(GameManager.Instance.delayAction(1, () =>
                //    {
                //        TimeCounter.Instance.addTimer(new TimeCounterInfo() { id = $"[Block]SwitchAppADS", autoRemoveIfToDestiny = true, destinyIfHave = GameManager.Instance.TimeDelayADSSwitchApp, resetOnStart = true });
                //        if (EasyMobile.Advertising.IsInterstitialAdReady())
                //        {
                //            EasyMobile.Advertising.ShowInterstitialAd();
                //        }
                //        else
                //        {
                //            GameManager.Instance.IsShowingADS = false;
                //        }
                //    }));
                
                //}
                minimize = false;
                breakTime = 120;
            }
        }
        public IEnumerator UpdateLastTimeInGame()
        {
            yield return new WaitForSeconds(1);
            System.DateTime time = DateTime.Now;
            if (getCurrentTime(ref time))
            {
                ES3.Save<TimeModule>("LastTimeGame", new TimeModule() { time = time });
            }
            else
            {
                ES3.DeleteKey("LastTimeGame");
            }
            StartCoroutine(UpdateLastTimeInGame());
        }

        public void saveLastTime()
        {

        }
        private void LateUpdate()
        {
            if (!GameManager.readyForThisState("Main")) return;
            if (!minimize)
            {
                if (breakTime > 0)
                {
                    breakTime -= Time.deltaTime;
                }
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
        }
    }
}

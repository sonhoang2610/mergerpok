using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;

namespace Pok
{
    public enum TypeEvent
    {
        ADD,REMOVE
    }
    public struct MissionEvent
    {
        public TypeEvent type;
    }
    public class ItemMission : BaseItem<ItemMissionObject>, EzEventListener<TimeEvent>, EzEventListener<MissionEvent>
    {
    
        public UI2DSprite icon;
        public UILabel nameMission;
        public UILabel time;
        public PaymentUI[] reward;
        public GameObject containerTime;
        public GameObject notifiDone;

        public bool instancedMission = false;
        public UIButton btnClaim,btnWatchADS;
        public Color colorTextActive, colorTextDeactive;

        protected ItemRewardInfo[] cacheReward;
        public override void setInfo(ItemMissionObject pInfo)
        {
            if(_info != pInfo)
            {
                var listReward = new List<ItemRewardInfo>();
                for (int i = 0; i < pInfo.itemReward.Length; ++i)
                {
                    var extraItem = ((IExtractItem)pInfo.itemReward[i].item).ExtractHere();
                    listReward.Add(new ItemRewardInfo() { itemReward = extraItem[0], iconOverride = pInfo.itemReward[i].item.icons[0].Icon });
                }
                cacheReward = listReward.ToArray();
            }
            base.setInfo(pInfo);
         
            if (nameMission)
            {
                nameMission.text = pInfo.displayNameItem.Value;
            }
            if (instancedMission)
            {
                if(btnWatchADS)
                  btnWatchADS.isEnabled = !ES3.Load("WatchADSMission", false);
                var timing = TimeCounter.Instance.timeCollection.Value.Find(x => x.id.Contains(pInfo.ItemID));
                if (timing != null)
                {
                    if (btnClaim)
                    {
                        btnClaim.isEnabled = timing.CounterTime >= timing.destinyIfHave;
                        btnClaim.GetComponentInChildren<UILabel>().color = timing.CounterTime >= timing.destinyIfHave ? colorTextActive : colorTextDeactive;
                    }
                 
                    var sec = timing.destinyIfHave - timing.CounterTime;
                    if (sec < 0)
                        sec = 0;
                    var timespan = TimeSpan.FromSeconds(sec);
                    time.text = string.Format("{0}H {1}M {2}S", timespan.Hours, timespan.Minutes, timespan.Seconds);
                    if (sec > 0)
                    {
                        if(sec > 0)
                        {
                            if (containerTime)
                            {
                                containerTime.gameObject.SetActive(true);
                            }
                            if (notifiDone)
                            {
                                notifiDone.gameObject.SetActive(false);
                            }
                        }
                        else
                        {
                            if (containerTime)
                            {
                                containerTime.gameObject.SetActive(false);
                            }
                            if (notifiDone)
                            {
                                notifiDone.gameObject.SetActive(true);
                            }
                        }
                    }
                }
            }
            else
            {
      
                var timespan = TimeSpan.FromSeconds(pInfo.timeMission);
                time.text = string.Format("{0}H {1}M {2}S", timespan.Hours, timespan.Minutes, timespan.Seconds);
            }
            for (int i = 0; i < reward.Length; ++i)
            {
                if (i >= pInfo.itemReward.Length)
                {
                    reward[i].container.gameObject.SetActive(false);
                }
                else
                {
                    reward[i].container.gameObject.SetActive(true);
                    reward[i].price.text = cacheReward[i].itemReward.quantity.clearDot().ToKMBTA();
                    var icon = reward[i].icon;
                    pInfo.itemReward[i].item.getSpriteForState((o) =>
                    {
                        icon.sprite2D = o;
                    });
                }
            }
            if (reward.Length > 0)
            {
                reward[0].container.transform.parent.GetComponent<UITable>().Reposition();
            }
        }

        public void claimMission()
        {
            TimeCounter.Instance.timeCollection.Value.RemoveAll(x => x.id == $"[Mission]{_info.ItemID}");
            GameManager.Instance.Database.timeRestore.RemoveAll(x => x.id == $"[Mission]{_info.ItemID}");
            EzEventManager.TriggerEvent(new MissionEvent() { type = TypeEvent.REMOVE });
            HUDManager.Instance.boxReward.show(cacheReward,"Mission Reward");
            foreach(var reward in cacheReward)
            {
                if(reward.itemReward.item.categoryItem == CategoryItem.CREATURE)
                {
                    var creature = ((CreatureItem)reward.itemReward.item);
                    var zone = GameDatabase.Instance.ZoneCollection.Find(x => x.ItemID == GameManager.Instance.ZoneChoosed);
                    var pack = zone.getPackage();
                    var mapParent = GameManager.Instance.Database.worldData.zones.Find(x => x.id == GameManager.Instance.ZoneChoosed).maps.Find(x => creature.parentMap && x.id == creature.parentMap.ItemID);
                    var newCreature = new PackageCreatureInstanceSaved() { creature = creature.ItemID, id = pack.package.ItemID, instanceID = GameManager.Instance.GenerateID.ToString(), mapParent = mapParent };
                    GameManager.Instance.GenerateID++;
                    GameManager.Instance.Database.worldData.addCreature(newCreature, GameManager.Instance.ZoneChoosed);
                    EzEventManager.TriggerEvent(new AddCreatureEvent() { change = 1, creature = newCreature, zoneid = GameManager.Instance.ZoneChoosed, manualByHand = false });
                }
                else
                {
                    GameManager.Instance.claimItem(reward.itemReward.item, reward.itemReward.quantity);
                }
                
            }
          

        }
        private void OnEnable()
        {
            StartCoroutine(setup());
        }
        public IEnumerator setup()
        {
            yield return new WaitForEndOfFrame();
            if (instancedMission)
            {
                EzEventManager.AddListener<TimeEvent>(this);
                EzEventManager.AddListener<MissionEvent>(this);
                checkMission();
            }
        }
        public void checkMission()
        {
            var timing = TimeCounter.Instance.timeCollection.Value.Find(x => x.id.Contains("[Mission]"));
            if (timing != null)
            {
                string id = timing.id.Remove(0, ("[Mission]").Length);
                setInfo(GameDatabase.Instance.missionContainer.Find(x => x.ItemID == id));
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            EzEventManager.RemoveListener<TimeEvent>(this);
            EzEventManager.RemoveListener<MissionEvent>(this);
        }
        public void wathADS()
        {
            var timing = TimeCounter.Instance.timeCollection.Value.Find(x => x.id.Contains(_info.ItemID));
            if (timing != null)
            {
                GameManager.Instance.WatchRewardADS("", (o) =>
                 {
                     if (o)
                     {
                         ES3.Save("WatchADSMission", true);
                         timing.firstTimeAdd -= 30 * 60;
                     }
                 });
            }
        }

        public void OnEzEvent(TimeEvent eventType)
        {
            string id = eventType.timeInfo.id.Remove(0, ("[Mission]").Length);
            if (_info != null && id == _info.ItemID)
            {
                var sec = eventType.timeInfo.destinyIfHave - eventType.timeInfo.CounterTime;
                if (sec < 0)
                    sec = 0;
                var timespan = TimeSpan.FromSeconds(sec);
                time.text = string.Format("{0}H {1}M {2}S", timespan.Hours, timespan.Minutes, timespan.Seconds);
                if (sec == 0)
                {
                    if (btnClaim)
                    {
                        btnClaim.isEnabled = eventType.timeInfo.CounterTime >= eventType.timeInfo.destinyIfHave;
                        btnClaim.GetComponentInChildren<UILabel>().color = eventType.timeInfo.CounterTime >= eventType.timeInfo.destinyIfHave ? colorTextActive : colorTextDeactive;
                    }
                }
                //if (sec > 0)
                {
                    if (sec > 0)
                    {
                        if (containerTime)
                        {
                            containerTime.gameObject.SetActive(true);
                        }
                        if (notifiDone)
                        {
                            notifiDone.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        if (containerTime)
                        {
                            containerTime.gameObject.SetActive(false);
                        }
                        if (notifiDone)
                        {
                            notifiDone.gameObject.SetActive(true);
                        }
                    }
                }
            }
        }

        public void OnEzEvent(MissionEvent eventType)
        {
            if (eventType.type == TypeEvent.ADD)
            {
                checkMission();
            }
            else
            {
                if (containerTime)
                {
                    containerTime.gameObject.SetActive(false);
                }
                if (notifiDone)
                {
                    notifiDone.gameObject.SetActive(false);
                }
            }
      
        }
    }
}

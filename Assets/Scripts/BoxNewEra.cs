using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;

namespace Pok
{
    public class BoxNewEra : Singleton<BoxNewEra>
    {
        public UIElement container;
        public GameObject attachMent;
        public BoxMapInZone boxMap;
        public UILabel nameEra;

        public void show(string leader)
        {
            var zoneInfo = GameManager.Instance.Database.zoneInfos.Find(x => x.id == GameManager.Instance.ZoneChoosed);
            List<MapStatusInZoneInfo> maps = new List<MapStatusInZoneInfo>();
            var currentMap = GameDatabase.Instance.treeCreature.Find(x => x.creatureLeader.ItemID == leader);
            zoneInfo.leaderSelected.Add(currentMap.map.ItemID, leader);
            for (int i = 0; i < GameDatabase.Instance.MapCollection.Count; ++i)
            {
                maps.Add(new MapStatusInZoneInfo()
                {
                    current = currentMap.map.ItemID == GameDatabase.Instance.MapCollection[i].ItemID,
                    id = GameDatabase.Instance.MapCollection[i].ItemID,
                    isUnlock = GameManager.Instance.Database.getAllMapActiveInZone(GameManager.Instance.ZoneChoosed).Exists(x => x.id == GameDatabase.Instance.MapCollection[i].ItemID),
                    leader = zoneInfo.leaderSelected.ContainsKey(GameDatabase.Instance.MapCollection[i].ItemID) ? (zoneInfo.leaderSelected[GameDatabase.Instance.MapCollection[i].ItemID]) : ""
                });
            }
            nameEra.text = currentMap.className.Value;
            boxMap.executeInfos(maps.ToArray());
            currentMap.map.getModelForState((o) => { attachMent.AddChild(o); },"[Preview]"+currentMap.creatureLeader.ItemID);
            container.show();
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
using EazyEngine.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok {
    public class BoxBonusEvolutionPok : MonoBehaviour
    {
        public ItemCreatureUI item1, item2;
        public UIButton btnGet;

        protected CreatureItem creatureEvo;
        protected CreatureInstanceSaved _currentPok;
        public void showBoxEvo(CreatureItem from, CreatureItem to,CreatureInstanceSaved currentPok)
        {
            creatureEvo = to;
            GetComponent<UIElement>().show();
            _currentPok = currentPok;
 
            item1.setInfo(from);
            item2.setInfo(to);
        }
        private void OnEnable()
        {
            //var creature1 = GameDatabase.Instance.CreatureCollection[Random.Range(0, 72)];
            //var creature2 = GameDatabase.Instance.CreatureCollection[Random.Range(0, 72)];
            //showBoxEvo(creature1, creature2, null);
            btnGet.isEnabled = GameManager.Instance.isRewardADSReady("EvoPok");
            if (!btnGet.isEnabled)
            {
                loadAds();
            }

        }
        public void loadAds()
        {
            GameManager.Instance.LoadRewardADS("EvoPok", (o) => {
                if (!gameObject.activeSelf) return;
                if (o)
                {
                    btnGet.isEnabled = true;
                }
                else
                {
                    btnGet.isEnabled = false;
                    if (gameObject.activeSelf)
                    {
                        loadAds();
                    }
                }
            });
        }
        // Update is called once per frame
        void Update()
        {

        }

        public void get()
        {
            GetComponent<UIElement>().close();
            GameManager.Instance.WatchRewardADS("EvoPok", (o) =>
            {
                if (o)
                {
                    var creature = creatureEvo;
                    var zone = GameDatabase.Instance.ZoneCollection.Find(x => x.ItemID == GameManager.Instance.ZoneChoosed);
                    var pack = zone.getPackage();
                    var mapParent = GameManager.Instance.Database.worldData.zones.Find(x => x.id == GameManager.Instance.ZoneChoosed).maps.Find(x => creature.parentMap && x.id == creature.parentMap.ItemID);
                    var newCreature = new PackageCreatureInstanceSaved() { creature = creature.ItemID, id = pack.package.ItemID, instanceID = GameManager.Instance.GenerateID.ToString(), mapParent = mapParent };
                    GameManager.Instance.GenerateID++;
                    GameManager.Instance.Database.worldData.addCreature(newCreature, GameManager.Instance.ZoneChoosed);
                    EzEventManager.TriggerEvent(new AddCreatureEvent()
                    {
                        change = 1,
                        creature = newCreature,
                        zoneid = GameManager.Instance.ZoneChoosed,
                        manualByHand = false,
                    });
                    DestroyCreature(_currentPok);
                }
            });
        }
        public void DestroyCreature(CreatureInstanceSaved  instancedID)
        {
            var maps = FindObjectsOfType<MapLayer>();
            foreach(var map in maps)
            {
                if(map._info.id == instancedID.mapParent.id)
                {
                    map.DestroyCreature(instancedID);
                }
            }
        }
    }
}

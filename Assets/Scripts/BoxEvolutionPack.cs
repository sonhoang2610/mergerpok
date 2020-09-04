using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyMobile;
using EazyEngine.Tools;

namespace Pok
{
    public class BoxEvolutionPack : MonoBehaviour
    {
        public string packID;
        public UILabel pricelbl;
        private void OnEnable()
        {
           var localize = InAppPurchasing.GetProductLocalizedData(packID.ToLower());
            if(localize != null)
            {
                pricelbl.text = localize.localizedPriceString;
            }
        }

        public void buy()
        {
            GameManager.Instance.RequestInappForItem(packID.ToLower(), (o) =>
            {
                if (o)
                {
                    ES3.Save<bool>("Evolution" + GameManager.Instance.ZoneChoosed, true);
                    var zone = GameManager.Instance.Database.zoneInfos.Find(x => x.Id == GameManager.Instance.ZoneChoosed);
                    for(int i = 0; i < 12; i++)
                    {
                        if (!zone.creatureAdded.Contains("Pok" + (i + 1).ToString()))
                        {
                            zone.creatureAdded.Add("Pok" + (i + 1).ToString());
                        }
                    }
                   var childs = GameDatabase.Instance.CreatureCollection.Find(x => x.ItemID == "Pok12").creatureChilds;
                    BoxSelectEra.Instance.show(childs);
                    var nextMapInfo = GameManager.Instance.Database.worldData.zones.Find(x => x.id == GameManager.Instance.ZoneChoosed).maps[2];
                    var midMap = GameManager.Instance.Database.worldData.zones.Find(x => x.id == GameManager.Instance.ZoneChoosed).maps[1];
                    System.Action<string> addAnother = delegate (string idCreature)
                    {
                        var newCreatureItem = GameDatabase.Instance.CreatureCollection.Find(x => x.ItemID == idCreature);
                        if (newCreatureItem.creatureChilds.Length == 0 && nextMapInfo.creatures.Count > 0)
                        {
                            var newCreature = new CreatureInstanceSaved() { id = idCreature, instanceID = GameManager.Instance.GenerateID.ToString(), mapParent = nextMapInfo };
                            nextMapInfo.creatures[0].level++;
                            EzEventManager.TriggerEvent(new AddCreatureEvent() { change = 1, creature = newCreature, manualByHand = true, zoneid = zone.Id });
                        }
                        else
                        {
                            var newCreature = new CreatureInstanceSaved() { id = idCreature, instanceID = GameManager.Instance.GenerateID.ToString(), mapParent = nextMapInfo };
                            //nextMapInfo.creatures.Add(newCreature);
                            GameManager.Instance.Database.worldData.addCreature(newCreature, GameManager.Instance.ZoneChoosed);
                            GameManager.Instance.GenerateID++;
                            EzEventManager.TriggerEvent(new AddCreatureEvent() { change = 1, creature = newCreature, manualByHand = true, zoneid = zone.Id });
                        }
                        HUDManager.Instance.checkEvolutionPack();
                    };
                    BoxSelectEra.Instance._onClose = delegate
                    {
              
                        zone.addLeader(midMap.id, "Pok7");
                        midMap.isUnlocked = true;
                        BoxNewEra.Instance.show(BoxSelectEra.Instance.selectCreature.ItemID);
                        EzEventManager.TriggerEvent(new UnlockNewEra() { nameLeader = BoxSelectEra.Instance.selectCreature.ItemID, mapID = nextMapInfo.id });             
                        addAnother.Invoke(BoxSelectEra.Instance.selectCreature.ItemID);
                        GameManager.Instance.Database.calculateCurrentUnlock(zone.Id);
               
                    };
                    HUDManager.Instance.checkEvolutionPack();
                }
            });
            GetComponent<UIElement>().close();
        }
    }
}

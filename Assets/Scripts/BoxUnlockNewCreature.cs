using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;

namespace Pok
{
    public class BoxUnlockNewCreature : Singleton<BoxUnlockNewCreature>
    {
        public UIElement container;
        public BoxCreatureInMap boxCreatureInMap;
        public UI2DSprite icon;
        public UILabel nameCreature;
        public Vector2Int size;
        public void show(string creautre)
        {
            var mapID = MainScene.Instance.MapObjects[MainScene.Instance.CurrentPageMapLayer].id;
           var creatures = GameManager.Instance.Database.getAllInfoCreatureInAddress(GameManager.Instance.ZoneChoosed, mapID);
           var zoneInfo =  GameManager.Instance.Database.zoneInfos.Find(x => x.id == GameManager.Instance.ZoneChoosed);
           var creatureOriginal = GameDatabase.Instance.CreatureCollection.Find(x => x.ItemID == creautre);
            creatureOriginal.getSpriteForState((o) =>
            {
                icon.sprite2D = o;
                icon.MakePixelPerfectClaimIn(size);
            });
            nameCreature.text = creatureOriginal.displayNameItem.Value;
            List<CreatureInfoSatus> infos = new List<CreatureInfoSatus>();
            foreach(var creature in creatures)
            {
                infos.Add(new CreatureInfoSatus() { mainInfo = creature, current = zoneInfo.curentUnlock == creature.id });
            }
            boxCreatureInMap.executeInfos(infos.ToArray());
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

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{
    public class MapStatusInZoneInfo
    {
        public string id;
        public bool isUnlock;
        public bool current;
        public string leader;
    }
    public class ItemStatusMapInZone : BaseItem<MapStatusInZoneInfo>
    {
        public UI2DSprite icon,iconLeader;
        public UI2DSprite statusbar;
        public Vector2Int claim;
        public Color colorBarPrevious, colorBarCurrent, ColorBarlocked;
        public override void setInfo(MapStatusInZoneInfo pInfo)
        {
            base.setInfo(pInfo);
            statusbar.color = pInfo.current ? colorBarCurrent : (pInfo.isUnlock ? colorBarPrevious : ColorBarlocked);
            var originalItem = GameDatabase.Instance.MapCollection.Find(x => x.ItemID == pInfo.id);
           var zone = GameManager.Instance.Database.zoneInfos.Find(x => x.id == GameManager.Instance.ZoneChoosed);
            if (zone.leaderSelected.ContainsKey(pInfo.id))
            {
                icon.gameObject.SetActive(true);
                iconLeader.gameObject.SetActive(true);
                originalItem.getSpriteForState((o) => {
                    icon.sprite2D = o;
                    icon.MakePixelPerfectClaimIn(new Vector2Int(96, 96));
                });
                var leader = GameDatabase.Instance.CreatureCollection.Find(x => x.ItemID == zone.leaderSelected[pInfo.id]);
                leader.getSpriteForState((o) =>
                {
                    iconLeader.sprite2D = o;
                    iconLeader.MakePixelPerfectClaimIn(claim);
                });
                if (!pInfo.isUnlock)
                {
                    icon.color = Color.black;
                }
                else
                {
                    icon.color = Color.white;
                }
            }
            else
            {
                icon.gameObject.SetActive(false);
                iconLeader.gameObject.SetActive(false);
            }
      
   
        }
    }
}

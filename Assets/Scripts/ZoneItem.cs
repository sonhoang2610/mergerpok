using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Pok
{
    public class ZoneItem : BaseItem<ZoneObject>
    {
        public UILabel nameZone;
        public UI2DSprite icon;
        public GameObject uRhere;
        public GameObject layerNotUnlocked;
        public UILabel factorTap;
        public UILabel price;
        public override void setInfo(ZoneObject pInfo)
        {
            base.setInfo(pInfo);
      
            uRhere.gameObject.SetActive(pInfo.ItemID == GameManager.Instance.ZoneChoosed);
            var state = "Default";
            var zoneinfo = GameManager.Instance.Database.zoneInfos.Find(x => x.id == pInfo.ItemID);
            price.text =$"{ pInfo.moneyToUnlock.ToKMBTA()} to unlock this planet" ;
            factorTap.text = $"x{pInfo.factorTap} money for each tap";
            layerNotUnlocked.gameObject.SetActive(!zoneinfo.isUnLock);
            if (!zoneinfo.isUnLock)
            {
                state = "DefaultDisable";
            }
            pInfo.getSpriteForState((o) =>
            {
                icon.sprite2D = o;
                icon.MakePixelPerfectClaimIn(new Vector2Int(icon.width, icon.height));
            }, state);
            nameZone.text = pInfo.displayNameItem.Value;


        }
    }
}

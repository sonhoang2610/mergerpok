using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{
    public class ItemSelectEra : BaseItem<CreatureItem>
    {
        public UI2DSprite icon;
        public Vector2Int claim;
        public UILabel nameEra;
        public override void setInfo(CreatureItem pInfo)
        {
            base.setInfo(pInfo);
          
            nameEra.text = GameDatabase.Instance.treeCreature.Find(x => x.creatureLeader == pInfo).className.Value;
            pInfo.getSpriteForState((o) =>
            {
                icon.sprite2D = o;
                icon.MakePixelPerfectClaimIn(claim);
            });
        }

        public void setEnable(bool pBool)
        {
            icon.color =!pBool ? Color.gray : Color.white;
            GetComponent<UI2DSprite>().color =!pBool ? Color.gray : Color.white;
        }
    }
}
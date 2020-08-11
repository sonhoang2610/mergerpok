using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Pok
{
    public class LayerShopCrystal : BaseLayerShop<SimpleItemShop>
    {
        public EnvelopContent boxOut;
        
        public override float getBonusForItem(ShopItemInfo item)
        {
            return ES3.Load<float>("BonusCrystal", 0);
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            boxOut.Execute();
        }

        public override void reload()
        {
            base.reload();
            _isInitDone = true;
        }
    }
}
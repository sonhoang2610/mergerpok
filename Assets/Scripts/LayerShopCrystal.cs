using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Pok
{
    public class LayerShopCrystal : BaseLayerShop<SimpleItemShop>
    {
        public EnvelopContent boxOut;
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
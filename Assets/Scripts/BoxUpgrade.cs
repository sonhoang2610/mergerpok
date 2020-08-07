using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{
    public class BoxUpgrade : BaseLayerShop<ItemUpgradeUI>
    {
        public UIElement container;
        public void show()
        {
            container.show();
            checkStateCoroutine = StartCoroutine(checkState());
        }

        protected override void OnEnable()
        {
       
        }
    }
}

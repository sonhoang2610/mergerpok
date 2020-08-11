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

        public override void claimItem(BaseItemGame item,float bonus= 0)
        {
            var exist = GameManager.Instance.Database.getItem(item.ItemID);
            if(exist != null)
            {
                exist.CurrentLevel++;
            }
            reload();
        }

        protected override void OnEnable()
        {
       
        }
    }
}

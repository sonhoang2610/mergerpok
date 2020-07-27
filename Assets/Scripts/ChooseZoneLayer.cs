using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{
    public class ChooseZoneLayer : BaseBox<ChooseZoneLayer,ZoneItem,ZoneObject>
    {
        public UIElement container;

        protected override void Awake()
        {
            base.Awake();
            container.gameObject.SetActive(false);
        }

        public void show()
        {
            executeInfos(GameDatabase.Instance.ZoneCollection.ToArray());
            container.show();
        }

        public void onChooseZone(object pZone)
        {
            MainScene.Instance.chooseZone(pZone);
            container.close();
        }
    }
}

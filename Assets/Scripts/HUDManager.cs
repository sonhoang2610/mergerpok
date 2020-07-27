using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;


namespace Pok
{
    public class HUDManager : Singleton<HUDManager>
    {
        public GameObject inGameHud;
        protected override void Awake()
        {
            base.Awake();
            StartCoroutine(init());
        }
        public IEnumerator init()
        {
            yield return new WaitForEndOfFrame();
            var  zone = ChooseZoneLayer.Instance.container.GetComponent<UIElement>();
            zone.onEnableEvent.AddListener(() => {
                inGameHud.gameObject.SetActive(false);
            });
            zone.onDisableEvent.AddListener(() => {
                inGameHud.gameObject.SetActive(true);
            });
        }
        public void showBoxCollectionCreature()
        {
            BoxCreatureCollection.Instance.show();
        }
        public void showChooseZoneLayer()
        {
            ChooseZoneLayer.Instance.show();
        }

        public void showBoxShopCreature()
        {
            BoxShopCreature.Instance.show();
        }

        public void showBoxShopGold()
        {
            BoxShopCommon.Instance.show();
        }
        public void showBoxShopCrystal()
        {
            BoxShopCommon.Instance.show();
        }
        public void showBoxShopBooster()
        {
            BoxShopCommon.Instance.show();
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

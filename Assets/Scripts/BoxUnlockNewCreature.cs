using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;
using DG.Tweening;

namespace Pok
{
    public class BoxUnlockNewCreature : Singleton<BoxUnlockNewCreature>
    {
        public UIElement container;
        public BoxCreatureInMap boxCreatureInMap;
        public UI2DSprite icon;
        public UILabel nameCreature;
        public Vector2Int size;
        public GameObject attachMentDiamond;
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
                infos.Add(new CreatureInfoSatus() { mainInfo = creature, current = zoneInfo.curentUnlock == creature.id,isUnlock = creature.isUnLock });
            }
            boxCreatureInMap.executeInfos(infos.ToArray());
            container.show();
            StartCoroutine( delayDiamond());
            if(creautre == "Pok6")
            {
                if (ES3.Load<bool>("FirstUnlockPok6", true))
                {
                    ES3.Save<bool>("FirstUnlockPok6", false);
                    container.onStartClose.AddListener(showStartedKit);
                   
                }
            }
        }
        public void showStartedKit()
        {
            HUDManager.Instance.boxPackedInapp.showData(GameDatabase.Instance.startedKit);
            container.onStartClose.RemoveListener(showStartedKit);
        }

        public IEnumerator delayDiamond()
        {
            yield return new WaitForSeconds(0.25f);
            var crystal = GameManager.Instance.Database.getItem("Crystal");
            crystal.addQuantity("1");
            var iconPos = attachMentDiamond.transform.position;
            var Home = System.Array.Find(GameObject.FindObjectsOfType<InventorySlot>(), x => x._info.itemID == "Crystal");
            if (Home)
            {
                GameObject pObject = new GameObject();
                var sprite = pObject.AddComponent<UI2DSprite>();
                sprite.width = 100;
                sprite.height = 100;
                var item = GameManager.Instance.Database.getItem("Crystal");
                item.item.getSpriteForState((o) =>
                {
                    sprite.sprite2D = o;
                });
                pObject.transform.parent = Home.transform;
                pObject.SetLayerRecursively(HUDManager.Instance.gameObject.layer);
                pObject.transform.localScale = new Vector3(1, 1, 1);
                pObject.transform.position = iconPos;
                NGUITools.BringForward(pObject);
                Sequence seq = DOTween.Sequence();
                seq.Append(pObject.transform.DOScale(1.5f, 0.75f));
                seq.Append(pObject.transform.DOMove(Home.transform.position, 0.5f));
                seq.Join(pObject.transform.DOScale(0, 0.6f).OnComplete(() =>
                {
                    GameObject.Destroy(pObject);
                }));
            }
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

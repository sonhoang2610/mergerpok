using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;
using DG.Tweening;

namespace Pok
{
    public class MapItemInstanced : BaseItem<MapInstanceSaved>,EzEventListener<AddCreatureEvent>
    {
        public UI2DSprite icon;
        public GameObject notifiFull;

        protected UIWidget root;
        private void Awake()
        {
            root = GetComponent<UIWidget>();
        }
        public override void setInfo(MapInstanceSaved pInfo)
        {
            base.setInfo(pInfo);
            var map = GameDatabase.Instance.MapCollection.Find(x => x.ItemID == pInfo.id);
            var zoneInfo = GameManager.Instance.Database.zoneInfos.Find(x => x.Id == pInfo.zoneParent.id);
            if (map != null)
            {
                map.getSpriteForState((o) =>
                {
                    icon.sprite2D = o;
                },"Icon" + zoneInfo.leaderSelected[pInfo.id]);
            }
            notifiFull.gameObject.SetActive(_info.creatures.Count >= 16);
            //nameMap.
        }

        private void Update()
        {
            if (root)
            {
                if (icon.depth <= root.depth)
                {
                    icon.depth = root.depth;
                }
            }
        }
        private void OnEnable()
        {
            EzEventManager.AddListener(this);
        }
        private void OnDisable()
        {
            EzEventManager.RemoveListener(this);
        }
        public void addCreatureObject(CreatureInstanceSaved pInfo)
        {
            var infosaved = GameManager.Instance.Database.creatureInfos.Find(x => x.id == pInfo.id);
            if (!infosaved.IsUnLock) { infosaved.IsUnLock = true; }
            var dataCreature = GameDatabase.Instance.CreatureCollection.Find(x => x.ItemID == pInfo.id);

            if (dataCreature == null)
            {
                dataCreature = (CreatureItem)GameDatabase.Instance.getItemInventory(pInfo.id);
            }
            dataCreature.getModelForState((o) =>
            {
                var creatureObject = HUDManager.Instance.transform.AddChild(o).GetComponent<Creature>();
                creatureObject.gameObject.SetLayerRecursively(gameObject.layer);
                creatureObject.transform.localScale = o.transform.localScale;
                creatureObject.gameObject.SetActive(true);
                creatureObject.transform.localPosition = Vector3.zero;
                creatureObject.setInfo(pInfo);
                var seq = DOTween.Sequence();
                seq.Append( creatureObject.transform.DOScale(2, 0.5f).SetEase(Ease.OutQuart));
                seq.Append(creatureObject.transform.DOScale(0.2f, 0.5f));
                seq.Join( creatureObject.transform.DOMove(transform.position, 0.5f));
                seq.AppendCallback(delegate
                {
                    Destroy(creatureObject.gameObject);
                });
            });

        }

        public void OnEzEvent(AddCreatureEvent eventType)
        {
            if (eventType.manualByHand  && eventType.change == 1 && MainScene.Instance.MapObjects[MainScene.Instance.CurrentPageMapLayer] != _info) 
            {
                if (eventType.creature.mapParent.id == _info.id)
                {
                    addCreatureObject(eventType.creature);
                }
            }
            notifiFull.gameObject.SetActive(_info.creatures.Count >= 16);
        }
    }
}

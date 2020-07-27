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

        public override void setInfo(MapInstanceSaved pInfo)
        {
            base.setInfo(pInfo);
            var map = GameDatabase.Instance.MapCollection.Find(x => x.ItemID == pInfo.id);
            var zoneInfo = GameManager.Instance.Database.zoneInfos.Find(x => x.id == pInfo.zoneParent.id);
            if (map != null)
            {
                map.getSpriteForState((o) =>
                {
                    icon.sprite2D = o;
                },"Icon" + zoneInfo.leaderSelected[pInfo.id]);
            }
            //nameMap.
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
            if (!infosaved.isUnLock) { infosaved.isUnLock = true; }
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
                seq.Append( creatureObject.transform.DOScale(3, 0.5f).SetEase(Ease.OutQuart));
                seq.Append(creatureObject.transform.DOScale(0.2f, 0.5f));
                seq.Join( creatureObject.transform.DOMove(transform.position, 0.5f));
                seq.AppendCallback(delegate
                {
                    creatureObject.gameObject.SetActive(false);
                });
            });

        }

        public void OnEzEvent(AddCreatureEvent eventType)
        {
            if (eventType.manualByHand ) 
            {
                if (eventType.creature.mapParent.id == _info.id)
                {
                    addCreatureObject(eventType.creature);
                }
            }
        }
    }
}

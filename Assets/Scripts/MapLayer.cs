using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;

namespace Pok
{
    public struct UnlockNewEra
    {
        public string nameLeader;
        public string mapID;
    }
    public class MapLayer : BaseItem<MapInstanceSaved>, EzEventListener<AddCreatureEvent>
    {
        public UI2DSprite bg;
        public AlphaController alpha;
        public UIWidget mainZone;
        [System.NonSerialized]
        public CreatureItem[] creatureInMap;
        public GameObject attachMent;
        Dictionary<string, List<Creature>> creatureCache = new Dictionary<string, List<Creature>>();

        public List<Creature> creatureAlive = new List<Creature>();
        public List<Creature> creatureAnimAble = new List<Creature>();

        public void onPressCreature(CreatureInstanceSaved infoCreature, CreatureItem item, Creature creature, bool press)
        {
            if (item.categoryItem == CategoryItem.PACKAGE_CREATURE)
            {
                if (_info != null)
                {
                    var creatureID = ((PackageCreatureInstanceSaved)infoCreature).creature;
                    var newCreature = new CreatureInstanceSaved() { id = creatureID, instanceID = GameManager.Instance.GenerateID.ToString(), mapParent = _info };
                    GameManager.Instance.GenerateID++;
                    _info.creatures.Add(newCreature);
                    addCreatureObject(newCreature, true, (o) => { o.transform.localPosition = creature.transform.localPosition; });
                    DestroyCreature(infoCreature);
                }
            }
            if (!press)
            {
                OnPressUp();
            }
        }
        public void OnPressUp()
        {
            List<Creature> listToCompare = new List<Creature>(creatureAnimAble);
            List<Creature> destroyList = new List<Creature>();
            List<System.Action> queueAction = new List<System.Action>();
            foreach (var creature1 in listToCompare)
            {
                if (!destroyList.Contains(creature1))
                {
                    foreach (var creature2 in listToCompare)
                    {
                        if (creature1._info.id == creature2._info.id && creature1 != creature2 && !destroyList.Contains(creature2) && !destroyList.Contains(creature1))
                        {
                            float distance = Vector2.Distance(creature1.transform.localPosition, creature2.transform.localPosition);
                            if (distance < 400)
                            {
                                var posDes = Vector2.Lerp(creature1.transform.localPosition, creature2.transform.localPosition, 0.5f);
                                var object1 = creature1;
                                var object2 = creature2;
                                object1.transform.DOLocalMove(posDes, 0.25f);
                                object2.transform.DOLocalMove(posDes, 0.25f).OnComplete(delegate
                                {
                                    DestroyCreature(object1._info);
                                    DestroyCreature(object2._info);
                                    var creatureData = GameDatabase.Instance.CreatureCollection.Find(x => x.ItemID == object1._info.id);
                                    var childs = creatureData.creatureChilds;
                                    var zone = GameManager.Instance.Database.zoneInfos.Find(x => x.id == GameManager.Instance.ZoneChoosed);
                                    var creatures = GameManager.Instance.Database.getAllInfoCreatureInAddress(GameManager.Instance.ZoneChoosed, _info.id);
                                    var nextMap = GameDatabase.Instance.MapCollection.FindIndex(x => x.ItemID == _info.id) + 1;
                                    var nextMapID = GameDatabase.Instance.MapCollection[nextMap].ItemID;
                                    var nextMapInfo = GameManager.Instance.Database.worldData.zones.Find(x => x.id == zone.id).maps.Find(x => x.id == nextMapID);
                                    System.Action addAnother = delegate
                                    {
                                        var newCreature = new CreatureInstanceSaved() { id = childs[0].ItemID, instanceID = GameManager.Instance.GenerateID.ToString(), mapParent = nextMapInfo };
                                        nextMapInfo.creatures.Add(newCreature);
                                        GameManager.Instance.GenerateID++;
                                        EzEventManager.TriggerEvent(new AddCreatureEvent() { creature = newCreature, manualByHand = true, zoneid = zone.id });
                                    };
                                    System.Action addNormal = delegate
                                    {
                                        var newCreature = new CreatureInstanceSaved() { id = creatureData.creatureChilds[0].ItemID, instanceID = GameManager.Instance.GenerateID.ToString(), mapParent = _info };
                                        GameManager.Instance.GenerateID++;
                                        _info.creatures.Add(newCreature);
                                        addCreatureObject(newCreature, true, (o) => { o.transform.localPosition = object1.transform.localPosition; });
                                    };
                                    if (zone.curentUnlock == creatureData.ItemID || string.IsNullOrEmpty(zone.curentUnlock))
                                    {
                                        if (creatureData.ItemID == creatures[creatures.Length - 1].id)
                                        {
                                            if (childs.Length > 1)
                                            {
                                                BoxSelectEra.Instance.show(childs);
                                                BoxSelectEra.Instance._onClose = delegate
                                                {
                                                    BoxNewEra.Instance.show(BoxSelectEra.Instance.selectCreature.ItemID);
                                                    EzEventManager.TriggerEvent(new UnlockNewEra() { nameLeader = BoxSelectEra.Instance.selectCreature.ItemID, mapID = GameDatabase.Instance.MapCollection[nextMap].ItemID });
                                                    addAnother.Invoke();
                                                    zone.curentUnlock = creatureData.creatureChilds[0].ItemID;
                                                };
                                            }
                                            else if (childs.Length == 1)
                                            {
                                                BoxNewEra.Instance.show(childs[0].ItemID);
                                                BoxNewEra.Instance.container.onStartClose.AddListener(delegate
                                                {
                                                    addAnother.Invoke();
                                                    zone.curentUnlock = creatureData.creatureChilds[0].ItemID;
                                                });
                                                EzEventManager.TriggerEvent(new UnlockNewEra() { nameLeader = childs[0].ItemID, mapID = GameDatabase.Instance.MapCollection[nextMap].ItemID });
                                            }

                                        }
                                        else
                                        {
                                            addNormal.Invoke();
                                            zone.curentUnlock = creatureData.creatureChilds[0].ItemID;
                                            BoxUnlockNewCreature.Instance.show(zone.curentUnlock);
                                        }
                                    }
                                    else
                                    {
                                        if (creatureData.ItemID != creatures[creatures.Length - 1].id)
                                        {
                                            addNormal.Invoke();
                                        }
                                        else
                                        {
                                            addAnother.Invoke();
                                        }
                                    }
                                });
                                destroyList.Add(object1);
                                destroyList.Add(object2);

                            }
                        }
                    }
                }
            }
            for (int i = 0; i < queueAction.Count; ++i)
            {
                queueAction[i]?.Invoke();
            }
        }
        public void DestroyCreature(CreatureInstanceSaved creature)
        {
            var creatures = creatureAlive.FindAll(x => x._info != null && x._info.instanceID == creature.instanceID);
            if (creatures.Count > 1)
            {
                Debug.LogError("something wrong" + creature.instanceID);
            }
            foreach (var creatureObject in creatures)
            {
                creatureObject.gameObject.SetActive(false);
                creatureAlive.Remove(creatureObject);
                if (creatureAnimAble.Contains(creatureObject))
                {
                    creatureAnimAble.Remove(creatureObject);
                }

            }
            _info.creatures.RemoveAll(x => x.instanceID == creature.instanceID);

        }
        public override void setInfo(MapInstanceSaved pInfo)
        {
            base.setInfo(pInfo);
            var mapObjec = GameDatabase.Instance.MapCollection.Find(x => x.ItemID == pInfo.id);
            mapObjec.getSpriteForState(delegate (Sprite pSprite)
            {
                bg.sprite2D = pSprite;
            }, "BG");
            for (int i = 0; i < creatureAlive.Count; ++i)
            {
                creatureAlive[i].gameObject.SetActive(false);
            }
            creatureAlive.Clear();
            creatureAnimAble.Clear();
            for (int i = 0; i < pInfo.creatures.Count; ++i)
            {
                var creatureObjectData = pInfo.creatures[i];
                addCreatureObject(creatureObjectData, false);
            }
            creatureInMap = GameDatabase.Instance.getAllCreatureInMap(pInfo.id);
        }
        protected MapInstanceSaved cacheInfo;
        public void setInfoCacheInfo(MapInstanceSaved pInfo)
        {
            cacheInfo = pInfo;
        }
        public void addCreatureObject(CreatureInstanceSaved pInfo, bool anim, System.Action<Creature> onCreate = null)
        {
            List<Creature> creatureArray = null;
            var infosaved = GameManager.Instance.Database.creatureInfos.Find(x => x.id == pInfo.id);
            if (!infosaved.isUnLock) { infosaved.isUnLock = true; }
            if (!creatureCache.ContainsKey(pInfo.id))
            {
                creatureCache.Add(pInfo.id, creatureArray = new List<Creature>());
            }
            creatureArray = creatureCache[pInfo.id];
            var dataCreature = GameDatabase.Instance.CreatureCollection.Find(x => x.ItemID == pInfo.id);
            var creature = creatureArray.Find(x => !x.gameObject.activeSelf);
            if (creature)
            {
                creature.transform.localPosition = new Vector3(Random.Range(mainZone.localCorners[0].x, mainZone.localCorners[2].x), Random.Range(mainZone.localCorners[0].y, mainZone.localCorners[2].y), 0);
                creature.gameObject.SetActive(true);
          
                creature.setInfo(pInfo);
                if (!creature.skin.panel)
                {
                    creature.skin.GetComponent<UIWidget>().onFindPanel = delegate {
                        NGUITools.BringForward(creature.gameObject);
                    };
                }
                else
                {
                    NGUITools.BringForward(creature.gameObject);
                }
              
                creature._onPress = onPressCreature;
                if (anim)
                {
                    creature.born();
                }
                creatureAlive.Add(creature);
                if (dataCreature.categoryItem == CategoryItem.CREATURE)
                {
                    creatureAnimAble.Add(creature);
                }
                onCreate?.Invoke(creature);
                return;
            }

            if (dataCreature == null)
            {
                dataCreature = (CreatureItem)GameDatabase.Instance.getItemInventory(pInfo.id);
            }
            dataCreature.getModelForState((o) =>
            {
                var creatureObject = attachMent.transform.AddChild(o).GetComponent<Creature>();
                creatureObject.transform.localScale = o.transform.localScale;
                creatureObject.gameObject.SetActive(true);
                creatureObject.transform.localPosition = new Vector3(Random.Range(mainZone.localCorners[0].x, mainZone.localCorners[2].x), Random.Range(mainZone.localCorners[0].y, mainZone.localCorners[2].y), 0);
                creatureObject.setInfo(pInfo);
                creatureObject._onPress = onPressCreature;
                if (anim)
                {
                    creatureObject.born();
                }
                creatureAlive.Add(creatureObject);
                if (dataCreature.categoryItem == CategoryItem.CREATURE)
                {
                    creatureAnimAble.Add(creatureObject);
                }

                if (!creatureObject.skin.panel)
                {
                    creatureObject.skin.GetComponent<UIWidget>().onFindPanel = delegate {
                        NGUITools.BringForward(creatureObject.gameObject);
                    };
                }
                else
                {
                    NGUITools.BringForward(creatureObject.gameObject);
                }
                onCreate?.Invoke(creatureObject);
            });

        }

        public IEnumerator delayChooseCreatureAnim()
        {
            float random = Random.Range(3, 5);
            yield return new WaitForSeconds(random);
            List<Creature> newArrayRandom = new List<Creature>();
            newArrayRandom.addFromList(creatureAnimAble.ToArray());
            int radomCount = Random.Range(Mathf.Min(1, creatureAnimAble.Count), Mathf.Min(3, creatureAnimAble.Count));
            for (int i = 0; i < radomCount; ++i)
            {
                var randomCreature = newArrayRandom[Random.Range(0, newArrayRandom.Count)];
                StartCoroutine(delayAction(Random.Range(0, 2), delegate
                {
                    var pos = new Vector3(Random.Range(mainZone.localCorners[0].x, mainZone.localCorners[2].x), Random.Range(mainZone.localCorners[0].y, mainZone.localCorners[2].y), 0);
                    randomCreature.move(pos);
                }));
                newArrayRandom.Remove(randomCreature);
            }
            StartCoroutine(delayChooseCreatureAnim());
        }
        public IEnumerator delayAction(float pSec, System.Action action)
        {
            yield return new WaitForSeconds(pSec);
            action?.Invoke();
        }
        public float Alpha
        {
            set
            {
                alpha.Alpha = value;
            }
            get
            {
                return alpha.Alpha;
            }
        }

        public void active(bool imediately)
        {
            gameObject.SetActive(true);
            if (imediately)
            {
                EzEventManager.AddListener(this);
                StartCoroutine(delayChooseCreatureAnim());
            }
        }
        public void show(bool imediately, int sideFrom)
        {
            active(imediately);
            transform.localPosition += new Vector3(1080 * sideFrom, 0, 0);
            var pDestiny = Vector3.zero;
            if (imediately)
            {
                Alpha = 1;
                transform.localPosition = pDestiny;
            
            }
            else
            {
                var pSed = DOTween.Sequence();
                pSed.Append(DOTween.To(() => Alpha, x => Alpha = x, 1, 1));
                transform.DOLocalMoveX(0, 0.5f).SetEase(Ease.OutExpo).OnComplete(delegate
                {
                    setInfo(cacheInfo);
                    EzEventManager.AddListener(this);
                    StartCoroutine(delayChooseCreatureAnim());
                });
            }
        }
        public void hide(bool imediately)
        {
            if (imediately)
            {
                gameObject.SetActive(false);
                for (int i = 0; i < creatureAlive.Count; ++i)
                {
                    creatureAlive[i].gameObject.SetActive(false);
                }
                creatureAlive.Clear();
                creatureAnimAble.Clear();
                EzEventManager.RemoveListener(this);
                StopAllCoroutines();
            }
            else
            {
                EzEventManager.RemoveListener(this);
                StopAllCoroutines();
                var pSed = DOTween.Sequence();
                pSed.Append(DOTween.To(() => Alpha, x => Alpha = x, 0, 1));
                pSed.AppendCallback(delegate ()
                {

                    gameObject.SetActive(false);
                    for (int i = 0; i < creatureAlive.Count; ++i)
                    {
                        creatureAlive[i].gameObject.SetActive(false);
                    }
                    creatureAlive.Clear();
                    creatureAnimAble.Clear();
                });
            }
        }
        private void OnEnable()
        {

        }

        private void OnDisable()
        {

        }

        public void OnEzEvent(AddCreatureEvent eventType)
        {
            if (eventType.zoneid == _info.zoneParent.id)
            {
                if (eventType.creature.mapParent.id == _info.id)
                {
                    addCreatureObject(eventType.creature, true);
                }

            }
        }
    }
}

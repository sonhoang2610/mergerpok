using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;
using ScriptableObjectArchitecture;
using UnityEngine.AddressableAssets;

namespace Pok
{
    public struct UnlockNewEra
    {
        public string nameLeader;
        public string mapID;
    }
    public class PoolContainer
    {
        public string id;
        public List<Creature> list = new List<Creature>();
        public GameObject parent;
    }
    public class PoolObjectContainer
    {
        public GameObject original;
        public List<GameObject> list = new List<GameObject>();
        public GameObject parent;
    }
    public class MapLayer : BaseItem<MapInstanceSaved>, EzEventListener<AddCreatureEvent>
    {
        public UI2DSprite bg;
        public AlphaController alpha;
        public UIWidget mainZone;
        [System.NonSerialized]
        public CreatureItem[] creatureInMap;
        public GameObject attachMent;
        public StringVariable currentMap;
        public GameObject coinJump;
        public UI2DSprite compareRenderQueue;
        List<GameObject> poolCoinJump = new List<GameObject>();
        List<PoolContainer> creatureCache = new List<PoolContainer>();
        public List<GameObject> objectInMap = new List<GameObject>();
        public List<Creature> creatureAlive = new List<Creature>();
        public List<Creature> creatureAnimAble = new List<Creature>();
        public List<PoolObjectContainer> poolContainer = new List<PoolObjectContainer>();
        public AssetReference effectPreload;
        protected GameObject parentCoinJump;
        private void Awake()
        {
            parentCoinJump = new GameObject();
            parentCoinJump.SetLayerRecursively(gameObject.layer);
            parentCoinJump.transform.parent = attachMent.transform;
            parentCoinJump.transform.localPosition = Vector3.zero;
            parentCoinJump.transform.localScale = new Vector3(1, 1, 1);
            var panel = parentCoinJump.AddComponent<UIPanel>();
            panel.depth = attachMent.GetComponent<UIPanel>().depth + 1;
            parentCoinJump.name = "[Pool]CoinJump";
            for (int i = 0; i < 20; ++i)
            {
                var pObject = parentCoinJump.AddChild(coinJump);
                pObject.gameObject.SetActive(false);
                poolCoinJump.Add(pObject);
                objectInMap.Add(pObject);
            }
            if(SceneManager.Instance.processing < 1)
            {
                SceneManager.Instance.block++;
            }
            effectPreload.loadAssetWrapped<GameObject>((a) =>
            {
                if (a)
                {
                    preloadPrefab(a, 10);
                }
                SceneManager.Instance.block--;
            });
        }
        public GameObject addMorePrefabInPool(PoolObjectContainer pool)
        {
            var newObject = pool.parent.AddChild(pool.original);
            newObject.GetComponent<RenderQueueModifier>().setTarget(compareRenderQueue);
            newObject.transform.localPosition = Vector3.zero;
            newObject.transform.localScale = pool.original.transform.localScale;
            newObject.gameObject.SetActive(false);
            pool.list.Add(newObject);
            return newObject;
        }
        public PoolObjectContainer preloadPrefab(GameObject prefab, int preloadCount)
        {
            GameObject parentPrefab = new GameObject();
            parentPrefab.SetLayerRecursively(gameObject.layer);
            parentPrefab.transform.parent = attachMent.transform;
            parentPrefab.transform.localPosition = Vector3.zero;
            parentPrefab.transform.localScale = new Vector3(1, 1, 1);
            parentPrefab.name = "[Pool]" + prefab.name;
            var listObject = new List<GameObject>();

            var pool = new PoolObjectContainer()
            {
                list = listObject,
                original = prefab,
                parent = parentPrefab,
            };
            for(int i = 0; i < preloadCount; ++i)
            {
                addMorePrefabInPool(pool);
            }
            poolContainer.Add(pool);
            return pool;
        }

        public GameObject getObjectFromPool(GameObject prefab)
        {
            var pool = poolContainer.Find(x => x.original == prefab);
            if (pool == null)
            {
                pool = preloadPrefab(prefab, 5);
            }
            var pObject = pool.list.Find(x => !x.activeSelf);
            if(pObject == null)
            {
                pObject = addMorePrefabInPool(pool);
            }
            return pObject;
        }
        public void spawnMixingEffect(Vector3 pos)
        {
            SoundManager.Instance.vib();
            var zoneInfo = GameManager.Instance.Database.zoneInfos.Find(x => x.Id == _info.zoneParent.id);
            if (zoneInfo.leaderSelected.ContainsKey(_info.id))
            {
               var leaderID = zoneInfo.leaderSelected[_info.id];
               var eraInfo = GameDatabase.Instance.treeCreature.Find(x => x.creatureLeader.ItemID == leaderID);
                eraInfo.prefabMix.loadAssetWrapped<GameObject>((o) =>
                {
                   var effect = getObjectFromPool(o);
                    effect.gameObject.SetActive(true);
                    effect.transform.localPosition = pos;
                });
            }
        }

        public GameObject getObjectCoinJump()
        {
            GameObject pCoin = poolCoinJump.Find(x => !x.activeSelf);
            if (!pCoin)
            {
                pCoin = parentCoinJump.AddChild(coinJump);
                objectInMap.Add(pCoin);
                poolCoinJump.Add(pCoin);
            }
            return pCoin;
        }
        public void addMoney(Creature pok,int factor)
        {
            pok.effect();
            var creature = GameDatabase.Instance.CreatureCollection.Find(x => x.ItemID == pok._info.id);
            var goldAdd = creature.getGoldAFK(GameManager.Instance.ZoneChoosed);
            goldAdd = (factor * System.Numerics.BigInteger.Parse(goldAdd.clearDot()) * (Random.Range(0, 10) > 8 ? 3 : (Random.Range(0, 10) > 7 ? 2 : 1))).ToString();
            var coin = GameManager.Instance.Database.getItem("Coin");
            coin.addQuantity(goldAdd.clearDot());
            var stringMoney = goldAdd.clearDot().ToKMBTA();
            spawnCoinJump(pok, stringMoney);
        }
        public void spawnCoinJump(Creature pok, string quantity)
        {
            var coin = getObjectCoinJump();
            coin.gameObject.SetActive(true);
            coin.gameObject.SetLayerRecursively(gameObject.layer);
            coin.GetComponentInChildren<UILabel>().text = quantity;
            coin.transform.localPosition = pok.transform.localPosition + new Vector3(0, pok.GetComponent<UIWidget>().height / 4, 0);
            coin.transform.DOLocalMoveY(450, 1.5f).SetEase(Ease.InQuad).SetRelative(true);
            var widget = coin.GetComponent<UIWidget>();
            widget.alpha = 1;
            var seq = DOTween.Sequence();
            seq.AppendInterval(1);

            seq.Append(DOTween.To(() => widget.alpha, x => widget.alpha = x, 0, 0.5f)).AppendCallback(() => { coin.gameObject.SetActive(false); });
            if (widget.panel)
            {
                StartCoroutine(delayAction(0.01f, () => { BringForward(coin); }));
            }
            else
            {
                widget.onFindPanel = () => { StartCoroutine(delayAction(0.01f, () => { BringForward(coin); })); };
            }

        }

        public void onPressCreature(CreatureInstanceSaved infoCreature, CreatureItem item, Creature creature, bool press)
        {
            bool dirty = false;
            if (MainScene.Instance.ChangingMap ) return;
            GameManager.Instance.checkShowAds();
            if (GameManager.Instance.GuideIndex == 4)
            {
                HUDManager.Instance.hand.gameObject.SetActive(false);
                GameManager.Instance.GuideIndex = 5;
                active(true);
            }
            if (item.categoryItem == CategoryItem.PACKAGE_CREATURE && !press)
            {
                if (_info != null)
                {
                    SoundManager.Instance.PlaySound("EggBreak");
                    SoundManager.Instance.vib();
                    var creatureID = ((PackageCreatureInstanceSaved)infoCreature).creature;
                    var newCreature = new CreatureInstanceSaved() { id = creatureID, instanceID = GameManager.Instance.GenerateID.ToString(), mapParent = _info };
                    GameManager.Instance.GenerateID++;
                    GameManager.Instance.Database.worldData.addCreature(newCreature, GameManager.Instance.ZoneChoosed);
                    Vector3 pos = creature.transform.localPosition;
                    addCreatureObject(newCreature, true, (o) => { o.transform.localPosition = pos; });
                    DestroyCreature(infoCreature);
                    GameManager.Instance.Database.calculateCurrentUnlock( GameManager.Instance.ZoneChoosed);
                    HUDManager.Instance.checkEvolutionPack();
                    ES3.markDirty(SaveDataConstraint.WORLD_DATA);
                    dirty = true;
                  
                }
            }
            if (!press && !dirty && Effecting <= 0)
            {
                OnPressUp(creature);
                addMoney(creature,1*(int)GameManager.Instance.getFactorIncome().x);
                SoundManager.Instance.PlaySound("CoinClickPure");
            }
        }
        int effecting = 0;
        public void OnPressUp(Creature mainObject)
        {
            List<Creature> listToCompare = new List<Creature>(creatureAnimAble);
            List<Creature> destroyList = new List<Creature>();
            List<System.Action> queueAction = new List<System.Action>();
            ZoneInfoSaved zone = GameManager.Instance.Database.zoneInfos.Find(x => x.Id == GameManager.Instance.ZoneChoosed);
       
            var nextMap = GameDatabase.Instance.MapCollection.FindIndex(x => x.ItemID == _info.id) + 1;
            var nextMapID = nextMap < GameDatabase.Instance.MapCollection.Count ? GameDatabase.Instance.MapCollection[nextMap].ItemID : "";
            var nextMapInfo = GameManager.Instance.Database.worldData.zones.Find(x => x.id == zone.Id).maps.Find(x => x.id == nextMapID);
            foreach (var creature1 in listToCompare)
            {
                if (!destroyList.Contains(creature1))
                {
                    foreach (var creature2 in listToCompare)
                    {
                        if (creature1._info.id == creature2._info.id && creature1 != creature2 && !destroyList.Contains(creature2) && !destroyList.Contains(creature1))
                        {
                            CreatureItem creatureData = GameDatabase.Instance.CreatureCollection.Find(x => x.ItemID == creature1._info.id);
                            if (creatureData.creatureChilds.Length == 0)
                            {
                                continue;
                            }
                            
                            {
                                var creatures1 = GameManager.Instance.Database.getAllInfoCreatureInAddress(GameManager.Instance.ZoneChoosed, _info.id);
                                if (zone.CurentUnlock == creatureData.ItemID || string.IsNullOrEmpty(zone.CurentUnlock))
                                {

                                }
                                else
                                {
                                    if (creatureData.ItemID != creatures1[creatures1.Length - 1].id)
                                    {
                                    }
                                    else
                                    {
                                        if (nextMapInfo != null)
                                        {
                                            if (nextMapInfo.creatures.Count >= 16)
                                            {
                                                if (!HUDManager.Instance.boxFullSlot.gameObject.activeSelf &&( (mainObject == creature1 || mainObject == creature2)))
                                                {
                                                    HUDManager.Instance.showBoxFullSlot();
                                                }
                                                continue;
                                            }
                                        }
                                    }
                                }
                            }
                            float distance = Vector2.Distance(creature1.transform.localPosition, creature2.transform.localPosition);
                            if (distance < 400)
                            {

                                var posDes = Vector2.Lerp(creature1.transform.localPosition, creature2.transform.localPosition, 0.5f);
                                var object1 = creature1;
                                var object2 = creature2;
                                Effecting++;
                                object1.transform.DOLocalMove(posDes, 0.25f);
                                object2.transform.DOLocalMove(posDes, 0.25f).OnComplete(delegate
                                {
                                    Effecting--;
                                    spawnMixingEffect(posDes);
                                    System.Action DestroyAction = delegate
                                    {
                                        DestroyCreature(object1._info);
                                        DestroyCreature(object2._info);
                                        SoundManager.Instance.PlaySound("Mixing");
                                    };
                                    
                                    var childs = creatureData.creatureChilds;
            
                                    var creatures = GameManager.Instance.Database.getAllInfoCreatureInAddress(GameManager.Instance.ZoneChoosed, _info.id);
                                    System.Action<string> addAnother = delegate (string idCreature)
                                    {
                                        var newCreatureItem = GameDatabase.Instance.CreatureCollection.Find(x => x.ItemID == idCreature);
                                        if(newCreatureItem.creatureChilds.Length == 0 && nextMapInfo.creatures.Count > 0)
                                        {
                                            var newCreature = new CreatureInstanceSaved() { id = idCreature, instanceID = GameManager.Instance.GenerateID.ToString(), mapParent = nextMapInfo };
                                            nextMapInfo.creatures[0].level++;
                                            EzEventManager.TriggerEvent(new AddCreatureEvent() { change = 1, creature = newCreature, manualByHand = true, zoneid = zone.Id });
                                        }
                                        else
                                        {
                                            var newCreature = new CreatureInstanceSaved() { id = idCreature, instanceID = GameManager.Instance.GenerateID.ToString(), mapParent = nextMapInfo };
                                            //nextMapInfo.creatures.Add(newCreature);
                                            GameManager.Instance.Database.worldData.addCreature(newCreature,GameManager.Instance.ZoneChoosed);
                                            GameManager.Instance.GenerateID++;
                                            EzEventManager.TriggerEvent(new AddCreatureEvent() { change = 1, creature = newCreature, manualByHand = true, zoneid = zone.Id });
                                        }
                                        HUDManager.Instance.checkEvolutionPack();
                                    };
                                    System.Action addNormal = delegate
                                    {
                                        var newCreature = new CreatureInstanceSaved() { id = creatureData.creatureChilds[0].ItemID, instanceID = GameManager.Instance.GenerateID.ToString(), mapParent = _info };
                                        GameManager.Instance.GenerateID++;
                                        GameManager.Instance.Database.worldData.addCreature(newCreature, GameManager.Instance.ZoneChoosed);
                                        EzEventManager.TriggerEvent(new AddCreatureEvent()
                                        {
                                            change = 1,
                                            creature = newCreature,
                                            zoneid = GameManager.Instance.ZoneChoosed,
                                            manualByHand = true,
                                            onCreated = (o) => { o.transform.localPosition = object1.transform.localPosition; }
                                        });
                                        var creaturesUnlock = GameManager.Instance.Database.getAllCreatureInfoInZone(GameManager.Instance.ZoneChoosed);
                                        var indexNow = System.Array.FindIndex(creaturesUnlock, x => x.ItemID == creatureData.creatureChilds[0].ItemID);
                                        if (indexNow >= 0 && creaturesUnlock.Length - indexNow >= 5 && creaturesUnlock.Length - indexNow < 7)
                                        {
                                            HUDManager.Instance.showBoxEvo(creaturesUnlock[indexNow], creaturesUnlock[indexNow + 2], newCreature);
                                        }
                                        GameManager.Instance.Database.calculateCurrentUnlock(zone.Id);
                                    };
                                    if (zone.CurentUnlock == creatureData.ItemID || string.IsNullOrEmpty(zone.CurentUnlock))
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
                                                    addAnother.Invoke(BoxSelectEra.Instance.selectCreature.ItemID);
                                                    DestroyAction();
                                                    GameManager.Instance.Database.calculateCurrentUnlock(zone.Id);
                                                };
                                            }
                                            else if (childs.Length == 1)
                                            {
                                                BoxNewEra.Instance.show(childs[0].ItemID);
                                                BoxNewEra.Instance.container.conCustomStartClose = (delegate
                                                {
                                                    addAnother.Invoke(childs[0].ItemID);
                                                    DestroyAction();
                                                    GameManager.Instance.Database.calculateCurrentUnlock(zone.Id);
                                                    BoxNewEra.Instance.container.conCustomStartClose = null;
                                                });
                                                EzEventManager.TriggerEvent(new UnlockNewEra() { nameLeader = childs[0].ItemID, mapID = GameDatabase.Instance.MapCollection[nextMap].ItemID });
                                            }

                                        }
                                        else
                                        {
                                            bool nonExist = false;
                                            if (!zone.creatureAdded.Contains(creatureData.ItemID))
                                            {
                                                nonExist = true;
                                            }
                                            addNormal.Invoke();
                                            DestroyAction();
                                            GameManager.Instance.Database.calculateCurrentUnlock(zone.Id);
                                            if (!nonExist)
                                            {
                                                BoxUnlockNewCreature.Instance.show(zone.CurentUnlock);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (creatureData.ItemID != creatures[creatures.Length - 1].id)
                                        {
                                            addNormal.Invoke();
                                            DestroyAction();
                                        }
                                        else
                                        {
                                            addAnother.Invoke(zone.leaderSelected[nextMapID]);
                                            DestroyAction();
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
            // Debug.LogError("chiu" + creatures.Count);
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
            if (creatures.Count > 1)
            {
                Debug.LogError("something wrong" + creature.instanceID);
            }
            else
            {
                EzEventManager.TriggerEvent(new AddCreatureEvent() { change = -1, creature = creature, manualByHand = true, zoneid = GameManager.Instance.ZoneChoosed });
            }

        }
        public override void setInfo(MapInstanceSaved pInfo)
        {
            base.setInfo(pInfo);
            if (pInfo.id == "Map1_1" && ES3.Load<bool>("First" + GameManager.Instance.ZoneChoosed, true))
            {
                ES3.Save<bool>("First" + GameManager.Instance.ZoneChoosed, false);
                GameManager.Instance.Database.checkTimeItem($"Egg{pInfo.zoneParent.id}");
                var time = TimeCounter.Instance.timeCollection.Value.Find(x => x.id.Contains($"Egg{pInfo.zoneParent.id}"));
                if (time != null)
                {
                    time.firstTimeAdd -= (time.destinyIfHave - 2);
                }
            }
            currentMap.Value = pInfo.id;
            var creatures = GameManager.Instance.Database.getAllInfoCreatureInAddress(pInfo.zoneParent.id, pInfo.id);

            var mapObjec = GameDatabase.Instance.MapCollection.Find(x => x.ItemID == pInfo.id);
            mapObjec.getSpriteForState(delegate (Sprite pSprite)
            {
                bg.sprite2D = pSprite;
            }, "BG" + creatures[0].id);
            for (int i = 0; i < creatureAlive.Count; ++i)
            {
                creatureAlive[i].gameObject.SetActive(false);
            }
            creatureAlive.Clear();
            creatureAnimAble.Clear();
            for (int i = pInfo.creatures.Count - 1; i >= 0; --i)
            {
                var creatureObjectData = pInfo.creatures[i];
                if (!addCreatureObject(creatureObjectData, false))
                {
                    pInfo.creatures.RemoveAt(i);
                }
            }
            creatureInMap = GameDatabase.Instance.getAllCreatureInMap(pInfo.id);
        }
        protected MapInstanceSaved cacheInfo;
        public void setInfoCacheInfo(MapInstanceSaved pInfo)
        {
            cacheInfo = pInfo;
        }
        public bool addCreatureObject(CreatureInstanceSaved pInfo, bool anim, System.Action<Creature> onCreate = null)
        {
            List<Creature> creatureArray = null;
            var infosaved = GameManager.Instance.Database.creatureInfos.Find(x => x.id == pInfo.id);
            if (infosaved == null) return false;
            if (!infosaved.IsUnLock) { infosaved.IsUnLock = true; }
            if (!creatureCache.Exists(x => x.id == pInfo.id))
            {
                GameObject pObject = new GameObject();
                pObject.name = "[Pool]" + pInfo.id;
                var widget = pObject.AddComponent<UIWidget>();
                pObject.transform.parent = attachMent.transform;
                pObject.transform.localPosition = Vector3.zero;
                pObject.transform.localScale = new Vector3(1, 1, 1);
                widget.depth = 0;
                creatureCache.Add(new PoolContainer() { id = pInfo.id, parent = pObject });
            }
            var poolParent = creatureCache.Find(x => x.id == pInfo.id);
            creatureArray = poolParent.list;
            var dataCreature = GameDatabase.Instance.CreatureCollection.Find(x => x.ItemID == pInfo.id);
            var creature = creatureArray.Find(x => !x.gameObject.activeSelf);
            if (creature)
            {
                var pos = new Vector3(Random.Range(mainZone.localCorners[0].x, mainZone.localCorners[2].x), Random.Range(mainZone.localCorners[0].y, mainZone.localCorners[2].y), 0);
                if (GameManager.Instance.GuideIndex == 1)
                {
                    pos = new Vector3(-200, -200, 0);
                }
                if (GameManager.Instance.GuideIndex == 3)
                {
                    pos = new Vector3(200, 200, 0);
                }
                creature.transform.localPosition = pos;
                creature.gameObject.SetActive(true);

                creature.setInfo(pInfo);
                if (!creature.skin.panel)
                {
                    creature.skin.GetComponent<UIWidget>().onFindPanel = delegate
                    {
                        BringForward(creature.gameObject);
                    };
                }
                else
                {
                    BringForward(creature.gameObject);
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
                return true;
            }

            if (dataCreature == null)
            {
                dataCreature = (CreatureItem)GameDatabase.Instance.getItemInventory(pInfo.id);
            }
            if (dataCreature != null)
            {
                dataCreature.getModelForState((o) =>
                {
                    var creatureObject = poolParent.parent.transform.AddChild(o).GetComponent<Creature>();
                    objectInMap.Add(creatureObject.gameObject);
                    creatureArray.Add(creatureObject);
                    creatureObject.transform.localScale = o.transform.localScale;
                    creatureObject.gameObject.SetActive(true);
                    var pos = new Vector3(Random.Range(mainZone.localCorners[0].x, mainZone.localCorners[2].x), Random.Range(mainZone.localCorners[0].y, mainZone.localCorners[2].y), 0);
                    if (GameManager.Instance.GuideIndex == 1)
                    {
                        pos = new Vector3(-200, -200, 0);
                    }
                    if (GameManager.Instance.GuideIndex == 3)
                    {
                        pos = new Vector3(200, 200, 0);
                    }
                    creatureObject.transform.localPosition = pos;
                    creatureObject.setInfo(pInfo);
                    creatureObject.GetComponent<UIDragObject>().restrictWithinPanel = true;
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
                        creatureObject.skin.GetComponent<UIWidget>().onFindPanel = delegate
                        {
                            BringForward(creatureObject.gameObject);
                        };
                    }
                    else
                    {
                        BringForward(creatureObject.gameObject);
                    }
                    onCreate?.Invoke(creatureObject);
                });
            }
            return true;
        }
        public void BringForward(GameObject creature)
        {
           var index = objectInMap.IndexOf(creature);
            if(index >= 0)
            {
                objectInMap.Remove(creature);
                objectInMap.Add(creature);
            }
            for(int i = 0; i < objectInMap.Count; ++i)
            {
                var widgets = objectInMap[i].GetComponentsInChildren<UIWidget>(true);
                int subIndex = 0;
                foreach(var widget in widgets)
                {
                    widget.depth = i * 5 + subIndex;
                    subIndex++;
                }
            }
            //var poolParent = creatureCache.Find(x => x.id == pInfo.id);
            //creatureArray = poolParent.list;
            //creature.transform.SetSiblingIndex(poolContainer.list);
        }

        public IEnumerator delayChooseCreatureAnim(bool first = true)
        {
            if (GameManager.Instance.GuideIndex < 5)
            {
                yield break;
            }
            float random = Random.Range(3, 5);
            if (first)
            {
                random = Random.Range(1, 2);
            }
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
            StartCoroutine(delayChooseCreatureAnim(false));
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

        public int Effecting { get => effecting; set { effecting = value;MainScene.Instance.MovingPok = value > 0; } }

        public void active(bool imediately)
        {
            gameObject.SetActive(true);
            if (imediately)
            {
                EzEventManager.AddListener(this);
                StopAllCoroutines();
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
                PokUltis.KillTween("MapMoving");
                Alpha = 1;
                transform.localPosition = pDestiny;

            }
            else
            {
                PokUltis.KillTween("MapMoving");
                var pSed = DOTween.Sequence();
                pSed.SetId("MapMoving");
                pSed.Append(DOTween.To(() => Alpha, x => Alpha = x, 1, 1));
                transform.DOLocalMoveX(0, 0.5f).SetEase(Ease.OutExpo).OnComplete(delegate
                {
                    setInfo(cacheInfo);
                    EzEventManager.AddListener(this);
                    StartCoroutine(delayChooseCreatureAnim());
                }).SetId("MapMoving");
            }
        }
        public void hide(bool imediately)
        {
            if (imediately)
            {
                PokUltis.KillTween("MapMoving");
                Alpha = 0;
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
                PokUltis.KillTween("MapMoving");
                EzEventManager.RemoveListener(this);
                StopAllCoroutines();
                var pSed = DOTween.Sequence();
                pSed.SetId("MapMoving");
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
            StartCoroutine(GameManager.Instance.checkGuide());
            if (eventType.zoneid == _info.zoneParent.id && eventType.change == 1)
            {
                if (eventType.creature.mapParent.id == _info.id)
                {
                    addCreatureObject(eventType.creature, true, eventType.onCreated);
                }

            }
        }
    }
}

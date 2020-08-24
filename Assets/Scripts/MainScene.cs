using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;
using DG.Tweening;
using BigInt = System.Numerics.BigInteger;


namespace Pok {
    public class MainScene : Singleton<MainScene>,EzEventListener<UnlockNewEra>,EzEventListener<AddCreatureEvent>
    {
        public List<MapLayer> mapPool;
        public float threshHoldDragLayerPage = 540;
        public EazyGroupTabNGUI ChoseMapLayer;
        [System.NonSerialized]
        public List<MapInstanceSaved> MapObjects = new List<MapInstanceSaved>();
        public GameObject boxTreasure;
        public GameObject blockTouch;
        private int currentPageMapLayer = 0;
        protected int currentIndexPool = 0,cacheSizeDrag = 0;

        bool isMinized = false;

        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                isMinized = true;
            }else if(isMinized && !TimeCounter.InstanceRaw.IsDestroyed() && TimeCounter.Instance.minimizeTime > GameDatabase.Instance.timeAFKShowBoxTreasure)
            {
                isMinized = false;
                showBoxTreasure();
            }
        }

        public void showBoxTreasure()
        {
            StartCoroutine(checkShowBoxTreasure());
        }
        public IEnumerator checkShowBoxTreasure()
        {
            yield return new WaitForSeconds(1);

       
            while (CurrentPageMapLayer != 0)
            {
                yield return new WaitForSeconds(1);
            }
            var zone = GameManager.Instance.Database.zoneInfos.Find(x => x.id == GameManager.Instance.ZoneChoosed);
            string index = zone.curentUnlock.Replace("Pok", "");
            if (CurrentPageMapLayer == 0 && int.Parse(index) > 7 && !string.IsNullOrEmpty(zone.curentUnlock))
            {
                boxTreasure.transform.parent.parent = mapPool[CurrentIndexPool].transform;
                boxTreasure.gameObject.SetActive(true);
            }
        }
        public void showBoxMagicCaseContain()
        {
            HUDManager.Instance.showBoxMagicCaseContain();
        }


        public IEnumerator onEnableLateUpdate()
        {
            yield return new WaitForEndOfFrame();
             while (!GameManager.readyForThisState("Main"))
            {
                yield return new WaitForEndOfFrame();
            }
            var quantitySecIncome = BigInt.Parse(GameManager.Instance.getTotalGoldGrowthCurrentZone()) * (int)GameManager.Instance.getFactorIncome().x;
            HUDManager.Instance.quanityHour.text = quantitySecIncome.ToString().ToKMBTA();
        }
        private IEnumerator StartQueue()
        {
            yield return new WaitForEndOfFrame();
            while (!GameManager.readyForThisState("Main"))
            {
                yield return new WaitForEndOfFrame();
            }
            MapObjects = GameManager.Instance.Database.getAllMapActiveInZone(GameManager.Instance.ZoneChoosed);
            MapObjects.Sort((a, b) => { return GameDatabase.Instance.MapCollection.FindIndex(x => x.ItemID == a.id).CompareTo(GameDatabase.Instance.MapCollection.FindIndex(x => x.ItemID == b.id)); });

            updateMapLayer(true);
        }

        private void Start()
        {
            StartCoroutine(StartQueue());
        }
        public bool chooseZone(object pChoose)
        {
            var pZone1 = (ZoneObject)pChoose;
            var zoneInfo1 = GameManager.Instance.Database.zoneInfos.Find(x => x.id == pZone1.ItemID);
            System.Action<object> action = (object pChooseZone) =>
            {
                var pZone = (ZoneObject)pChooseZone;
                var zoneInfo = GameManager.Instance.Database.zoneInfos.Find(x => x.id == pZone.ItemID);
                for (int i = 0; i < GameDatabase.Instance.ZoneCollection.Count; ++i)
                {
                    var itemCoinBank = GameDatabase.Instance.ZoneCollection[i].coinBank;
                    var timing = GameManager.Instance.Database.timeRestore.Find(x => x.id.Contains(itemCoinBank.ItemID));
                    if (timing != null)
                    {
                        timing.pauseTime(GameDatabase.Instance.ZoneCollection[i] != pZone);
                    }
                }
                GameManager.Instance.ZoneChoosed = pZone.ItemID;
                HUDManager.Instance.checkEvolutionPack();
                CurrentPageMapLayer = 0;
                CurrentIndexPool = 0;
                MapObjects = GameManager.Instance.Database.getAllMapActiveInZone(GameManager.Instance.ZoneChoosed);
                updateMapLayer(true);
                GameManager.Instance.Database.checkTimeForAll();
            };
            if (!zoneInfo1.isUnLock)
            {
                var existCoin = GameManager.Instance.Database.getItem("Coin");
                if(existCoin.QuantityBig >= BigInt.Parse( pZone1.moneyToUnlock.clearDot()))
                {
                    existCoin.addQuantity((-BigInt.Parse(pZone1.moneyToUnlock.clearDot())).ToString());
                    zoneInfo1.isUnLock = true;
                    action(pChoose);
                    //close choose zone + dialog atten
                }
                else
                {
                    HUDManager.Instance.showBoxNotEnough(existCoin.item);
                }
                return false;
            }
            else
            {
                action(pChoose);
                return true;
            }
        }
        public void updateMapLayer(bool imediately = false)
        {
            if (boxTreasure.activeSelf)
            {
                boxTreasure.gameObject.SetActive(false);
            }
            if (imediately)
            {
                GameManager.Instance.tryShowBoxRewardAds();
                mapPool[AnotherIndexPool].hide(imediately);
                mapPool[CurrentIndexPool].show(imediately,0);
                mapPool[CurrentIndexPool].setInfo(MapObjects[CurrentPageMapLayer]);
            }
            else
            {
                GameManager.Instance.tryShowBoxRewardAds();
                mapPool[AnotherIndexPool].hide(imediately);
                mapPool[CurrentIndexPool].show(imediately, cacheSizeDrag);
                mapPool[CurrentIndexPool].setInfoCacheInfo(MapObjects[CurrentPageMapLayer]);
            }
            ChoseMapLayer.changeTabUI(CurrentPageMapLayer);
     
        }
        public int CurrentIndexPool
        {
            get
            {
                return currentIndexPool;
            }
            set
            {
                currentIndexPool = value;
            }
        }
        public int AnotherIndexPool
        {
            get
            {
                var another = currentIndexPool + 1;
                if(another > 1)
                {
                    another = 0;
                }
                return another;
            }

        }

        public int CurrentPageMapLayer { get => currentPageMapLayer; set => currentPageMapLayer = value; }
        protected bool MovingMap { get => movingMap; set { movingMap = value;
                 foreach(var tab in ChooseMapLayer.Instance.tabs.GroupTab)
                {
                    tab.GetComponent<UIButton>().isEnabled = !value;
                }
            } }

        protected Vector2 cachePosDrag;
        protected bool allowDrag = false;
        protected int cacheSideDrag = 0;
        private bool movingMap = false;
        public void DragMapLayer(Vector2 delta,int index,bool EndDrag = false)
        {
            if (MovingMap) return;
            if (EndDrag && allowDrag)
            {
                blockTouch.gameObject.SetActive(true);
               var distance = Mathf.Abs(mapPool[AnotherIndexPool].transform.localPosition.x - cachePosDrag.x);
                float pAlphaDestiny = 0;
                var current = mapPool[CurrentIndexPool];
                var another = mapPool[AnotherIndexPool];
                if (distance > threshHoldDragLayerPage)
                {
                    GameManager.Instance.tryShowBoxRewardAds();
                    if (boxTreasure.activeSelf)
                    {
                        boxTreasure.gameObject.SetActive(false);
                    }
                    mapPool[AnotherIndexPool].transform.DOLocalMoveX(0, 0.25f);
                    CurrentPageMapLayer -= cacheSideDrag;
                    CurrentIndexPool = Mathf.Abs(CurrentIndexPool - 1);
                    pAlphaDestiny = 1;
                    ChoseMapLayer.changeTabUI(CurrentPageMapLayer);
                }
                else
                {
                    mapPool[AnotherIndexPool].transform.DOLocalMoveX(cachePosDrag.x, 0.25f);
                }
                MovingMap = true;
                var pTween = DOTween.Sequence();
                pTween.Append( DOTween.To(() => another.Alpha, x => another.Alpha = x, pAlphaDestiny, 0.25f));
                pTween.AppendCallback(delegate () {
                    MovingMap = false;
                    blockTouch.gameObject.SetActive(false);
                    if (pAlphaDestiny == 0)
                    {
                        another.hide(true);
                    }
                    else
                    {
                        current.hide(true);
                    }
                });
                allowDrag = false;
                return;
            }
            if (index == 0)
            {
               
                var side = Mathf.Sign(delta.x);
                cacheSideDrag = (int)side;
                int anotherPage = CurrentPageMapLayer - (int)side;
                allowDrag = anotherPage >= 0 && anotherPage < MapObjects.Count;
                if (allowDrag)
                {
                    var startPos = new Vector3(-side * 1080, 0, 0);
                    cachePosDrag = startPos;
                    mapPool[AnotherIndexPool].transform.localPosition = startPos;
                    mapPool[AnotherIndexPool].setInfo(MapObjects[anotherPage]);
                    mapPool[AnotherIndexPool].active(true);
                    NGUITools.BringForward(mapPool[AnotherIndexPool].gameObject);
                }
            }
            else if(allowDrag)
            {
                Vector3 pos = (Vector3)cachePosDrag + new Vector3(delta.x, 0, 0); 
                if(cacheSideDrag < 0)
                {
                    pos.x = pos.x < 0 ? 0 : pos.x;
                }
                if (cacheSideDrag > 0)
                {
                    pos.x = pos.x > 0 ? 0 : pos.x;
                }
                mapPool[AnotherIndexPool].transform.localPosition = pos;
            }
            if (allowDrag)
            {
                float calculatorAlpha = Mathf.Abs(mapPool[AnotherIndexPool].transform.localPosition.x - cachePosDrag.x) / 1080.0f;
                mapPool[AnotherIndexPool].Alpha = calculatorAlpha;
            }
        
        }

        public void ChooseMap(int index)
        {
            MapObjects = GameManager.Instance.Database.getAllMapActiveInZone(GameManager.Instance.ZoneChoosed);
            if(mapPool[CurrentIndexPool].gameObject.activeSelf && mapPool[CurrentIndexPool]._info != null && index < MapObjects.Count && mapPool[CurrentIndexPool]._info.id != MapObjects[index].id)
            {
                CurrentIndexPool++;
                if (CurrentIndexPool > 1)
                {
                    CurrentIndexPool = 0;
                }
            }
        
            CurrentPageMapLayer = index;
            cacheSizeDrag = 0;
            updateMapLayer(true);
            //  ChoseMapLayer(index);
        }
        private void OnEnable()
        {
            StartCoroutine(onEnableLateUpdate());
            EzEventManager.AddListener<UnlockNewEra>(this);
            EzEventManager.AddListener<AddCreatureEvent>(this);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            EzEventManager.RemoveListener<UnlockNewEra>(this);
            EzEventManager.RemoveListener<AddCreatureEvent>(this);
        }
        public void OnEzEvent(UnlockNewEra eventType)
        {
            //MapObjects = GameManager.Instance.Database.getAllMapActiveInZone(GameManager.Instance.ZoneChoosed);
            //CurrentIndexPool++;
            //if(CurrentIndexPool> 1)
            //{
            //    CurrentIndexPool = 0;
            //}
            //CurrentPageMapLayer = MapObjects.FindIndex(x => x.id == eventType.mapID);
            //cacheSizeDrag = 1;
            //ChoseMapLayer.changeTabUI(CurrentPageMapLayer);
            //updateMapLayer(false);
        }

        public void OnEzEvent(AddCreatureEvent eventType)
        {
            MapObjects = GameManager.Instance.Database.getAllMapActiveInZone(GameManager.Instance.ZoneChoosed);
            if (GameManager.readyForThisState("Main"))
            {
                var quantitySecIncome = BigInt.Parse(GameManager.Instance.getTotalGoldGrowthCurrentZone()) * (int)GameManager.Instance.getFactorIncome().x;
                HUDManager.Instance.quanityHour.text = quantitySecIncome.ToString().ToKMBTA();
                var creature = GameManager.Instance.Database.getAllCreatureInstanceInAdress(eventType.zoneid, eventType.creature.mapParent.id);
                var map = GameDatabase.Instance.MapCollection.Find(x => x.ItemID == eventType.creature.mapParent.id);
                HUDManager.Instance.fullSlotIcon.gameObject.SetActive(creature.Count >= map.limitSlot);
            }
        }
    }
}

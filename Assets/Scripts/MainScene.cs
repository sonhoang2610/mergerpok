﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;
using DG.Tweening;


namespace Pok {
    public class MainScene : Singleton<MainScene>,EzEventListener<UnlockNewEra>
    {
        public List<MapLayer> mapPool;
        public float threshHoldDragLayerPage = 540;
        public EazyGroupTabNGUI ChoseMapLayer;
        [System.NonSerialized]
        public List<MapInstanceSaved> MapObjects = new List<MapInstanceSaved>();

        private int currentPageMapLayer = 0;
        protected int currentIndexPool = 0,cacheSizeDrag = 0;

        private void Start()
        {
            MapObjects = GameManager.Instance.Database.getAllMapInZone(GameManager.Instance.ZoneChoosed);
            updateMapLayer(true);
        }
        public void chooseZone(object pChoose)
        {
            var pZone = (ZoneObject)pChoose;
            GameManager.Instance.ZoneChoosed = pZone.ItemID;
            CurrentPageMapLayer = 0;
            CurrentIndexPool = 0;
            MapObjects = GameManager.Instance.Database.getAllMapActiveInZone(GameManager.Instance.ZoneChoosed);
            updateMapLayer(true);
            //map1.setInfo
        }
        public void updateMapLayer(bool imediately = false)
        {
            if (imediately)
            {
                mapPool[AnotherIndexPool].hide(imediately);
                mapPool[CurrentIndexPool].show(imediately,0);
                mapPool[CurrentIndexPool].setInfo(MapObjects[CurrentPageMapLayer]);
            }
            else
            {
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

        protected Vector2 cachePosDrag;
        protected bool allowDrag = false;
        protected int cacheSideDrag = 0;
        public void DragMapLayer(Vector2 delta,int index,bool EndDrag = false)
        {
            if (EndDrag && allowDrag)
            {
                var distance = Mathf.Abs(mapPool[AnotherIndexPool].transform.localPosition.x - cachePosDrag.x);
                float pAlphaDestiny = 0;
                var current = mapPool[CurrentIndexPool];
                var another = mapPool[AnotherIndexPool];
                if (distance > threshHoldDragLayerPage)
                {
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
                var pTween = DOTween.Sequence();
                pTween.Append( DOTween.To(() => another.Alpha, x => another.Alpha = x, pAlphaDestiny, 0.25f));
                pTween.AppendCallback(delegate () {
                    if(pAlphaDestiny == 0)
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
                mapPool[AnotherIndexPool].transform.localPosition = (Vector3)cachePosDrag + new Vector3(delta.x, 0, 0);
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
            EzEventManager.AddListener(this);
        }

        private void OnDisable()
        {
            EzEventManager.RemoveListener(this);
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
    }
}

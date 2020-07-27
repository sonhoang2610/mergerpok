using DG.Tweening;
using NodeCanvas.StateMachines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace Pok
{

    public class Creature : BaseItem<CreatureInstanceSaved>
    {
        public UI2DSprite skin;
        protected Vector3 scale;
        protected bool effecting = false,blockMove;
        protected Coroutine timer;
        protected CreatureItem cacheItem;
        protected Tween tweenMove;
        public System.Action<CreatureInstanceSaved,CreatureItem,Creature, bool> _onPress;
      
        public void OnPress(bool press)
        {
            blockMove = press;
            if (tweenMove != null)
            {
                tweenMove.Kill();
            }
            _onPress?.Invoke(_info,cacheItem,this, press);
        }

    
        public override void setInfo(CreatureInstanceSaved info)
        {
            scale = GameManager.ScaleFactor;
            base.setInfo(info);
            var pInfo = GameDatabase.Instance.CreatureCollection.Find(x => x.ItemID == info.id);
            cacheItem = pInfo;
            pInfo.getSpriteForState((s) =>
            {
                skin.sprite2D = s;
                skin.MakePixelPerfect();
                skin.transform.localScale = GameManager.ScaleFactor;
                if (skin.gameObject != gameObject)
                {
                    GetComponent<BoxCollider>().size = skin.localSize * skin.transform.localScale * transform.localScale;
                }

            });
       
        }
        private void OnEnable()
        {
            if(_info != null)
            {
                StartCoroutine(startEffect());
            }
        }
        public void Start()
        {
     
        }

        public IEnumerator startEffect()
        {
            float sec = Random.Range(0, 5);
            yield return new WaitForSeconds(sec);
            if (cacheItem.categoryItem == CategoryItem.CREATURE)
            {
                timer = StartCoroutine(nhatien());
            }
        }

        public IEnumerator nhatien()
        {
            yield return new WaitForSeconds(5);
            effect();
            timer = StartCoroutine(nhatien());
        }
        private void OnDisable()
        {
            if(timer != null){
                StopCoroutine(timer);
                timer = null;
            }
            StopAllCoroutines();
        }

        public void effect()
        {
            effecting = true;
            NGUITools.BringForward(gameObject);
            skin.transform.localScale = scale * 0.8f;
           var seq = DOTween.Sequence();
            seq.Append(skin.transform.DOScale(scale,1).SetEase(Ease.OutElastic));
            seq.AppendCallback(delegate { effecting = false; });
        }
        [ContextMenu("Born")]
        public void born()
        {
            GetComponent<FSMOwner>().SendEvent("Born");
        }
        public void bornScale()
        {
            skin.transform.localScale = Vector3.zero;
            skin.transform.DOScale(scale, 1).SetEase(Ease.OutElastic);
        }
        public void bornDrop(System.Action onComplete = null)
        {
            effecting = true;
            var oldPos = skin.transform.localPosition;
            skin.transform.localPosition += new Vector3(0, 1920, 0);
            var seq = DOTween.Sequence();
            skin.transform.localScale = new Vector3(scale.x*0.7f, scale.y , scale.z);
            seq.Append(skin.transform.DOLocalMove(oldPos, 0.8f).SetEase(Ease.InQuad));
            seq.Append(skin.transform.DOScale(new Vector3(scale.x, scale.y * 0.7f, scale.z), 0.25f).SetEase(Ease.OutQuad));
            seq.Append(skin.transform.DOScale(scale , 1).SetEase(Ease.OutElastic));
            seq.AppendCallback(delegate { effecting = false; onComplete?.Invoke(); });
        }

        public void move(Vector3 destiny)
        {
            if (!blockMove)
            {
                NGUITools.BringForward(gameObject);
                if(tweenMove != null)
                {
                    tweenMove.Kill();
                    tweenMove = null;
                }
                tweenMove = transform.DOLocalMove(destiny, 5).SetEase(Ease.OutExpo);
            }
        }
    }
}

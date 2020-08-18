using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using EazyEngine.Tools;
using DG.Tweening;
using Pok;

public class UIElement : MonoBehaviour {
    public bool _fadeAnimation;
    public string _moveAnimation;
   
    [SerializeField]
     public UnityEvent onEnableEvent;
    [SerializeField]
    public UnityEvent onDisableEvent;
    [SerializeField]
    UnityEvent onStartEvent;
    [SerializeField]
    UnityEvent onInitEvent;
    [SerializeField]
    UnityEvent onEnableLateUpdateEvent;
    [SerializeField]
    UnityEvent onCompleteTweenShow;


    public UnityEvent onStartClose;
    public int relative = 0;

    protected Vector3 cachePos;

    private void Awake()
    {
        cachePos = transform.localPosition;
    }
    public void showRelative()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        relative++;
    }
    public void setActive(bool active,bool isRelative = false)
    {
        if (active)
        {
            showActive(true);
            relative += isRelative ? 1 :0;
        }
        else
        {
            relative -= isRelative ? 1 : 0;
            if (relative <= 0)
            {
                closeActive(true);
            }
        }
    }
    public void hideRelative()
    {
        relative--;
        if(relative <= 0)
        {
            gameObject.SetActive(false);
        }
    }
    public void resetTween()
    {
    }

    public void stepEnable()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
    public  virtual void show()
    {
        showActive(false);
    }
    public virtual void close()
    {
        closeActive(false);
    }
    public virtual void showActive(bool imediately = false)
    {
        if (imediately)
        {
            if(!gameObject.activeSelf)
                gameObject.SetActive(true);
            var rect = GetComponent<UIRect>();
            rect.alpha = 1;
            return;
        }
        if (!gameObject.activeSelf)
        {
        }
            showElement(
                delegate {
                    onCompleteTweenShow.Invoke();
                }
                ); 
    
    }

    public bool Animation
    {
        get
        {
            return _fadeAnimation || !string.IsNullOrEmpty(_moveAnimation);
        }
    }
    protected Sequence tween;
    public virtual void showElement(System.Action pComplete)
    {
        GameObject o;
        (o = gameObject).SetActive(true);

        var rect = GetComponent<UIRect>();
 
        if (Animation)
        {
            if (_fadeAnimation)
            {
                if (rect)
                {
                    if(tween != null) { tween.Kill(); }
                    tween = DOTween.Sequence();
                    tween.Append(
                     DOTween.To(() => rect.alpha, x => rect.alpha = x, 1, 0.25f).OnStepComplete(delegate { pComplete?.Invoke(); }));
                    if (!string.IsNullOrEmpty(_moveAnimation))
                    {
                        transform.localPosition = new Vector3(_moveAnimation.Contains("Right") ? GameManager.Instance.resolution.x : -GameManager.Instance.resolution.x, cachePos.y, cachePos.z);
                        tween.Join( transform.DOLocalMove(cachePos, 0.5f).SetEase(Ease.OutExpo));
                    }
                }
            }
        }
        else
        {
            pComplete?.Invoke();
        }
    }

    public virtual void closeActive(bool imediately = false)
    {
        if (imediately)
        {
            gameObject.SetActive(false);
            return;
        }
        //   RootMotionController.stopAllAction(gameObject);
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
        //SoundManager.Instance.PlaySound(AudioGroupConstrant.Disappear);
        var rect = GetComponent<UIRect>();
        if (!Animation)
        {
            gameObject.SetActive(false);
        }
        else {
            if (tween != null) { tween.Kill(); }
            tween = DOTween.Sequence();tween.Append( DOTween.To(() => rect.alpha, x => rect.alpha = x, 0, 0.25f));
            if (!string.IsNullOrEmpty(_moveAnimation))
            {
                var pDestiny = new Vector3(_moveAnimation.Contains("Right")? GameManager.Instance.resolution.x :-GameManager.Instance.resolution.x, cachePos.x, cachePos.y);
                tween.Join( transform.DOLocalMove(pDestiny, 0.5f).SetEase(Ease.OutExpo));
            }
            tween.AppendCallback(delegate () { gameObject.SetActive(false); });
        }
        onStartClose?.Invoke();
    }

    public void change()
    {
        if (gameObject.activeSelf)
        {
            close();
        }
        else
        {
            show();
        }
    }

    private int isFirst = 2;

    private void LateUpdate()
    {
        if (isFirst == 0) return;
        isFirst--;
        if (isFirst == 0)
        {
            onEnableLateUpdateEvent.Invoke();
        }
    }
    private void OnEnable()
    {
  
        isFirst = 2;
        onEnableEvent?.Invoke();
        onStartEvent?.Invoke();
    }

    private void OnDisable()
    {
        //if (listenTriggerUI)
        //{
        //    EzEventManager.RemoveListener(this);
        //}
        onDisableEvent?.Invoke();
    }
    protected virtual void Start()
    {
        onInitEvent.Invoke();
    }

    public IEnumerator delayAction(float pDelay,UnityEvent pEvent)
    {
        yield return new WaitForSeconds(pDelay);
        pEvent.Invoke();
    }
    private void Update()
    {
        DoUpdate();
    }

    // Update is called once per frame
    protected virtual void DoUpdate () {
    
	}

}

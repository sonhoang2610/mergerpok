using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EazyParallaxElement : MonoBehaviour
{

    [SerializeField]
    Vector2 sizeElement = Vector2.zero;
    [SerializeField]
    UnityEvent _onOutSide, _onStartMove;
    float offsetElement;
    float oldOffset = 0;
    [HideInInspector]
    public int index = 0;
    EazyParallax parrent;

    public Vector2 SizeElement
    {
        get
        {
            return sizeElement;
        }

        set
        {
            sizeElement = value;
        }
    }

    public float OffsetElement
    {
        get
        {
            return offsetElement;
        }

        set
        {
            offsetElement = value;
        }
    }

    public EazyParallax Parrent
    {
        get
        {
            return parrent;
        }

        set
        {
            parrent = value;
        }
    }

    public float OldScroll
    {
        get
        {
            return oldOffset;
        }

        set
        {
            oldOffset = value;
        }
    }

    public void outSide()
    {
        _onOutSide.Invoke();
        onOutSide();
    }

    public void startMove()
    {
        _onStartMove.Invoke();
        onStartMove();
    }

    protected virtual void onOutSide()
    {

    }

    protected virtual void onStartMove()
    {

    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}

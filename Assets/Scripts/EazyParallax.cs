using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IElementReset
{
    void resetToBegin();
}
public enum ParallaxSide { HORIZONTAL, VERTICAL };
[System.Serializable]
public class EazyParallax : MonoBehaviour
{
    EazyParallaxElement[] elements;
    [SerializeField]
    EazyParallaxElement prefabItem;
    [SerializeField]
    int countItem;
    [SerializeField]
    ParallaxSide side;
    [SerializeField]
    public float distance = 0;
    [SerializeField]
    public float startSpeed = 0;
    [SerializeField]
    public float maxSpeed = 0, durationToMaxSpeed = 0, durationAtMaxSpeed = 0;
    [SerializeField]
    public float scroll = 0;
    [SerializeField]
    public float timeStop = 1;
    [SerializeField]
    public int countRoll = 1;
    [SerializeField]
    AnimationCurve curveStop = null;
    [SerializeField]
    public bool isForever = false;
    [SerializeField]
    bool isFit = true;
    [SerializeField]
    Vector2 sizeControll = Vector2.zero;
    [SerializeField]
    public UnityAction callBackStop = null;

    bool isStop = false;
    float currentTime = 0;
    float currentTimeStop = 0;
    float startRollStop = 0;
    float totalRollStop = 0;
    float totalSize = 0;
    float currentTimeAtMax = 0;
    Rect rectElementController;
    float accel;
    bool rolling = false;
    public void cacheElement()
    {
        Elements = GetComponentsInChildren<EazyParallaxElement>();
        if (Elements.Length > 0)
        {
            resortElement();
        }
        for (int i = 0; i < Elements.Length; ++i)
        {
            //int nextIndex = i + 1;
            //if (nextIndex >= elements.Length)
            //{
            //    nextIndex = 0;
            //}
            //elements[i].addElement(elements[nextIndex], distance);
            //int downIndex = i - 1;
            //if (downIndex < 0)
            //{
            //    downIndex = elements.Length - 1;
            //}
            //elements[i].addElement(elements[downIndex], -distance);
            if (side == ParallaxSide.VERTICAL)
            {
                Elements[i].OffsetElement = convertPointToOffset(Elements[i].transform.localPosition.y, totalSize);
                Elements[i].OldScroll = Elements[i].OffsetElement;
            }
        }

    }

    float convertPointToOffset(float pos, float size)
    {
        float offset = 0.5f;
        offset += pos / size;
        return offset;
    }

    float convertOffsetToPos(float offset, float size)
    {
        offset -= 0.5f;
        return offset * size;
    }

    public void resortElement()
    {
        totalSize = (Elements.Length) * Mathf.Abs(distance);
        float pStart = -0.5f * totalSize;
        for (int i = 0; i < Elements.Length; ++i)
        {
            Elements[i].transform.setLocalPosition2D(new Vector2(0, pStart + distance / 2));
            Elements[i].index = i;
            Elements[i].Parrent = this;
            pStart += distance;
        }
    }

    public bool Rolling
    {
        get
        {
            return rolling;
        }

        set
        {
            rolling = value;
        }
    }
    [ContextMenu("Start")]
    public void startRoll()
    {
        SoundManager.Instance.PlaySound("Wheel");
        Rolling = true;
        isStop = false;
        currentTime = 0;
        startRollStop = 0;
        totalRollStop = (float)countRoll;
        currentTimeStop = 0;
        currentTimeAtMax = 0;
        for (int i = 0; i < Elements.Length; ++i)
        {
            IElementReset pReset = Elements[i].GetComponent<IElementReset>();
            if (pReset != null)
            {
                pReset.resetToBegin();
            }
        }
    }

    public EazyParallaxElement[] Elements
    {
        get
        {
            return elements;
        }

        set
        {
            elements = value;
        }
    }
    [ContextMenu("Preload")]
    public void preIntial()
    {
        for (int i = 0; i < countItem; ++i)
        {
           var item = gameObject.AddChild(prefabItem.gameObject);
            
        }
    }

    private void Awake()
    {
        if (transform.childCount == 0)
        {
            for (int i = 0; i < countItem; ++i)
            {
               var item = gameObject.AddChild(prefabItem.gameObject);
                item.transform.localRotation = prefabItem.transform.localRotation;
            }
        }
        rectElementController = new Rect(-sizeControll.x / 2, -sizeControll.y / 2, sizeControll.x, sizeControll.y);
        accel = (maxSpeed - startSpeed) / durationToMaxSpeed;
        cacheElement();

    }

    private void OnEnable()
    {

    }

    void Start()
    {
        totalRollStop = (float)countRoll;
    }
    // Update is called once per frame
    void Update()
    {
        if (Rolling)
        {
            float pDistance = 0;
            if (!isStop)
            {
                currentTime += Time.deltaTime;
                float speed = maxSpeed;
                bool isMax = false;
                if (maxSpeed != startSpeed && durationToMaxSpeed > 0 && currentTime <= durationToMaxSpeed)
                {
                    speed = startSpeed + accel * currentTime;
                }
                else
                {
                    isMax = true;
                }
                pDistance = speed * Time.deltaTime;
                scroll += pDistance;
                if (isMax)
                {
                    if (currentTimeAtMax < durationAtMaxSpeed)
                    {
                        currentTimeAtMax += Time.deltaTime;
       
                    }
                    if (currentTimeAtMax >= durationAtMaxSpeed && !isForever)
                    {
                        isMax = false;
                        if (scroll < 0)
                        {
                            scroll += Mathf.Abs((int)scroll);
                            scroll += 1;
                        }
                        if (scroll > 1)
                        {
                            scroll -= (int)scroll;
                        }
                        totalRollStop += pDistance < 0 ? scroll : 1 - scroll;
                        startRollStop = scroll;
                        isStop = true;
                    }
                }
            }
            else
            {
                currentTimeStop += Time.deltaTime;
                float percent = currentTimeStop / timeStop;
                if (percent >= 1)
                {
                    SoundManager.Instance.PlaySound("RewardSpin");
                    percent = 1;
                    Rolling = false;
                    if (callBackStop != null)
                    {
                        callBackStop();
                    }
                }
                pDistance = totalRollStop * curveStop.Evaluate(percent) * (maxSpeed / Mathf.Abs(maxSpeed));
                scroll = startRollStop + pDistance;

            }
            if (scroll < 0)
            {
                scroll += Mathf.Abs((int)scroll);
                scroll += 1;
            }
            if (scroll > 1)
            {
                scroll -= (int)scroll;
            }
            for (int i = 0; i < Elements.Length; i++)
            {
                if (Elements[i] != null)
                {
                    if (side == ParallaxSide.VERTICAL)
                    {
                        float offset = Elements[i].OffsetElement + scroll;

                        offset -= (int)offset;
                        if (Mathf.Abs(offset - Elements[i].OldScroll) > 0.5f && Elements[i].OldScroll != Elements[i].OffsetElement)
                        {
                            Elements[i].outSide();
                        }
                        Elements[i].OldScroll = offset;
                        float pY = convertOffsetToPos(offset, totalSize);
                        Elements[i].transform.setLocaPosY(pY);
                    }
                }
            }
        }
    }
}

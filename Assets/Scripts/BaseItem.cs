using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class EventOnDataItem : UnityEvent<object>
{

}

public class BaseItem<T> : MonoBehaviour where T : new()
{
    // [HideInInspector]
    public T _info;
    [HideInInspector]
    public List<EventOnDataItem> _onData = new List<EventOnDataItem>();
    [HideInInspector]
    public int _indexItem;
    protected int cacheDepth = 999999;
    public void setDepth(int depth)
    {
        if (cacheDepth == depth) return;
        cacheDepth = depth;
        var widgets = GetComponentsInChildren<UIWidget>(true);
        int minDepth = 99999;
        foreach (var widget in widgets)
        {
            if (widget.depth < minDepth)
            {
                minDepth = widget.depth;
            }
        }
       if(minDepth < depth)
        {
            int delta = depth - minDepth;
            foreach (var widget in widgets)
            {
                widget.depth += delta;
            }
        }
    }

    public virtual void empty()
    {
        var pLabels = GetComponentsInChildren<UILabel>();
        foreach (UILabel pLabel in pLabels)
        {
            pLabel.text = "";
        }
    }

    public virtual void setInfo(T pInfo)    
    {
        _info = pInfo;
    }


    public virtual void onExecute(int index)
    {
        if (_onData != null && index < _onData.Count)
        {
            _onData[index].Invoke(_info);
        }
    }

    public virtual void onExecuteFirst()
    {
        onExecute(0);
    }

    public virtual void onExecuteSecond()
    {
        onExecute(1);
    }
    public virtual void onExecuteThird()
    {
        onExecute(2);
    }
}

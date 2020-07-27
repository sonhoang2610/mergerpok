using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class NGUISupport
{
    public static T addChildNGUI<T>(this GameObject pParent, T prefab) where T : Component
    {
        var pItem = pParent.AddChild<T>(prefab);
        if (pParent.transform.GetComponent<UIPanel>())
        {
            return pItem;
        }
        int depth = pParent.GetComponentInParent<UIWidget>().depth;
        int mainDepth = pItem.GetComponentInChildren<UIWidget>().depth;
        var widgets = pItem.GetComponentsInChildren<UIWidget>();
        foreach (var pWidget in widgets)
        {
            pWidget.depth = depth + (pWidget.depth - mainDepth);
        }
        return pItem;
    }

    public static GameObject addChildGameObjectNGUI(this GameObject pParent, GameObject prefab) 
    {
        var pItem = pParent.AddChild(prefab);
        if (pParent.transform.GetComponent<UIPanel>())
        {
            return pItem;
        }
        int depth = pParent.GetComponentInParent<UIWidget>().depth;
        int mainDepth = pItem.GetComponentInChildren<UIWidget>().depth;
        var widgets = pItem.GetComponentsInChildren<UIWidget>();
        foreach (var pWidget in widgets)
        {
            pWidget.depth = depth + (pWidget.depth - mainDepth) + 1;
        }
        return pItem;
    }
}
public class BaseBox<T, T0, T1> : BaseNormalBox<T0,T1> where T0 : BaseItem<T1> where T1 : new() where T : Component
{

    protected static T _instance;

    public static T InstanceRaw
    {
        get
        {
            return _instance;
        }
    }

    /// <summary>
    /// Singleton design pattern
    /// </summary>
    /// <value>The instance.</value>
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    _instance = obj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// On awake, we initialize our instance. Make sure to call base.Awake() in override if you need awake.
    /// </summary>
    protected virtual void Awake()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        _instance = this as T;
    }

 

    protected virtual void OnEnable()
    {
    }

    protected virtual void OnDisable()
    {
    }
}

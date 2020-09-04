using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseNormalBox<T0, T1> : MonoBehaviour where T0 : BaseItem<T1> where T1 : new()
{
    //[HideInInspector]
    public T1[] datas;
    public List<T0> items = new List<T0>();
    public EventOnDataItem[] actionWithData;
    [SerializeField]
    protected GameObject attachMent;
    [SerializeField]
    protected T0 prefabItem;
    [SerializeField]
    protected GameObject loading;
    public bool createItemEmpty = false;
    public bool autoExpand = true;
    public int limitItem = 10;
    public UIScrollView _scrollView;
    protected int oldIndex = 0;

    public List<T0> getActiveItems()
    {
        return items.FindAll(x => x.gameObject.activeSelf);
    }

    public GameObject AttachMent
    {
        get
        {
            return attachMent;
        }

        set
        {
            attachMent = value;
        }
    }

    public void WrapData(GameObject pObject, int index, int index1)
    {
        pObject.GetComponent<T0>()._indexItem = -index1;
        pObject.GetComponent<T0>().setInfo(datas[-index1]);
    }

    public virtual void executeInfos(T1[] pInfo1s)
    {
        datas = pInfo1s;
        var pWrap = AttachMent.GetComponent<UIWrapContent>();
        if (pWrap)
        {
            pWrap.minIndex = -pInfo1s.Length + 1;
            pWrap.onInitializeItem = WrapData;
        }
        List<T1> pList = new List<T1>();
        pList.AddRange(pInfo1s);
        if (createItemEmpty && pInfo1s.Length < limitItem)
        {
            for (int i = 0; i < limitItem - pInfo1s.Length; ++i)
            {
                pList.Add(new T1());
            }
        }
        T1[] pInfos = pList.ToArray();
        oldIndex = 0;
        for (int i = 0; i < items.Count; ++i)
        {
            items[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < Mathf.Max(pInfos.Length, items.Count); ++i)
        {

            if (i < pInfos.Length)
            {
                T0 pItem = null;
                if (i < items.Count)
                {
                    pItem = items[i];
                }
                else
                {
                    if (!autoExpand)
                    {
                        continue;
                    }
                    pItem = Instantiate<T0>(prefabItem, AttachMent.transform);
                    items.Add(pItem);
                }
                items[i]._indexItem = oldIndex;
                pItem.gameObject.SetActive(true);
                var widgets = items[i].GetComponentsInChildren<UIWidget>();
                items[i].setInfo(pInfos[i]);
                var widget = attachMent.GetComponentInParents<UIWidget>();
                if (!widget)
                {
                    widget = attachMent.transform.parent.GetComponent<UIWidget>();
                }
                if (widget)
                {
                    pItem.setDepth(widget.depth + 1);
                }
                // pItem.
   
                if (i >= pInfo1s.Length && createItemEmpty)
                {
                    items[i].empty();
                }

                if (actionWithData != null)
                {
                    items[i]._onData.addFromList(actionWithData);
                }
                oldIndex++;
            }
        }
        UIGrid grid;
        UITable table;
        if (grid = AttachMent.GetComponent<UIGrid>())
        {
            grid.enabled = true;
        }
        else if (table = AttachMent.GetComponent<UITable>())
        {
            table.enabled = true;
        }
        AttachMent.SendMessage("Reposition", AttachMent, SendMessageOptions.DontRequireReceiver);
        //if (pWrap)
        //{
        //    pWrap.ResetChildPositions();
        //    _scrollView?.ResetPosition();
        //}
    }

    public virtual void addMoreInfo(T1 pInfo)
    {
        oldIndex++;
        T0 pItem = Instantiate<T0>(prefabItem, AttachMent.transform);
        items.Add(pItem);
        pItem.gameObject.SetActive(true);
        pItem.setInfo(pInfo);
        pItem._indexItem = oldIndex;
        UIGrid grid;
        UITable table;
        if (grid = AttachMent.GetComponent<UIGrid>())
        {
            grid.enabled = true;
        }
        else if (table = AttachMent.GetComponent<UITable>())
        {
            table.enabled = true;
        }
        AttachMent.SendMessage("Reposition", AttachMent, SendMessageOptions.DontRequireReceiver);
    }


}

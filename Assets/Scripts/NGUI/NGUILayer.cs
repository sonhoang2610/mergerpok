using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NGUILayer : MonoBehaviour
{
    public void BringForward(GameObject pObject)
    {
        NGUITools.BringForward(pObject);
    }
    [ContextMenu("BringForward")]
    public void BringForward()
    {
        NGUITools.BringForward(gameObject);
    }
}

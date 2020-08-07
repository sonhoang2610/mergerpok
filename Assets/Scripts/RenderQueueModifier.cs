using UnityEngine;
using System.Collections;
using System;
using Spine.Unity;


[System.Serializable]
public class MatRenderOrder
{
   public Material mat;
    public int delta;
}


public class RenderQueueModifier : MonoBehaviour
{
    public enum RenderType
    {
        FRONT,
        BACK,
        SAME
    }

    public UIWidget m_target = null;
    public RenderType m_type = RenderType.FRONT;
    public bool runOnUpdateObjectRender = true;
    public bool sharedMat = true;
    public bool isIncludeChild = true;
    public MatRenderOrder[] mats;
    Renderer[] _renderers;
    int _lastQueue = 0;

    SkeletonAnimation customMat;

    public bool isHaveMat
    {
        get
        {
             if(mats != null && mats.Length != 0)
            {
                return true;
            }
            return false;
        }
    }
    public void setTarget(UIWidget pTarget)
    {
        updateAtleatOne = false;
        if (pTarget != null && m_target != pTarget)
        {
            pTarget.onRender += resort;
        }
        m_target = pTarget;
    }
    
    int index = 0;
    protected bool updateAtleatOne = false;
    private void OnEnable()
    {
        isStarted = false;
        _lastQueue = 0;
        updateAtleatOne = false;
        refreshRender();
        if (m_target)
        {
            m_target.onRender -= resort;
            m_target.onRender += resort;
        }
    }


    public void refreshRender()
    {
        if (m_target == null)
            return;
        if (_renderers == null)
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
        }
        if (!isHaveMat)
        {
            foreach (Renderer r in _renderers)
            {
                if (!isIncludeChild && r.gameObject != gameObject) continue;
                if (m_target.drawCall != null)
                {
                    updateAtleatOne = true;
                    if (sharedMat)
                    {
                        if (r.sharedMaterials != null)
                        {
                            for (int i = 0; i < r.sharedMaterials.Length; ++i)
                            {
                                if (r.sharedMaterials[i] != null)
                                {
                                    r.sharedMaterials[i].renderQueue = m_target.drawCall.renderQueue;
                                }
                            }
                        }
                        else
                        {
                            r.sharedMaterial.renderQueue = m_target.drawCall.renderQueue;
                        }
                    }
                    else
                    {
                        // r.sharedMaterial.renderQueue = m_target.drawCall.renderQueue;
                    }
                }
            }
        }
        else
        {
            if (m_target.drawCall)
            {
                updateAtleatOne = true;
                for (int i = 0; i < mats.Length; ++i)
                {
                    mats[i].mat.renderQueue = m_target.drawCall.renderQueue + mats[i].delta;
                }
            }
        }
    }

    private void OnDisable()
    {
        if (m_target == null || m_target.IsDestroyed())
            return;
        m_target.onRender -= resort;
    }
    void Start()
    {
        _renderers = GetComponentsInChildren<Renderer>(true);
        customMat = GetComponent<SkeletonAnimation>();
        updateAtleatOne = false;
    }

    float currentTime = 0;
    bool isStarted = false;
    [ContextMenu("Resort")]
    public void resort(Material mat)
    {
        updateAtleatOne = true;
        if (runOnUpdateObjectRender || index == 0)
        {
            index++;
            int queue = mat.renderQueue;
            queue += m_type == RenderType.FRONT ? 1 : (m_type == RenderType.BACK ? -1 : 0);
            if (_lastQueue != queue)
            {
              //  Debug.Log("change Queue" + gameObject.name);
                currentTime = 0;
                _lastQueue = queue;
                if (!isHaveMat)
                {
                    if (_renderers != null)
                    {
                        if (sharedMat)
                        {
                            foreach (Renderer r in _renderers)
                            {
                                if (!isIncludeChild && r.gameObject != gameObject) continue;
                                if(r.GetType() == typeof(SpriteRenderer))
                                {
                                    if (r.material == null) continue;
                                    r.material.renderQueue = _lastQueue;
                                    continue;
                                }
                                if (r.sharedMaterials != null)
                                {
                                    for (int i = 0; i < r.sharedMaterials.Length; ++i)
                                    {
                                        if (r.sharedMaterials[i] != null)
                                        {
                                            r.sharedMaterials[i].renderQueue = _lastQueue;
                                        }
                                    }
                                }
                                else
                                {
                                    r.sharedMaterial.renderQueue = _lastQueue;
                                }

                            }
                        }
                        else
                        {
                            foreach (Renderer r in _renderers)
                            {
                                if (!isIncludeChild && r.gameObject != gameObject) continue;
                                r.material.renderQueue = _lastQueue;
                            }
                        }
                    }
                }
                else
                {
                    for(int i = 0; i < mats.Length; ++i)
                    {
                        mats[i].mat.renderQueue = _lastQueue + mats[i].delta;
                    }
                }
                //}
            }
        }

    }
    private void Update()
    {
        if (!updateAtleatOne)
        {   
            refreshRender();
        }
    }
}

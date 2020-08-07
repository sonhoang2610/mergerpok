using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ParticleSystemAutoDestroy : MonoBehaviour {

    private ParticleSystem ps;
    public bool isDestroy = false;
    public float forceDestroyAfter = -1;
    public UnityEvent onStart;
    public UnityEvent onDestroy;
    float currentTime = 0;
    public void Start()
    {
        ps = GetComponent<ParticleSystem>();

    }
    private void OnEnable()
    {
        currentTime = forceDestroyAfter;
        onStart.Invoke();
    }
    public void Update()
    {
        if(currentTime >= 0)
        {
            currentTime -= Time.deltaTime;
            if(currentTime <= 0)
            {
           
                onDestroy.Invoke();
                if (!isDestroy)
                {
                    gameObject.SetActive(false);
                }
                else
                {
                    Destroy(gameObject);
                }
   
            }
        }
        if (ps)
        {
            if (!ps.IsAlive())
            {
                onDestroy.Invoke();
                if (!isDestroy)
                {
                    gameObject.SetActive(false);
                }
                else
                {
                    Destroy(gameObject);
                }
             
            }
        }
    }

    public  void detachFromParent()
    {
        transform.parent = null;
    }

    public  void setParent( Transform pTransParent)
    {
        transform.parent = pTransParent;
        transform.localPosition = Vector3.zero;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleResolution : MonoBehaviour {
    [SerializeField]
    Vector2 defaultResolution;
    [SerializeField]
    UIPanel panel;
    [SerializeField]
    bool scalePlatform = false;
    private void Start()
    {
        if (!ScaleConfig.Instance) return;
        if (!ScaleConfig.Instance.isEnable) return;
        if (!ScaleConfig.Instance.isContraint)
        {
            //TopLayer.Instance.showToast("Dont Scale", 4);
            Vector3 pScale = new Vector3(1, 1, 1);
            pScale.y = panel.GetWindowSize().y / defaultResolution.y;
            pScale.x = panel.GetWindowSize().x / defaultResolution.x;
            if(pScale.y> pScale.x)
            {
                pScale.y = pScale.x;
            }
            else
            {
                pScale.x = pScale.y;
            }
            transform.localScale = pScale;
        }
        else
        {

            //TopLayer.Instance.showToast("Scale",4);
            float tileOriginal = defaultResolution.x / defaultResolution.y;
            float tileScreen = panel.GetWindowSize().x / panel.GetWindowSize().y;
            if (tileOriginal > tileScreen)
            {
                Vector3 pScale = new Vector3(1, 1, 1)
                {
                    x = panel.GetWindowSize().x / defaultResolution.x
                };
                pScale.y = pScale.x;
                transform.localScale = pScale;
            }
            else
            {
                Vector3 pScale = new Vector3(1, 1, 1)
                {
                    y = panel.GetWindowSize().y / defaultResolution.y
                };
                pScale.x = pScale.y;
                transform.localScale = pScale;
            }
        }
        if (scalePlatform)
        {
#if UNITY_WEBGL
             transform.localScale*=0.75f;
#endif
        }
    }
    private void Awake()
    {
       
    }
}

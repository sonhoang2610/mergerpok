using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropCamera : MonoBehaviour
{
    public UIPanel root;
    public Vector2 realRatio = new Vector2(1080,1920);
    public bool cropY = false;
    [ContextMenu("log")]
    public void cropCamera()
    {
      var pCam =  GetComponent<Camera>();
        var pRoot = root.GetComponent<UIRoot>();
        var pRealRatio = (float)realRatio.x / (float)realRatio.y;
        float ratio = root.GetViewSize().x / root.GetViewSize().y;

        if (pRealRatio < ratio)
        {
            float pWidth = root.GetWindowSize().y * pRealRatio / root.GetWindowSize().x;
            pCam.rect = new Rect(0.5f- pWidth/2, 0, root.GetWindowSize().y * pRealRatio / root.GetWindowSize().x, 1);
        }
        else if(cropY)
        {
            float pHeight = (root.GetWindowSize().x / pRealRatio) / root.GetWindowSize().y;
            pCam.rect = new Rect(0, 0.5f- pHeight/2, 1, pHeight);
            Debug.Log("rect" + pCam.rect.x + "." + pCam.rect.y);
        }
        
    }
    bool isClearRender = false;
    [ContextMenu("Clear Manual")]
    public void clearRender()
    {

        isClearRender = true;
       
    }
    private void OnPostRender()
    {
        if (isClearRender)
        {
            isClearRender = false;
            //GL.Clear(true, true, Color.black);
        }
    }
    public void OnRenderObject()
    {
 
    }

        // Start is called before the first frame update
        void Start()
    {

        cropCamera();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

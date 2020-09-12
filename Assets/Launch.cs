using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class Launch : MonoBehaviour
{
   public AssetReference scene;
    // Start is called before the first frame update
    void Start()
    {
        var async = scene.LoadSceneAsync();
    }
    
}

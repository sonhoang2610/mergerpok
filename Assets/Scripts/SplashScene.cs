using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok {
    public class SplashScene : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            SceneManager.Instance.loadScene("MainScene");
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

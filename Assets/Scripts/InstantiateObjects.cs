using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateObjects : MonoBehaviour
{

    public GameObject[] _objects;
    public void excute()
    {
        for(int i = 0; i < _objects.Length; ++i)
        {
            Instantiate(_objects[i]);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

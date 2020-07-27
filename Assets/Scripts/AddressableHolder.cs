using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;
using System;
using System.Linq;

    public class AddressableHolder : PersistentSingleton<AddressableHolder>
    {
        public Dictionary<Type, List<UnityEngine.Object>> assetLoaded = new Dictionary<Type, List<UnityEngine.Object>>();

        public void addLoadedItem<T>(IList<T> assets) where T : UnityEngine.Object
        {
            for(int i = 0; i < assets.Count; ++i)
            {
                if (!assetLoaded.ContainsKey(assets[i].GetType()))
                {
                    assetLoaded.Add(assets[i].GetType(),new List<UnityEngine.Object>() { assets[i] });
                }
                assetLoaded[assets[i].GetType()].Add(assets[i]);
            }
         
         
        }

        public T FindAsset<T>() where T : UnityEngine.Object
        {
            if (assetLoaded.ContainsKey(typeof(T)))
            {
                return (T)assetLoaded[typeof(T)][0];
            }
            return null;
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

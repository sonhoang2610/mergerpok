using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AddressableAssets;

namespace Pok {
    public class ItemVip : MonoBehaviour
    {
        public AssetReference assetVip;
        public UnityEvent onExistVip;
        public UnityEvent onNoVip;
        private void OnEnable()
        {
            StartCoroutine(onEnable());
        }

        private IEnumerator onEnable()
        {
            while (!GameManager.readyForThisState("Main"))
            {
                yield return new WaitForEndOfFrame();
            }
            assetVip.loadAssetWrapped<BaseItemGame>((o) =>
            {
                if (o)
                {
                    var exist = GameManager.Instance.Database.getItem(o.ItemID);
                    if (exist.QuantityBig > 0)
                    {
                        onExistVip.Invoke();
                    }
                    else
                    {
                        onNoVip.Invoke();
                    }
                }
            });
 
        }
    }
}

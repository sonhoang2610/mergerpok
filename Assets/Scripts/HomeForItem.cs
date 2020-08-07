using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Pok
{
    public interface IHomeForItem
    {
        string[] itemRegisted();
    }
    public class HomeForItem : MonoBehaviour , IHomeForItem
    {
        public string[] items;

        public string[] itemRegisted()
        {
            return items;
        }
    }
}

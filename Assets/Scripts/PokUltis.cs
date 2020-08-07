using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{

    public class NoAds : IBoosterExecute
    {
#if UNITY_EDITOR
        [UnityEditor.Callbacks.DidReloadScripts]
#endif
        static void AutoRegister() {
            if (!ItemBoosterObject.boosterType.ContainsKey("NoAds"))
            {
                ItemBoosterObject.boosterType.Add("NoAds", typeof(NoAds));
            }
        }
        public void Excute(Dictionary<string,object> blackBoard)
        {
            ES3.Save("BlockADS", 1);
        }

        public string getContent(Dictionary<string, object> blackBoard)
        {
            return "Block ADS";
        }
    }

    public class SuperInCome : IBoosterExecute
    {
#if UNITY_EDITOR
        [UnityEditor.Callbacks.DidReloadScripts]
#endif
        static void AutoRegister()
        {
            if (!ItemBoosterObject.boosterType.ContainsKey(nameof(SuperInCome)))
            {
                ItemBoosterObject.boosterType.Add(nameof(SuperInCome), typeof(SuperInCome));
            }
        }
        public void Excute(Dictionary<string, object> blackBoard)
        {
            var factor = (float)blackBoard["SuperInCome"];
            double time = blackBoard["time"].GetType() == typeof(double) ? (double)blackBoard["time"] : Random.Range(((Vector2Int)blackBoard["time"]).x, ((Vector2Int)blackBoard["time"]).y);
            GameManager.Instance.addFactorSuperIncome(factor, time);
        }

        public string getContent(Dictionary<string, object> blackBoard)
        {
            var factor = (float)blackBoard["SuperInCome"];
            return "x" + factor + " Gold";
        }
    }
    public class DiscountCreature : IBoosterExecute
    {
#if UNITY_EDITOR
        [UnityEditor.Callbacks.DidReloadScripts]
#endif
        static void AutoRegister()
        {
            if (!ItemBoosterObject.boosterType.ContainsKey(nameof(DiscountCreature)))
            {
                ItemBoosterObject.boosterType.Add(nameof(DiscountCreature), typeof(DiscountCreature));
            }
        }
        public void Excute(Dictionary<string, object> blackBoard)
        {
            var discount = (float)blackBoard["DiscountCreature"];
            double time =  blackBoard["time"].GetType() ==  typeof(double) ? (double)blackBoard["time"] : Random.Range(((Vector2Int)blackBoard["time"]).x, ((Vector2Int)blackBoard["time"]).y);
            GameManager.Instance.DiscountCreature(discount, time);
        }

        public string getContent(Dictionary<string, object> blackBoard)
        {
            return "Discount";
        }
    }
    public class ReduceTimeEgg : IBoosterExecute
    {
#if UNITY_EDITOR
        [UnityEditor.Callbacks.DidReloadScripts]
#endif
        static void AutoRegister()
        {
            if (!ItemBoosterObject.boosterType.ContainsKey(nameof(ReduceTimeEgg)))
            {
                ItemBoosterObject.boosterType.Add(nameof(ReduceTimeEgg), typeof(ReduceTimeEgg));
            }
        }
        public void Excute(Dictionary<string, object> blackBoard)
        {
            var discount = (float)blackBoard["ReduceTimeEgg"];
            double time = blackBoard["time"].GetType() == typeof(double) ? (double)blackBoard["time"] : Random.Range(((Vector2Int)blackBoard["time"]).x, ((Vector2Int)blackBoard["time"]).y);
            GameManager.Instance.ReduceTimeEgg(discount, time);
        }

        public string getContent(Dictionary<string, object> blackBoard)
        {
            return "Reduce Time Egg";
        }
    }
}


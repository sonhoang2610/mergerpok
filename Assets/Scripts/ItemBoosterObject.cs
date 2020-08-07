using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pok
{
    public interface IBoosterExecute
    {
        void Excute(Dictionary<string, object> blackBoard);
        string getContent(Dictionary<string, object> blackBoard);
    }
    [CreateAssetMenu(fileName = "ExecuteItem",menuName = "Pok/ExecuteItem")]
    public class ItemBoosterObject : BaseItemGame, IUsageItem
    {
        public static Dictionary<string, Type> boosterType = new Dictionary<string, Type>();
        [ValueDropdown("ValuesFunction")]
        public string boosterID;
        public Dictionary<string, object> blackBoard = new Dictionary<string, object>();
        public override string getContent()
        {
            if (!boosterType.ContainsKey(boosterID)) return base.getContent();
            IBoosterExecute execute = (IBoosterExecute)Activator.CreateInstance(boosterType[boosterID]);
            return execute.getContent(blackBoard);
        }
        private IList<ValueDropdownItem<string>> ValuesFunction()
        {
            var list = new ValueDropdownList<string>();
            for(int i = 0; i < boosterType.Count; ++i)
            {
                list.Add(boosterType.ElementAt(i).Key, boosterType.ElementAt(i).Key);
            }
            return list;
        }
        public void executeBooster()
        {
            if (!boosterType.ContainsKey(boosterID)) return;
            IBoosterExecute execute =(IBoosterExecute) Activator.CreateInstance(boosterType[boosterID]);
            execute.Excute(blackBoard);
        }

        public bool useWhenClaim()
        {
            return true;
        }
    }
}

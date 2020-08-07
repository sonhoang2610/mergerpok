using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{
    [CreateAssetMenu(fileName ="MissionItem",menuName = "Pok/MissionItem")]
    public class ItemMissionObject : BaseItemGame
    {
        public float timeMission;
        public ItemWithQuantity[] itemReward;
    }

}

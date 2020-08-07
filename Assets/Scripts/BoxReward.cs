using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{

    public class BoxReward : BaseNormalBox<ItemRewardUI, ItemRewardInfo>
    {
        public UIElement container;
        public UILabel titlelbl;
        public void show(ItemRewardInfo[] infos,string title = "Reward")
        {
            attachMent.GetComponent<UIGrid>().cellWidth = infos.Length <= 2 ? 300 : 200;
            container.show();
            executeInfos(infos);
            titlelbl.text = title;
        }
    }
}

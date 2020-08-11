using EasyMobile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.AddressableAssets;

namespace Pok
{
    public class VipInfo
    {

    }
    public class BoxVip : BaseItem<BaseItemGame>
    {
        public AssetReference item;
        public UILabel factor, percent, addDiamond;
        public UIElement container;

        public void show()
        {
            container.show();
        }

        public void subscription()
        {
            item.loadAssetWrapped<BaseItemGame>((o) => {
                if (o != null)
                {
                    GameManager.Instance.RequestInappForItem(o.ItemID, (b) =>
                    {
                        if (b)
                        {
                            container.close();
                            var exist = GameManager.Instance.Database.getItem(o.ItemID);
                            exist.setQuantity("1");
                            HUDManager.Instance.boxReward.show(new ItemRewardInfo[] { new ItemRewardInfo() {
                                itemReward = new ItemWithQuantity()
                                {
                                    item= o,
                                    quantity="1"
                                }
                            } }, "Become Vip Member");
                        }
                    });
                }
            });
        }


    }
}

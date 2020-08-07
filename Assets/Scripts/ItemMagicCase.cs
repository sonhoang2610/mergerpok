using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pok
{
    [System.Serializable]
    public class ItemMagicCaseInfo
    {
        public BaseItemGame item;
        public string content;
        public string quantity;
    }

    public class ItemMagicCase : MonoBehaviour,IElementReset
    {
        public UI2DSprite icon;
        public UILabel content;

        public BaseItemGame[] _infos;
        protected bool isFixSkin = false;
        protected Vector2Int sizeCache;
        protected int fixSkinIndex;
        private void Awake()
        {
            sizeCache = new Vector2Int(icon.width, icon.height);
        }

        public void setInfo(BaseItemGame[] items)
        {
            _infos = items;
            ramdonSkin();
        }
        [System.NonSerialized]
        public ItemMagicCaseInfo lastItem;
        public ItemMagicCaseInfo getItemFromBaseItem(BaseItemGame item)
        {
            if (typeof(IExtractItem).IsAssignableFrom(item.GetType()))
            {
                var itemExtras = ((IExtractItem)item).ExtractHere();
                return lastItem = (new ItemMagicCaseInfo() { item = itemExtras[0].item, content = itemExtras[0].quantity.clearDot().ToKMBTA(),quantity = itemExtras[0].quantity.clearDot() });
            }
            return lastItem = (new ItemMagicCaseInfo() { item = item, content = item.getContent(),quantity = "1" });
        }
        public void ramdonSkin()
        {
            if (!isFixSkin)
            {
                var random = Random.Range(0, _infos.Length);
                var info = getItemFromBaseItem(_infos[random]);
                setSkinInfo(info);
            }
            else
            {
                var info = getItemFromBaseItem(_infos[fixSkinIndex]);
                setSkinInfo(info);
            }
        }

        public void setSkinInfo(ItemMagicCaseInfo info)
        {
            info.item.getSpriteForState((o) => { icon.sprite2D = o; icon.MakePixelPerfectClaimIn(sizeCache); });
            content.text = info.content;
        }

        public void fixSkin(int index)
        {
            fixSkinIndex = index;
            isFixSkin = true;
        }

        public void resetToBegin()
        {
            isFixSkin = false;
        }
    }

}
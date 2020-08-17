using DG.Tweening;
using EazyEngine.Tools;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pok
{
    public class BoxRewardADS : BaseNormalBox<ItemPackRewardADS, ItemPackRewardADSInfo>
    {
        public ItemMagicCaseConfig[] infos;
        public GameObject blockTouch;
        public float delayStart;
        public float delayMax;
        public float timeRoll;
        public float durationMax;

        protected float CurrentSpeed { get; set; }
        protected float CurrentTime { get; set; }
        protected int resultIndex;
        public void startRoll()
        {
            CurrentTime = 0;
            CurrentSpeed = delayStart;
            blockTouch.gameObject.SetActive(true);
            var Sequence = DOTween.Sequence();
            Sequence.Append( DOTween.To(() => CurrentSpeed, x => CurrentSpeed = x, delayMax, (timeRoll- durationMax) / 2));
            Sequence.AppendInterval(durationMax);
            Sequence.Append(DOTween.To(() => CurrentSpeed, x => CurrentSpeed = x, delayStart, (timeRoll - durationMax) / 2));
            StartCoroutine(roll(0, Random.Range(0, infos.Length), false));

            List<ItemMagicCaseConfig> wheels = new List<ItemMagicCaseConfig>(GameDatabase.Instance.containerRewardADS.ToArray());
            wheels.Shuffle();
            float random = Random.Range(0.0f, 1.0f);
            float currentPercent = 1;
            ItemMagicCaseConfig result = null;
            for (int i = 0; i < wheels.Count; ++i)
            {
                currentPercent -= wheels[i].rate;
                if (random > currentPercent)
                {
                    result = wheels[i];
                    break;
                }
                if (i == wheels.Count - 1)
                {
                    result = wheels[i];
                }
            }
             resultIndex =  System.Array.IndexOf(infos,result);
        }
        public IEnumerator roll(float time,int oldIndex,bool end)
        {
            yield return new WaitForSeconds(time);
            int random = Random.Range(0, infos.Length);
            while(random == oldIndex )
            {
                random = Random.Range(0, infos.Length);
            }
            if(end)
            {
                random = resultIndex;
            }
            for (int i = 0; i < items.Count; ++i)
            {
                items[i].choose(i == random);
            }
            SoundManager.Instance.PlaySound("BoxRewardRun");
            CurrentTime += time;
            if (!end)
            {
                StartCoroutine(roll(CurrentSpeed, random, CurrentTime > timeRoll));
            }
            else
            {
                onStop();
            }
        }

        public void onStop()
        {
            SoundManager.Instance.PlaySound("RewardSpin");
            var item = items[resultIndex]._info;
            claimItem(new ItemWithQuantity() { item = item.item, quantity = item.quantity });
            StartCoroutine(delayAction(2, delegate
            {
                GetComponent<UIElement>().close();
            }));

        }
        public IEnumerator delayAction(float sec, System.Action action)
        {
            yield return new WaitForSeconds(sec);
            action?.Invoke();
        }
        public void claimItem(ItemWithQuantity item)
        {
            if (typeof(CreatureItem).IsAssignableFrom(item.item.GetType()))
            {
                var creature = ((CreatureItem)item.item);
                var zone = GameDatabase.Instance.ZoneCollection.Find(x => x.ItemID == GameManager.Instance.ZoneChoosed);
                var pack = zone.getPackage();
                var mapParent = GameManager.Instance.Database.worldData.zones.Find(x => x.id == GameManager.Instance.ZoneChoosed).maps.Find(x => creature.parentMap && x.id == creature.parentMap.ItemID);
                var newCreature = new PackageCreatureInstanceSaved() { creature = creature.ItemID, id = pack.package.ItemID, instanceID = GameManager.Instance.GenerateID.ToString(), mapParent = mapParent };
                GameManager.Instance.GenerateID++;
                GameManager.Instance.Database.worldData.addCreature(newCreature, GameManager.Instance.ZoneChoosed);
                addCreatureObject(newCreature);
                EzEventManager.TriggerEvent(new AddCreatureEvent() { change = 1, creature = newCreature, zoneid = GameManager.Instance.ZoneChoosed, manualByHand = false });
            }
            else
            {
                AddModelItemClaim(item, delegate {
                    GameManager.Instance.claimItem(item.item, item.quantity);
                });

            }
        }

        public void AddModelItemClaim(ItemWithQuantity item, System.Action onComplete)
        {
            var slots = FindObjectsOfType<InventorySlot>();
            GameObject home = null;
            foreach (var slot in slots)
            {
                if (slot._info != null && slot._info.item.ItemID == item.item.ItemID)
                {
                    home = slot.gameObject;
                }
            }
            if (home == null)
            {
                var Homes = FindObjectsOfType<MonoBehaviour>().OfType<IHomeForItem>();
                foreach (var localHome in Homes)
                {
                    if (System.Array.Exists(localHome.itemRegisted(), x => x == item.item.ItemID))
                    {
                        home = ((MonoBehaviour)localHome).gameObject;
                    }
                }
            }
            GameObject newObject = new GameObject();
            var state = "Default";
            if (item.item.ItemID.Contains("SuperInCome"))
            {
                if (GameManager.Instance.getFactorIncome().x >= 2)
                {
                    state = "X4";
                }
            }
            var sprite = newObject.AddComponent<UI2DSprite>();
            item.item.getSpriteForState((o) =>
            {
                sprite.sprite2D = o;
                sprite.MakePixelPerfectClaimIn(new Vector2Int(200, 200));
            }, state);

            newObject.transform.parent = blockTouch.transform.parent;
            newObject.transform.localScale = new Vector3(1, 1, 1);
            newObject.transform.localPosition = Vector3.zero;
            newObject.gameObject.SetLayerRecursively(gameObject.layer);
            newObject.gameObject.SetActive(true);
            newObject.transform.localPosition = UnityEngine.Vector3.zero;
            var seq = DOTween.Sequence();
            seq.Append(newObject.transform.DOScale(2, 0.5f).SetEase(Ease.OutQuart));
            seq.Append(newObject.transform.DOScale(0.2f, 0.5f));
            if (home)
            {
                seq.Join(newObject.transform.DOMove(home.transform.position, 0.5f));
            }
            seq.AppendCallback(delegate
            {
                onComplete();
                Destroy(newObject);
            });
        }
        public void addCreatureObject(CreatureInstanceSaved pInfo)
        {
            var maps = FindObjectsOfType<MapItemInstanced>();
            GameObject home = null;
            foreach (var map in maps)
            {
                if (map._info != null && map._info.id == pInfo.mapParent.id)
                {
                    home = map.gameObject;
                }
            }
            var infosaved = GameManager.Instance.Database.creatureInfos.Find(x => x.id == pInfo.id);
            if (!infosaved.isUnLock) { infosaved.isUnLock = true; }
            var dataCreature = GameDatabase.Instance.CreatureCollection.Find(x => x.ItemID == pInfo.id);

            if (dataCreature == null)
            {
                dataCreature = (CreatureItem)GameDatabase.Instance.getItemInventory(pInfo.id);
            }
            dataCreature.getModelForState((o) =>
            {
                var creatureObject = blockTouch.transform.parent.AddChild(o).GetComponent<Creature>();
                NGUITools.BringForward(blockTouch.transform.parent.gameObject);
                creatureObject.gameObject.SetLayerRecursively(gameObject.layer);
                creatureObject.transform.localScale = o.transform.localScale;
                creatureObject.gameObject.SetActive(true);
                creatureObject.transform.localPosition = UnityEngine.Vector3.zero;
                creatureObject.setInfo(pInfo);
                var seq = DOTween.Sequence();
                seq.Append(creatureObject.transform.DOScale(2, 0.5f).SetEase(Ease.OutQuart));
                seq.Append(creatureObject.transform.DOScale(0.2f, 0.5f));
                if (home)
                {
                    seq.Join(creatureObject.transform.DOMove(home.transform.position, 0.5f));
                }
                seq.AppendCallback(delegate
                {
                    Destroy(creatureObject.gameObject);
                });
            });

        }
        private void OnEnable()
        {
            blockTouch.gameObject.SetActive(false);
            infos = GameDatabase.Instance.containerRewardADS.ToArray();
            List<ItemMagicCaseConfig> newList = new List<ItemMagicCaseConfig>(infos);
            newList.Shuffle();
            infos = newList.ToArray();
            StartCoroutine(setup());
            blockTouch.gameObject.SetActive(false);
        }
        public IEnumerator setup()
        {
            yield return new WaitForEndOfFrame();
            var pInfos = new List<ItemPackRewardADSInfo>();
            for (int i = 0; i < infos.Length; ++i)
            {
                pInfos.Add(getItem(infos[i]));
            }
            executeInfos(pInfos.ToArray());
        }

        public ItemPackRewardADSInfo getItem(ItemMagicCaseConfig itemOriginal )
        {
            var item = new ItemPackRewardADSInfo();
            if (typeof(IExtractItem).IsAssignableFrom(itemOriginal.item.GetType()))
            {
               var items = ((IExtractItem)itemOriginal.item).ExtractHere();
                item.item = items[0].item;
                item.content = items[0].quantity.clearDot().ToKMBTA();
                item.quantity = items[0].quantity;
            }
            else if (typeof(ItemBoosterObject).IsAssignableFrom(itemOriginal.item.GetType()))
            {
                item.item = itemOriginal.item;
                item.content = itemOriginal.item.getContent();
                item.quantity = "1";
            }
            return item;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyEngine.Tools;
using UnityEngine.AddressableAssets;
using System.Numerics;
using DG.Tweening;

namespace Pok
{
    public enum TypeItem
    {
        CREATUREDOWN5,
        CREATUREDOWN2,
        GOLD,
        BIG_GOLD,
        DIAMOND,
        SPIN
    }

    [System.Serializable]
    public class WheelConfig
    {
        public TypeItem itemType;
        public float[] percentRoll = new float[] { 0.4f, 0.4f, 0.2f };
        public AssetReferenceSprite icon;
        public float change = 1;
        public AssetReferenceSprite getICon()
        {
            var creatureActive = GameManager.Instance.Database.getAllCreatureInfoInZone(GameManager.Instance.ZoneChoosed, true);
            int indexDown2 = (creatureActive.Length - 3).Clamp(creatureActive.Length, 0);
            int indexDown5 = (creatureActive.Length - 6).Clamp(creatureActive.Length, 0);

            if (itemType == TypeItem.CREATUREDOWN2)
            {
                return creatureActive[indexDown2].icons[0].Icon;
            }
            if (itemType == TypeItem.CREATUREDOWN5)
            {
                return creatureActive[indexDown5].icons[0].Icon;
            }
            return icon;
        }

        public ItemWithQuantity getItem(int result)
        {
            var creatureActive = GameManager.Instance.Database.getAllCreatureInfoInZone(GameManager.Instance.ZoneChoosed, true);
            int indexDown2 = (creatureActive.Length - 3).Clamp(creatureActive.Length, 0);
            int indexDown5 = (creatureActive.Length - 6).Clamp(creatureActive.Length, 0);

            switch (itemType)
            {
                case TypeItem.BIG_GOLD:
                    var bigGold = BigInteger.Parse(GameManager.Instance.getTotalGoldGrowthCurrentZone()) * (result == 1 ? 100 : (result == 3 ? 2000 : 200));
                    return new ItemWithQuantity() { item = GameDatabase.Instance.getItemInventory("Coin"), quantity = bigGold.ToString() };
                case TypeItem.CREATUREDOWN2:
                    return new ItemWithQuantity() { item = creatureActive[indexDown2], quantity = result == 3 ? "1" : "0" };
                case TypeItem.CREATUREDOWN5:
                    return new ItemWithQuantity() { item = creatureActive[indexDown5], quantity = result == 3 ? "1" : "0" };
                case TypeItem.GOLD:
                    int[] factor = new int[] { 20, 200, 400 };
                    var small = BigInteger.Parse(GameManager.Instance.getTotalGoldGrowthCurrentZone()) * factor[result - 1];
                    return new ItemWithQuantity() { item = GameDatabase.Instance.getItemInventory("Coin"), quantity = small.ToString() };
                case TypeItem.DIAMOND:
                    return new ItemWithQuantity() { item = GameDatabase.Instance.getItemInventory("Crystal"), quantity = result == 3 ? "5" : "0" };
                case TypeItem.SPIN:
                    return new ItemWithQuantity() { item = GameDatabase.Instance.getItemInventory("TicketSpin"), quantity = result == 3 ? "3" : "0" };
            }
            return new ItemWithQuantity();
        }
    }
    public class BoxWheelFortune : Singleton<BoxWheelFortune>, EzEventListener<GameDatabaseInventoryEvent>
    {
        public EazyParallax[] rolls;
        public UIElement container;
        public GameObject ticketLayer, DiamonLayer;
        public GameObject[] coinEffect;
        public GameObject blockTouch;
        public GameObject creatureTo;
        public UI2DSprite rewardIcon;
        public UILabel labelRewardQuantiy, labelRewardType;
        public void OnEzEvent(GameDatabaseInventoryEvent eventType)
        {
            if (eventType.item.item.ItemID == "TicketSpin")
            {
                updateLayer();
            }
        }
        public void updateLayer()
        {
            var ticket = GameManager.Instance.Database.getItem("TicketSpin");
            ticketLayer.gameObject.SetActive(ticket.QuantityBig > 0);
            DiamonLayer.gameObject.SetActive(ticket.QuantityBig <= 0);
        }
        public void addCreatureObject(CreatureInstanceSaved pInfo)
        {
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
                seq.Join(creatureObject.transform.DOMove(creatureTo.transform.position, 0.5f));
                seq.AppendCallback(delegate
                {
                    Destroy(creatureObject.gameObject);
                });
            });

        }
        private void OnEnable()
        {
            EzEventManager.AddListener(this);
        }
        private void OnDisable()
        {
            EzEventManager.RemoveListener(this);
            StopAllCoroutines();
            coinEffect[0].gameObject.SetActive(false);
            coinEffect[0].gameObject.SetActive(false);
        }

        public void show()
        {
            updateLayer();
            StartCoroutine(setup());
            container.show();
        }
        protected List<int> resultItem;
        public IEnumerator setup()
        {
            yield return new WaitForEndOfFrame();
            for (int i = 0; i < rolls.Length; ++i)
            {
                for (int j = 0; j < rolls[i].Elements.Length; ++j)
                {
                    rolls[i].callBackStop = onStop;
                    rolls[i].Elements[j].GetComponent<ItemWheel>().setInfos(GameDatabase.Instance.wheelMainConfig.ToArray());
                }
            }
        }
        protected int rollCount = 0;
        public void onStop()
        {
            rollCount--;
            if (rollCount == 0)
            {
                List<Vector2Int> ressultMerger = new List<Vector2Int>();
                for (int i = 0; i < resultItem.Count; ++i)
                {
                    if (!ressultMerger.Exists(x => x.x == resultItem[i]))
                    {
                        ressultMerger.Add(new Vector2Int(resultItem[i], 0));
                    }
                    var index = ressultMerger.FindIndex(x => x.x == resultItem[i]);
                    var vec = ressultMerger[index];
                    vec.y += 1;
                    ressultMerger[index] = vec;
                }
                List<ItemWithQuantity> items = new List<ItemWithQuantity>();
                for (int i = 0; i < ressultMerger.Count; ++i)
                {
                    var item = GameDatabase.Instance.wheelMainConfig[ressultMerger[i].x].getItem(ressultMerger[i].y);
                    if (!items.Exists(x => x.item == item.item))
                    {
                        items.Add(item);
                    }
                    else
                    {
                        var index = items.FindIndex(x => x.item == item.item);
                        var itemInfo = items[index];
                        itemInfo.quantity = (BigInteger.Parse(itemInfo.quantity) + BigInteger.Parse(item.quantity)).ToString();
                        items[index] = itemInfo;
                    }
                }
                items.Sort((a, b) => BigInteger.Parse(b.quantity).CompareTo(BigInteger.Parse(a.quantity)));
                var itemFinal = items[0];
                if (itemFinal.item.ItemID == "Coin")
                {
                    var index = Random.Range(0, 2);
                    coinEffect[index].gameObject.SetActive(true);
                }
                labelRewardQuantiy.gameObject.SetActive(itemFinal.item.ItemID == "Coin" || itemFinal.item.ItemID == "Crystal");
                rewardIcon.gameObject.SetActive(itemFinal.item.ItemID == "Coin" || itemFinal.item.ItemID == "Crystal");
                labelRewardType.gameObject.SetActive(itemFinal.item.ItemID == "TicketSpin" || itemFinal.item.categoryItem == CategoryItem.CREATURE);
                if(BigInteger.Parse(itemFinal.quantity) <= 0){
                    labelRewardType.gameObject.SetActive(false);
                    rewardIcon.gameObject.SetActive(false);
                    labelRewardQuantiy.gameObject.SetActive(false);
                }
                if (itemFinal.item.ItemID == "Coin" || itemFinal.item.ItemID == "Crystal")
                {

                    labelRewardQuantiy.text = itemFinal.quantity.ToKMBTA();
                    itemFinal.item.getSpriteForState((o) =>
                    {
                        rewardIcon.sprite2D = o;
                    });
                }
                else
                {
                    labelRewardType.text = itemFinal.item.categoryItem == CategoryItem.CREATURE ? "Pok" : "Spin";
                }
                var exist = GameManager.Instance.Database.getItem(itemFinal.item.ItemID);
                if (exist != null)
                {
                    exist.addQuantity(itemFinal.quantity.clearDot());
                }
                else if (BigInteger.Parse(itemFinal.quantity) > 0)
                {
                    var creature = ((CreatureItem)itemFinal.item);
                    var zone = GameDatabase.Instance.ZoneCollection.Find(x => x.ItemID == GameManager.Instance.ZoneChoosed);
                    var pack = zone.getPackage();
                    var mapParent = GameManager.Instance.Database.worldData.zones.Find(x => x.id == GameManager.Instance.ZoneChoosed).maps.Find(x => creature.parentMap && x.id == creature.parentMap.ItemID);
                    var newCreature = new PackageCreatureInstanceSaved() { creature = creature.ItemID, id = pack.package.ItemID, instanceID = GameManager.Instance.GenerateID.ToString(), mapParent = mapParent };
                    GameManager.Instance.GenerateID++;
                    GameManager.Instance.Database.worldData.addCreature(newCreature, GameManager.Instance.ZoneChoosed);
                    EzEventManager.TriggerEvent(new AddCreatureEvent()
                    {
                        change = 1,
                        creature = newCreature,
                        zoneid = GameManager.Instance.ZoneChoosed,
                        manualByHand = false,
                    });
                    addCreatureObject(new CreatureInstanceSaved() { id = creature.ItemID, instanceID = "1", mapParent = mapParent });
                }
                blockTouch.SetActive(false);
            }

            GameManager.Instance.SaveGame();
        }

        public IEnumerator delayAction(float delay, System.Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }
        public IEnumerator result()
        {
            yield return new WaitForSeconds(1);
            List<WheelConfig> wheels = new List<WheelConfig>(GameDatabase.Instance.wheelMainConfig.ToArray());
            wheels.Shuffle();
            float random = Random.Range(0.0f, 1.0f);
            float currentPercent = 1;
            WheelConfig result = null;
            for (int i = 0; i < wheels.Count; ++i)
            {
                currentPercent -= wheels[i].change;
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
            float randomInsinde = Random.Range(0.0f, 1.0f);
            currentPercent = 1;
            int resultCount = 0;
            for (int i = 0; i < result.percentRoll.Length; ++i)
            {
                currentPercent -= result.percentRoll[i];
                if (randomInsinde > currentPercent)
                {
                    resultCount = i + 1;
                    break;
                }
                if (i == result.percentRoll.Length - 1)
                {
                    resultCount = i + 1;
                }
            }
            resultItem = new List<int>();
            for (int i = 0; i < resultCount; ++i)
            {
                resultItem.Add(GameDatabase.Instance.wheelMainConfig.IndexOf(result));
            }
            for (int i = 0; i < 3 - resultCount; ++i)
            {
                int randomAnother = -1;
                do
                {
                    randomAnother = Random.Range(0, wheels.Count);
                } while (resultItem.Exists(x => x == randomAnother));
                resultItem.Add(randomAnother);
            }
            resultItem.Shuffle();
            for (int i = 0; i < rolls.Length; ++i)
            {
                rolls[i].isForever = false;
                rolls[i].Elements[3].GetComponent<ItemWheel>().fixSkin(GameDatabase.Instance.wheelMainConfig[resultItem[i]]);
            }
        }

        public void startRoll()
        {
            labelRewardQuantiy.gameObject.SetActive(false);
            rewardIcon.gameObject.SetActive(false);
            labelRewardType.gameObject.SetActive(true);
            labelRewardType.text = "SPINNING";
            blockTouch.SetActive(true);
            rollCount = 3;
            foreach (var roll in rolls)
            {
                roll.isForever = true;
                StartCoroutine(delayAction(Random.Range(0.0f, 1.0f), delegate
                {
                    roll.startRoll();
                }));

            }
            StartCoroutine(result());
        }
        public void startRollTicket()
        {
            var ticket = GameManager.Instance.Database.getItem("TicketSpin");
            if (ticket.QuantityBig > 0)
            {
                ticket.addQuantity("-1");
                startRoll();
                updateLayer();
            }
        }
        public void startRollDiamond()
        {
            var ticket = GameManager.Instance.Database.getItem("Crystal");
            if (ticket.QuantityBig > 0)
            {
                ticket.addQuantity("-1");
                startRoll();
            }
            else
            {
                HUDManager.Instance.showBoxNotEnough(ticket.item);
            }
        }


        public void watch()
        {
            GameManager.Instance.WatchRewardADS(GameConfig.Advertise.CLAIM_TICKET);
            var exist = GameManager.Instance.Database.getItem("TicketSpin");
            exist.addQuantity("3");
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
}

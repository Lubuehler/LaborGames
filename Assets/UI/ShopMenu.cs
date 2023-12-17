using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;
using Unity.VisualScripting;
using ExitGames.Client.Photon.StructWrapping;
using System.Linq;

public class ShopMenu : MonoBehaviour
{
    public GameObject statRowPrefab;
    public Transform statsPanelTransform;

    public TMP_Text wave;
    public TMP_Text coins;

    public Button goButton;
    [SerializeField] private Button ressurectButton;

    private Player player;

    private Dictionary<string, Func<Player, string>> statMappings;
    private Dictionary<string, StatRow> statRows;

    private List<Item> itemList = new List<Item>();
    [SerializeField] private LayoutGroup itemGroup;
    [SerializeField] private GameObject shopItemPrefab;


    [SerializeField] private LayoutGroup specialAttackCellGroup;
    [SerializeField] private GameObject specialAttackCellPrefab;

    [SerializeField] private LayoutGroup itemCellGroup;
    [SerializeField] private GameObject itemCellPrefab;
    private bool initialized = false;

    private void OnEnable()
    {
        if (!initialized)
        {
            player = NetworkController.Instance.GetLocalPlayerObject().GetComponent<Player>();
            if (player == null)
            {
                Debug.Log("wrong order; player not initialised in ShopMenu Awake");
                return;
            }
            statMappings = new Dictionary<string, Func<Player, string>>
            {
                { "Max Health", p => p.maxHealth.ToString() },
                { "Attack Damage", p => p.attackDamage.ToString("F2") },
                { "Attack Speed", p => p.attackSpeed.ToString("F2") },
                { "Crit Chance", p=> p.critChance.ToString("F2") },
                { "Crit Damage Multiplier", p=> p.critDamageMultiplier.ToString("F2") },
                { "Dodge Chance", p=> p.dodgeChance.ToString("F2") },
                { "Movement Speed", p => p.movementSpeed.ToString("F2") },
                { "Armor", p => p.armor.ToString("F2")  },
                { "Range", p => p.range.ToString()  }
            };

            statRows = new Dictionary<string, StatRow>();

            foreach (var mapping in statMappings)
            {
                InstantiateStatRow(mapping.Key);
            }
            initialized = true;
        }

        ShopSystem.Instance.OnSpecialAttacksChanged += UpdateSpecialAttacks;

        goButton.interactable = true;
        if (player != null)
        {
            player.OnStatsChanged += UpdateStats;
            UpdateStats(); // Update immediately to show current stats
        }
        this.wave.text = "Shop (Wave " + LevelController.Instance.currentWave.ToString() + ")";
        this.coins.text = player.coins.ToString();

        RandomizeShop();
    }

    private void Update()
    {
        Player localPlayer = NetworkController.Instance.GetLocalPlayerObject().GetComponent<Player>();
        if (localPlayer.isAlive)
        {
            switch (LevelController.Instance.GetDeadPlayers().Count)
            {
                case 0:
                    ressurectButton.gameObject.SetActive(false); break;
                case 1:
                    ressurectButton.gameObject.SetActive(true);
                    ressurectButton.GetComponentInChildren<TMP_Text>().text = "Ressurect Ally"; break;
                default:
                    ressurectButton.gameObject.SetActive(true);
                    ressurectButton.GetComponentInChildren<TMP_Text>().text = "Ressurect Allies"; break;
            }
        }
        else
        {
            ressurectButton.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.OnStatsChanged -= UpdateStats;
        }
        ShopSystem.Instance.OnSpecialAttacksChanged -= UpdateSpecialAttacks;
    }

    private void InstantiateStatRow(string propertyName)
    {
        GameObject statRowObject = Instantiate(statRowPrefab, statsPanelTransform);
        StatRow statRow = statRowObject.GetComponent<StatRow>();
        statRows[propertyName] = statRow;
    }

    private void UpdateStats()
    {
        foreach (var mapping in statMappings)
        {
            string propertyName = mapping.Key;
            Func<Player, string> getStatValue = mapping.Value;

            if (statRows.ContainsKey(propertyName))
            {
                statRows[propertyName].SetStat(propertyName, getStatValue(player));
            }
        }
        coins.text = player.coins.ToString();

    }

    public void OnGoClick()
    {
        goButton.interactable = false;
        LevelController.Instance.RPC_Ready(NetworkController.Instance.GetLocalPlayerObject(), true);
    }

    public void OnRessurectClicked()
    {
        print("RESPAWN CLICKED");
        if (ShopSystem.Instance.CanAfford(20))
        {
            LevelController.Instance.RessurectPlayers();
        }
        else
        {
            print("NO MONEY");
        }
    }

    public void UpdateDisplayedItems()
    {
        for (int i = itemGroup.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(itemGroup.transform.GetChild(i).gameObject);
        }
        foreach (Item item in itemList)
        {
            GameObject itemVis = Instantiate(shopItemPrefab);
            itemVis.transform.SetParent(itemGroup.transform, false);
            itemVis.GetComponentInChildren<ShopItem>().SetItem(item);
        }
    }

    public void RandomizeShop()
    {
        itemList.Clear();

        List<Item> itemPool = new();
        itemPool.AddRange(ShopSystem.Instance.availableItems);

        for (int i = 0; i < 3; i++)
        {
            Item item = itemPool[UnityEngine.Random.Range(0, itemPool.Count)];
            itemList.Add(item);
            itemPool.Remove(item);
        }
        UpdateDisplayedItems();
    }

    public void UpdateSpecialAttacks(int itemID)
    {
        Item item = ShopSystem.Instance.allItems.FirstOrDefault(item => item.itemID == itemID);
        if (item != null)
        {
            if (item.itemType == ItemType.SpecialAttack)
            {
                GameObject itemVis = Instantiate(specialAttackCellPrefab);
                itemVis.transform.SetParent(specialAttackCellGroup.transform, false);
                itemVis.GetComponent<SpecialAttackCell>().Initialize(item);
            }
            else if(item.itemType == ItemType.Item)
            {
                GameObject itemVis = Instantiate(itemCellPrefab);
                itemVis.transform.SetParent(itemCellGroup.transform, false);
                itemVis.GetComponent<ItemCell>().Initialize(item);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopMenu : MonoBehaviour
{
    public GameObject statRowPrefab;
    public Transform statsPanelTransform;

    public TMP_Text wave;
    public TMP_Text coins;

    public Button goButton;
    [SerializeField] private Button ressurectButton;
    [SerializeField] private Button healButton;

    [SerializeField] private TMP_Text goButtonText;
    [SerializeField] private TMP_Text deadText;
    [SerializeField] private TMP_Text ressurectButtonPriceText;
    [SerializeField] private TMP_Text refreshButtonPriceText;
    [SerializeField] private TMP_Text healButtonPriceText;

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

    [SerializeField]
    private int initialRefreshShopPrice;
    [SerializeField]
    private int refreshShopPriceIncreasePerWave;
    [SerializeField]
    private int initialRespawnPrice;
    [SerializeField]
    private int respawnPriceIncreasePerWave;
    [SerializeField]
    private int initialHealPrice;
    [SerializeField]
    private int HealPriceIncreasePerWave;


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
                { "Critical Strike Chance", p=> p.critChance.ToString("F2") },
                { "Critical Strike Damage Factor", p=> p.critDamageMultiplier.ToString("F2") },
                { "Life Steal", p=> p.lifesteal.ToString("F2") },
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

        if (player != null)
        {
            player.OnStatsChanged += UpdateStats;
            UpdateStats();
        }
        wave.text = "Shop (Wave " + LevelController.Instance.currentWave.ToString() + ")";
        coins.text = player.coins.ToString();

        InitializeButtons();
        RandomizeShop();
    }

    private void InitializeButtons()
    {
        goButtonText.text = "Go!";
        goButton.interactable = true;
        ressurectButton.interactable = true;

        refreshButtonPriceText.text = (initialRefreshShopPrice + refreshShopPriceIncreasePerWave * LevelController.Instance.currentWave).ToString();
        ressurectButtonPriceText.text = (initialRespawnPrice + respawnPriceIncreasePerWave * LevelController.Instance.currentWave).ToString();
        healButtonPriceText.text = (initialHealPrice + HealPriceIncreasePerWave * LevelController.Instance.currentWave).ToString();

        RandomizeShop();
        ClearCells();


        if (LevelController.Instance.localPlayer.isAlive)
        {
            if (LevelController.Instance.localPlayer.currentHealth == LevelController.Instance.localPlayer.maxHealth)
            {
                healButton.interactable = false;
            }
            else
            {
                healButton.interactable = true;
            }
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
            healButton.interactable = false;
        }
    }

    private void Update()
    {
        coins.text = player.coins.ToString();

        if (LevelController.Instance.localPlayer.isAlive)
        {
            if (deadText.enabled)
            {
                deadText.enabled = false;
            }
        }
        else
        {
            deadText.enabled = true;
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
        RedrawItems();
    }

    public void OnGoClick()
    {
        goButton.interactable = false;
        LevelController.Instance.RPC_ShopReady(NetworkController.Instance.GetLocalPlayerObject(), true);
        goButtonText.text = "Waiting for allies...";
    }

    public void OnRessurectClicked()
    {
        if (ShopSystem.Instance.BuyService(initialRespawnPrice + respawnPriceIncreasePerWave * LevelController.Instance.currentWave))
        {
            LevelController.Instance.RessurectPlayers();
            ressurectButton.interactable = false;
        }
    }

    public void OnRefreshClicked()
    {
        if (ShopSystem.Instance.BuyService(initialRefreshShopPrice + refreshShopPriceIncreasePerWave * LevelController.Instance.currentWave))
        {
            RandomizeShop();
        }

    }

    public void OnHealClicked()
    {
        if (ShopSystem.Instance.BuyService(initialHealPrice + HealPriceIncreasePerWave * LevelController.Instance.currentWave))
        {
            LevelController.Instance.localPlayer.Heal(int.MaxValue);
            healButton.interactable = false;
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

    public void RedrawItems()
    {
        for (int i = itemGroup.transform.childCount - 1; i >= 0; i--)
        {
            if (itemGroup.transform.GetChild(i).gameObject.GetComponentInChildren<ShopItem>() != null)
            {
                itemGroup.transform.GetChild(i).gameObject.GetComponentInChildren<ShopItem>().Redraw();
            }
        }
        if (!ShopSystem.Instance.CanAfford(initialRespawnPrice + respawnPriceIncreasePerWave * LevelController.Instance.currentWave))
        {
            ressurectButtonPriceText.color = Color.red;
        }
        else
        {
            ressurectButtonPriceText.color = Color.white;
        }
        if (!ShopSystem.Instance.CanAfford(initialRefreshShopPrice + refreshShopPriceIncreasePerWave * LevelController.Instance.currentWave))
        {
            refreshButtonPriceText.color = Color.red;
        }
        else
        {
            refreshButtonPriceText.color = Color.white;
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
                GameObject itemVis = Instantiate(specialAttackCellPrefab, parent: specialAttackCellGroup.transform);
                itemVis.GetComponent<SpecialAttackCell>().Initialize(item);
            }
            else if (item.itemType == ItemType.Item)
            {
                GameObject itemVis = Instantiate(itemCellPrefab, parent: itemCellGroup.transform);
                itemVis.GetComponent<ItemCell>().Initialize(item);
            }
        }
    }

    private void ClearCells()
    {
        if (player.GetComponent<Weapon>().specialAttacks.Count == 0 && player.items.Count == 0)
        {
            while (specialAttackCellGroup.transform.childCount > 0)
            {
                DestroyImmediate(specialAttackCellGroup.transform.GetChild(0).gameObject);
            }
            while (itemCellGroup.transform.childCount > 0)
            {
                DestroyImmediate(itemCellGroup.transform.GetChild(0).gameObject);
            }
        }
    }
}
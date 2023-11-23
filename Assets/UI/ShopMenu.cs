using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;

public class ShopMenu : MonoBehaviour
{
    public GameObject statRowPrefab;
    public Transform statsPanelTransform;

    public TMP_Text wave;
    public TMP_Text coins;

    public Button goButton;
    [SerializeField] private Button ressurectButton;

    [SerializeField] private CanvasGroup Toast;
    private Animator animator;


    private Player player;

    private Dictionary<string, Func<Player, string>> statMappings;
    private Dictionary<string, StatRow> statRows;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        player = NetworkController.Instance.GetLocalPlayerObject().GetComponent<Player>();
        if (player == null)
        {
            print("wrong order; player not initialised in ShopMenu Awake");
            return;
        }
        statMappings = new Dictionary<string, Func<Player, string>>
        {
            { "Max Health", p => p.maxHealth.ToString() },
            { "Attack Damage", p => p.attackDamage.ToString("F2") },
            { "Attack Speed", p => p.attackSpeed.ToString("F2") },
            { "Crit Chance", p=> p.critChance.ToString("F2") },
            { "Crit Damage Multiplier", p=> p.critDamageMultiplier.ToString("F2") },
            { "Dodge Chance", p=> p.dodgeProbability.ToString("F2") },
            { "Movement Speed", p => p.movementSpeed.ToString("F2") },
            { "Armor", p => p.armor.ToString("F2")  },
            { "Range", p => p.range.ToString()  }

        };

        statRows = new Dictionary<string, StatRow>();

        foreach (var mapping in statMappings)
        {
            InstantiateStatRow(mapping.Key);
        }
        this.wave.text = "Shop (Wave " + LevelController.Instance.currentWave.ToString() + ")";
        this.coins.text = player.coins.ToString();


    }

    private void OnEnable()
    {
        goButton.interactable = true;
        if (player != null)
        {
            player.OnStatsChanged += UpdateStats;
            UpdateStats(); // Update immediately to show current stats
        }
        this.wave.text = "Shop (Wave " + LevelController.Instance.currentWave.ToString() + ")";
        this.coins.text = player.coins.ToString();

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
    }

    public void OnGoClick()
    {
        goButton.interactable = false;
        LevelController.Instance.RPC_Ready(NetworkController.Instance.GetLocalPlayerObject(), true);
    }

    public void onRessurectClicked()
    {
        print("RESPAWN CLICKED");
        if (NetworkController.Instance.GetLocalPlayerObject().GetComponent<Player>().SpendCoins(20)) {
            LevelController.Instance.RessurectPlayers();
        } else
        {
            print("NO MONEY");
            Toast.alpha = 1;
            animator.SetTrigger("FadeOut");
        }
    }

}
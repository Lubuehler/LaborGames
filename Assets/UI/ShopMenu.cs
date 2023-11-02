using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopMenu : MonoBehaviour

{
    public GameObject statRowPrefab;
    public Transform statsPanelTransform;
    private Player player;

    private Dictionary<string, Func<Player, string>> statMappings;
    private Dictionary<string, StatRow> statRows;

    private void Awake()
    {
        player = GameController.Instance.localPlayer;
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
            { "Movement Speed", p => p.movementSpeed.ToString("F2") }

        };

        statRows = new Dictionary<string, StatRow>();

        foreach (var mapping in statMappings)
        {
            InstantiateStatRow(mapping.Key);
        }
    }

    private void OnEnable()
    {
        if (player != null)
        {
            player.OnStatsChanged += UpdateStats;
            UpdateStats(); // Update immediately to show current stats
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
}
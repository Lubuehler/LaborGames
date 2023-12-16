using Fusion.StatsInternal;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AllyHealthbar : MonoBehaviour
{
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private Image fillImage;
    [SerializeField] private RectTransform fillRect;

    public Player player { get; set; }

    private bool subscribed = false;
    public bool meanwhileRespawned = false;
    
    [SerializeField] private float allyBarInitialWidth = 200;
    [SerializeField] private float localPlayerInitialWidth = 400;
    private float standardWidth = 0;

    private void Awake()
    {
        fillRect = fillImage.GetComponent<RectTransform>();
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {

        // Adjust the fill's width based on maxHealth
        fillRect.SetSizeDelta(standardWidth * player.maxHealth / 100, fillRect.sizeDelta.y);

        // Adjust the fill amount based on the current health
        fillImage.fillAmount = player.currentHealth / player.maxHealth;
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.OnHealthChanged -= UpdateHealthBar;
            subscribed = false;
        }
    }

    private void Update()
    {
        if (!subscribed && player != null)
        {
            
            if (player == LevelController.Instance.localPlayer)
            {
                standardWidth = localPlayerInitialWidth;
                return;
            }
            else
            {
                standardWidth = allyBarInitialWidth;
                playerName.text = player.playerName;
                print("player name: "+ playerName.text);
            }

            player.OnHealthChanged += UpdateHealthBar;
            subscribed = true;
            UpdateHealthBar(player.currentHealth, player.maxHealth);
        } else if (player != null && playerName.text == "") {
            playerName.text = player.playerName;
        }
    }
}

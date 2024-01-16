using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AllyHealthbar : MonoBehaviour
{
    private TMP_Text playerName;
    private Slider slider;
    public Player player { get; set; }

    private void Awake()
    {
        slider = GetComponentInChildren<Slider>();
        playerName = GetComponentInChildren<TMP_Text>();
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        slider.value = currentHealth / maxHealth;
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.OnHealthChanged -= UpdateHealthBar;
        }
    }

    private void OnEnable()
    {
        if (player != null)
        {
            player.OnHealthChanged += UpdateHealthBar;
        }
    }

    public void SetPlayer(Player player)
    {
        playerName.text = player.playerName;

        player.OnHealthChanged += UpdateHealthBar;
        UpdateHealthBar(player.currentHealth, player.maxHealth);
    }
}

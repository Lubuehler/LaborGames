using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Slider slider;

    private bool subscribed = false;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }


    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        slider.value = currentHealth / maxHealth;
    }

    private void OnDisable()
    {
        if (LevelController.Instance.localPlayer != null)
        {
            LevelController.Instance.localPlayer.OnHealthChanged -= UpdateHealthBar;
            subscribed = false;
        }
    }

    private void Update()
    {
        if (!subscribed && LevelController.Instance != null && LevelController.Instance.localPlayer != null)
        {
            LevelController.Instance.localPlayer.OnHealthChanged += UpdateHealthBar;
            subscribed = true;
        }
    }

    public void OnEnable()
    {
        if (LevelController.Instance != null && LevelController.Instance.localPlayer != null)
        {
            UpdateHealthBar(LevelController.Instance.localPlayer.currentHealth, LevelController.Instance.localPlayer.maxHealth);
        }
    }
}

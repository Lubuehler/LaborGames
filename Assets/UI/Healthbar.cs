using Fusion.StatsInternal;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image fillImage;
    private RectTransform fillRect;
    private float initialWidth = 400;

    private bool subscribed = false;

    private void Awake()
    {
        fillRect = fillImage.GetComponent<RectTransform>();
    }


    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        // Adjust the fill's width based on maxHealth
        fillRect.SetSizeDelta(initialWidth * maxHealth / 100, fillRect.sizeDelta.y);

        // Adjust the fill amount based on the current health
        fillImage.fillAmount = currentHealth / maxHealth;
        print("updated healthbar: " + currentHealth);
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
        if (!subscribed && LevelController.Instance.localPlayer != null)
        {
            LevelController.Instance.localPlayer.OnHealthChanged += UpdateHealthBar;
            subscribed = true;
        }
    }
}

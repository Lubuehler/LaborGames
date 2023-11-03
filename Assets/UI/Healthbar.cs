using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image fillImage;
    private float maxHealth;

    public void SetMaxHealth(float health)
    {
        maxHealth = health;
        SetHealth(health);
    }

    public void SetHealth(float health)
    {
        fillImage.fillAmount = health / maxHealth;
    }
}

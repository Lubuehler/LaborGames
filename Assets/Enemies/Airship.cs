using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class Airship : Enemy
{
    [SerializeField] private Slider slider;
    [SerializeField] private ProjectileEnemy projectilePrefab;
    [SerializeField] private GameObject smoke;
    [SerializeField] private Transform missileSpawnPosition;

    [SerializeField] private GameObject smokeGrenadePrefab;
    [SerializeField] private const float throwCooldown = 30f;

    private float lastThrowTime = -30f;

    public float attackSpeed = 0.5f;
    private int maxHealth;

    void Awake()
    {
        maxHealth = health;
    }

    void LateUpdate()
    {
        UpdateHealthBar(health, maxHealth);

        if (health <= maxHealth * 0.3)
        {
            smoke.SetActive(true);
        }
    }

    public void UpdateHealthBar(float currentValue, float maxValue)
    {
        slider.value = currentValue / maxValue;
    }


    protected override void DoSomething()
    {
        if (currentTarget == null) return;
        if (Time.time - lastThrowTime >= throwCooldown)
        {
            if (Vector3.Distance(getPosition(), currentTarget.GetComponent<Player>().getPosition()) < 10)
            {
                RPC_throwSmokeGrenade();
                lastThrowTime = Time.time;
            }
        }
    }

    [Rpc]
    public void RPC_throwSmokeGrenade()
    {
        GameObject grenade = Instantiate(smokeGrenadePrefab, getPosition(), Quaternion.identity);
        grenade.GetComponent<SmokeGrenade>().ThrowToTargetPosition(currentTarget.GetComponent<Player>().getPosition());
    }

}

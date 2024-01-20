using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class Airship : Enemy
{
    [SerializeField] private Slider slider;
    [SerializeField] private GameObject smoke;


    // Smoke
    [SerializeField] private GameObject smokeGrenadePrefab;
    [SerializeField] private const float throwCooldown = 30f;
    private float lastThrowTime = -30f;


    // Flame Thrower
    [SerializeField] private Transform flameSpawnPosition;
    [SerializeField] private GameObject flameThrowerPrefab;
    [SerializeField] private const float flameThrowerCooldown = 10f;
    private float lastFlameThrowerTime = -10f;


    public float attackSpeed = 0.5f;
    private int maxHealth;


    public override void Spawned()
    {
        base.Spawned();
        maxHealth = health;
    }

    public void UpdateHealthBar(float currentValue, float maxValue)
    {
        slider.value = currentValue / maxValue;
    }

    protected override void Move()
    {
        if (!movementDisabled)
        {
            if (currentTarget == null || !currentTarget.GetComponent<Player>().isAlive)
            {
                networkRigidbody2D.Rigidbody.velocity = Vector2.Lerp(networkRigidbody2D.Rigidbody.velocity, Vector2.zero, Runner.DeltaTime * movementSmoothing);
                return;
            }
            Vector2 toTarget = currentTarget.transform.position - transform.position;
            Vector2 separationForce = CalculateSeparationForce();

            var speed = EnemySpawner.Instance.currentSpeed / 2;

            Vector2 desiredVelocity = (toTarget.normalized + separationForce).normalized * speed;
            networkRigidbody2D.Rigidbody.velocity = Vector2.Lerp(networkRigidbody2D.Rigidbody.velocity, desiredVelocity, Runner.DeltaTime * movementSmoothing);
        }
    }


    protected override void DoSomething()
    {
        UpdateHealthBar(health, maxHealth);

        if (health <= maxHealth * 0.3)
        {
            smoke.SetActive(true);
        }

        if (currentTarget == null) return;
        if (Time.time - lastThrowTime >= throwCooldown)
        {
            if (Vector3.Distance(getPosition(), currentTarget.GetComponent<Player>().getPosition()) < 10)
            {
                RPC_throwSmokeGrenade();
                lastThrowTime = Time.time;
            }
        }

        if (Time.time - lastFlameThrowerTime >= flameThrowerCooldown)
        {
            if (Vector3.Distance(getPosition(), currentTarget.GetComponent<Player>().getPosition()) < 10)
            {
                RPC_throwFlame();
                lastFlameThrowerTime = Time.time;
            }
        }
    }

    [Rpc]
    public void RPC_throwSmokeGrenade()
    {
        GameObject grenade = Instantiate(smokeGrenadePrefab, getPosition(), Quaternion.identity);
        grenade.GetComponent<SmokeGrenade>().ThrowToTargetPosition(currentTarget.GetComponent<Player>().getPosition());
    }

    [Rpc]
    public void RPC_throwFlame()
    {
        Vector3 direction = currentTarget.GetComponent<Player>().getPosition() - networkRigidbody2D.Rigidbody.position;
        var vec2 = new Vector2(direction.x, direction.y);
        GameObject flame = Instantiate(flameThrowerPrefab, flameSpawnPosition.position, Quaternion.LookRotation(vec2), parent: flameSpawnPosition);
        flame.GetComponent<FlameThrower>();
    }

}

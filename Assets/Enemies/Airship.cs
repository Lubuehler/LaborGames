using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Airship : Enemy
{
    [SerializeField] private Slider slider;
    [SerializeField] private ProjectileEnemy projectilePrefab;
    [SerializeField] private GameObject smoke;
    [SerializeField] private Transform missileSpawnPosition;

    public float attackSpeed = 0.5f;
    private int maxHealth;
    private float currentTime;

    void Awake()
    {
        maxHealth = health;
    }

    void LateUpdate()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= 1f / attackSpeed && Vector3.Distance(transform.position, currentTarget.transform.position) <= 10)
        {
            Fire();
            currentTime = 0f;
        }
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

    private void Fire()
    {
        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, networkRigidbody2D.Rigidbody.velocity);
        var projectile = Runner.Spawn(projectilePrefab, missileSpawnPosition.position, rotation, Object.InputAuthority);
        projectile?.Fire(networkRigidbody2D.Rigidbody.velocity.normalized);
    }
}

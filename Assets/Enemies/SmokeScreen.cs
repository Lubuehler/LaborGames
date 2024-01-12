using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeScreen : MonoBehaviour
{
    public float playerClearRadius = 5.0f;
    public float smokeGrenadeRadius = 7f;

    private ParticleSystem fogParticleSystem;
    private GameObject player;

    [SerializeField] private GameObject smokeGrenadePrefab;
    [SerializeField] private const float keepAliveDuration = 30f;

    private float explosionTime;

    private void Awake()
    {
        fogParticleSystem = GetComponent<ParticleSystem>();
        player = LevelController.Instance.localPlayer.gameObject;
    }

    void Update()
    {
        if (explosionTime - Time.time > keepAliveDuration){
            Destroy(this);
        }
        if (player == null || 
            Vector3.Distance(player.transform.position, transform.position) > smokeGrenadeRadius + playerClearRadius)
        {
            return;
        }

        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[fogParticleSystem.particleCount];
        int particleCount = fogParticleSystem.GetParticles(particles);

        for (int i = 0; i < particleCount; i++)
        {
            float distance = Vector3.Distance(player.transform.position, particles[i].position);
            if (distance < playerClearRadius && Random.value > 0.95)
            {
                particles[i].remainingLifetime = 0;
                
            }
        }

        fogParticleSystem.SetParticles(particles, particleCount);
    }

    public void Emit() { 
        fogParticleSystem.Play();
        explosionTime = Time.time;
    }
}

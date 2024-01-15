using UnityEngine;

public class SmokeScreen : MonoBehaviour
{
    public float playerClearRadius = 5.0f;
    public float smokeGrenadeRadius = 7f;

    private ParticleSystem fogParticleSystem;

    [SerializeField] private GameObject smokeGrenadePrefab;
    [SerializeField] private const float keepAliveDuration = 30f;

    private float explosionTime;

    private void Awake()
    {
        fogParticleSystem = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if (explosionTime - Time.time > keepAliveDuration)
        {
            Destroy(this);
        }
        Vector2 playerPos = Vector2.zero;
        try
        {
            playerPos = Camera.main.GetComponent<CameraScript>().target.GetComponent<Player>().getPosition();
        }
        catch
        {
            return;
        }
        if (Vector3.Distance(playerPos, transform.position) > smokeGrenadeRadius + playerClearRadius)
        {
            return;
        }

        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[fogParticleSystem.particleCount];
        int particleCount = fogParticleSystem.GetParticles(particles);

        for (int i = 0; i < particleCount; i++)
        {
            float distance = Vector3.Distance(playerPos, particles[i].position);
            if (distance < playerClearRadius && Random.value > 0.95)
            {
                particles[i].remainingLifetime = 0;

            }
        }

        fogParticleSystem.SetParticles(particles, particleCount);
    }

    public void Emit()
    {
        fogParticleSystem.Play();
        explosionTime = Time.time;
    }
}

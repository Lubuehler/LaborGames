using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private ParticleSystem laserSystem;
    [SerializeField] private ParticleSystem explosionSystem;
    [SerializeField] private ParticleSystem lightningSystem;


    private bool deactivated = false;

    public void Init(LaserDrone drone)
    {
        GetComponentInChildren<LaserHit>().Init(drone, this);
    }

    void Update()
    {
        if (deactivated && !laserSystem.IsAlive())
        {
            Destroy(gameObject);
        }
    }

    public void playHitParticle(Vector2 position)
    {
        explosionSystem.transform.position = position;
        explosionSystem.Emit(1);
        lightningSystem.Emit(1);
    }

    public void activate(Vector2 targetPosition)
    {
        Vector2 direction = targetPosition - (Vector2)transform.position; // Get direction to target
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // Calculate target rotation angle

        laserSystem.Play();
        transform.rotation = Quaternion.Euler(0, 0, targetAngle); // Apply new rotation
    }

    public void rotate(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    public void deactivate()
    {
        laserSystem.Stop(false, stopBehavior: ParticleSystemStopBehavior.StopEmitting);
        deactivated = true;
    }
}

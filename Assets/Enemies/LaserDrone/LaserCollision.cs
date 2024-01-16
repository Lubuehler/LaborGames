using System.Collections.Generic;
using UnityEngine;

public class LaserHit : MonoBehaviour
{
    [SerializeField] private LaserDrone drone;
    public List<ParticleCollisionEvent> collisionEvents;

    private ParticleSystem laserSystem;
    private Laser laser;


    private void Start()
    {
        collisionEvents = new List<ParticleCollisionEvent>();
        laserSystem = GetComponent<ParticleSystem>();
    }


    public void Init(LaserDrone drone, Laser laser)
    {
        this.drone = drone;
        this.laser = laser;
    }

    private void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = laserSystem.GetCollisionEvents(other, collisionEvents);

        Player player = other.GetComponent<Player>();
        int i = 0;

        while (i < numCollisionEvents)
        {
            if (player)
            {
                drone.OnHit(player);
                Vector2 direction = player.getPosition() - (Vector2)transform.position;
                laser.playHitParticle(player.getPosition() + (direction * -0.1f));
            }
            i++;
        }
    }

}

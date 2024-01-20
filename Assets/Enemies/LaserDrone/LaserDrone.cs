using UnityEngine;

public class LaserDrone : Enemy
{
    [SerializeField] private float damagePerHit = 1.0f;
    [SerializeField] private float rotationSpeed = 10f;

    [SerializeField] private GameObject laserPrefab;
    private Laser laser;

    protected override void DoSomething()
    {
        Player player = currentTarget?.gameObject.GetComponent<Player>();
        if (player == null)
        {
            return;
        }
        if (Vector2.Distance(getPosition(), player.getPosition()) < 10)
        {
            if (laser == null)
            {
                stopRequested = true;
                StartLaser(player.getPosition());
            }

            AimLaser(player.getPosition());


        }
        else
        {
            if (laser != null)
            {
                stopRequested = false;
                StopLaser();
            }
        }
    }

    public void OnHit(Player player)
    {
        if (Runner.IsServer)
        {
            player.TakeDamage(damagePerHit);
        }
    }

    public void StartLaser(Vector2 position)
    {
        laser = Instantiate(laserPrefab, parent: getTransform()).GetComponent<Laser>();
        laser.Init(this);
        laser.activate(position);
    }

    public void AimLaser(Vector2 targetPosition)
    {
        Vector2 direction = targetPosition - (Vector2)transform.position;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float currentAngle = laser.transform.eulerAngles.z;

        float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);


        float newAngle = currentAngle + Mathf.Clamp(angleDifference, -rotationSpeed * Runner.DeltaTime, rotationSpeed * Runner.DeltaTime);

        laser.rotate(Quaternion.Euler(0, 0, newAngle));
    }

    public void StopLaser()
    {
        laser.deactivate();
        laser = null;
    }
}
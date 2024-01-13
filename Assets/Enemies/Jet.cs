using Fusion;
using UnityEngine;
public class Jet : Enemy
{
    [SerializeField] private ProjectileJet projectilePrefab;

    public bool maneuverStarted = false;

    // Projectile Stats
    private float projectileSpeed = 20f;
    private int projectileDamage = 50;

    // Speed & Fire
    private Vector2 lockedPosition;
    private Vector2 desiredVelocity;
    private bool fired = false;

    // Looping
    private bool loopingStarted = false;
    private bool centerSet = false;
    private Vector2 rotationCenter;

    protected override void Move()
    {
        var speed = EnemySpawner.Instance.speed;

        if (!movementDisabled)
        {
            if (currentTarget == null || !currentTarget.GetComponent<Player>().isAlive)
            {
                networkRigidbody2D.Rigidbody.velocity = Vector2.Lerp(networkRigidbody2D.Rigidbody.velocity, Vector2.zero, Runner.DeltaTime * movementSmoothing);
                return;
            }

            Vector2 currentPosition = new Vector2(transform.position.x, transform.position.y);
            Vector2 playerPosition = new Vector2(currentTarget.transform.position.x, currentTarget.transform.position.y);

            float distanceToTarget = Vector2.Distance(currentPosition, playerPosition);

            if (distanceToTarget < 10 && !loopingStarted)
            {
                if (!maneuverStarted)
                {
                    lockedPosition = playerPosition;
                    Vector2 toTarget = lockedPosition - currentPosition;


                    

                    desiredVelocity = toTarget.normalized * speed * 2;

                    maneuverStarted = true;
                }
                if (!fired)
                {
                    Fire();
                    fired = true;
                }

                networkRigidbody2D.Rigidbody.velocity = Vector2.Lerp(networkRigidbody2D.Rigidbody.velocity, desiredVelocity, Runner.DeltaTime * movementSmoothing);
            }
            else
            {
                if (maneuverStarted)
                {
                    loopingStarted = true;

                    if (!centerSet)
                    {
                        Vector2 perpendicularVector1 = new Vector2(-networkRigidbody2D.Rigidbody.velocity.y, networkRigidbody2D.Rigidbody.velocity.x).normalized;
                        Vector2 perpendicularVector2 = new Vector2(networkRigidbody2D.Rigidbody.velocity.y, -networkRigidbody2D.Rigidbody.velocity.x).normalized;
                        Vector2 selectedPerpendicularVector = (perpendicularVector1.y > perpendicularVector2.y) ? perpendicularVector1 : perpendicularVector2;
                        Vector2 perpendicularVector = selectedPerpendicularVector * 2f;

                        rotationCenter = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y) + perpendicularVector;
                        centerSet = true;
                    }

                    var magnitude = networkRigidbody2D.Rigidbody.velocity.magnitude;

                    // get vector center <- obj
                    Vector2 gravityVector = this.rotationCenter - networkRigidbody2D.Rigidbody.position;

                    // check whether left or right of target
                    bool left = Vector2.SignedAngle(networkRigidbody2D.Rigidbody.velocity, gravityVector) > 0;

                    // get new vector which is 90° on gravityDirection and world Z (since 2D game) normalize so it has magnitude = 1
                    Vector3 newDirection = Vector3.Cross(gravityVector, Vector3.forward).normalized;

                    // invert the newDirection in case user is touching right of movement direction
                    if (!left) newDirection *= -1;

                    // set new direction but keep speed(previously stored magnitude)
                    networkRigidbody2D.Rigidbody.velocity = newDirection * magnitude;

                    // Find end of looping motion
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, networkRigidbody2D.Rigidbody.velocity, Mathf.Infinity, playerLayerMask);
                    if (hit.collider != null && hit.transform.gameObject.GetComponent<NetworkObject>().NetworkGuid == currentTarget.NetworkGuid)
                    {
                        loopingStarted = false;
                        maneuverStarted = false;
                        fired = false;
                        centerSet = false;
                    }
                }
                else
                {
                    Vector2 toTarget = currentTarget.transform.position - transform.position;
                    Vector2 separationForce = CalculateSeparationForce();



                    Vector2 desiredVelocity = (toTarget.normalized + separationForce).normalized * speed;
                    networkRigidbody2D.Rigidbody.velocity = Vector2.Lerp(networkRigidbody2D.Rigidbody.velocity, desiredVelocity, Runner.DeltaTime * movementSmoothing);
                }
            }
        }
    }

    private void Fire()
    {
        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, networkRigidbody2D.Rigidbody.velocity);
        var projectile = Runner.Spawn(projectilePrefab, networkRigidbody2D.transform.position, rotation, Object.InputAuthority);
        projectile?.Fire(networkRigidbody2D.Rigidbody.velocity.normalized, projectileSpeed, this);
    }

    public void OnProjectileHit(Player player)
    {
        player.TakeDamage(projectileDamage);
    }
}
using UnityEngine;

public class SmokeGrenade : MonoBehaviour
{
    [SerializeField] private ParticleSystem trail;
    [SerializeField] private GameObject smokeScreen;
    [SerializeField] private float speed = 30;

    private Vector2 targetPosition;
    private bool isTargetPositionSet = false;

    private void Start()
    {
        EnemySpawner.Instance.RegisterObject(gameObject);
    }

    void Update()
    {
        if (!isTargetPositionSet) return;

        Vector2 currentPosition = new Vector2(transform.position.x, transform.position.y);

        if (Vector2.Distance(targetPosition, currentPosition) > 0.1f) // Adjust the threshold as needed
        {
            MoveToPosition();
        }
        else
        {
            trail.Stop();
            smokeScreen.GetComponent<SmokeScreen>().Emit();
            isTargetPositionSet = false;
        }
    }

    void MoveToPosition()
    {
        Vector2 currentPosition = new Vector2(transform.position.x, transform.position.y);
        Vector2 toTarget = targetPosition - currentPosition;

        Vector2 desiredVelocity = toTarget.normalized * speed;

        transform.position = new Vector3(currentPosition.x, currentPosition.y, transform.position.z) + new Vector3(desiredVelocity.x, desiredVelocity.y, 0) * Time.deltaTime;
    }

    public void ThrowToTargetPosition(Vector2 position)
    {
        targetPosition = position;
        isTargetPositionSet = true;
    }
}

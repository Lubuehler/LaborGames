using Fusion;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public NetworkObject target;
    public BoxCollider2D boundariesCollider;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    private float minX, maxX, minY, maxY;

    private void Start()
    {
        float camHalfHeight = Camera.main.orthographicSize;
        float camHalfWidth = camHalfHeight * Camera.main.aspect;

        Bounds bounds = boundariesCollider.bounds;
        minX = bounds.min.x + camHalfWidth;
        maxX = bounds.max.x - camHalfWidth;
        minY = bounds.min.y + camHalfHeight;
        maxY = bounds.max.y - camHalfHeight;
    }

    private void Update()
    {

        if (target == null) return;
        Player player = target.GetComponent<Player>();
        Vector3 desiredPosition = new Vector3(player.getPosition().x + offset.x, player.getPosition().y + offset.y, transform.position.z);

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minX, maxX);
        smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minY, maxY);

        transform.position = smoothedPosition;
    }
}

using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class Teleport : NetworkBehaviour
{
    [SerializeField] private GameObject teleportParticlePrefab;

    private NetworkRigidbody2D networkRigidbody;

    private static int maxLength = 8; // 4 times per second -> 2 seconds
    private Queue<Vector3> positions = new Queue<Vector3>(maxLength + 1);
    private Queue<Quaternion> rotations = new Queue<Quaternion>(maxLength + 1);
    private float positionTimer = 0f;
    private float interval = 0.25f;

    public override void Spawned()
    {
        networkRigidbody = GetComponentInParent<NetworkRigidbody2D>();
    }

    public void Activate()
    {
        Runner.Spawn(teleportParticlePrefab, networkRigidbody.transform.position);
        networkRigidbody.TeleportToPosition(positions.Peek());
        networkRigidbody.TeleportToRotation(rotations.Peek());
        Runner.Spawn(teleportParticlePrefab, networkRigidbody.transform.position);
    }


    public override void Render()
    {
        positionTimer += Time.deltaTime;
        if (positionTimer >= interval)
        {
            StorePosition();
            positionTimer = 0f;
        }
    }

    void StorePosition()
    {
        positions.Enqueue(networkRigidbody.transform.position);
        rotations.Enqueue(networkRigidbody.transform.rotation);
        if (positions.Count > maxLength)
        {
            positions.Dequeue();
            rotations.Dequeue();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Fusion;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Coin : NetworkBehaviour
{
    private float speed = 30;
    private NetworkRigidbody2D networkRigidbody2D;
    private Transform magnetTarget;

    public void SetTarget(Transform magnetTarget)
    {
        this.magnetTarget = magnetTarget;
    }

    public override void Spawned()
    {
        networkRigidbody2D = GetComponent<NetworkRigidbody2D>();
    }

    public override void Render()
    {
        if (magnetTarget != null)
        {
            Vector3 direction = (magnetTarget.position - transform.position).normalized;
            networkRigidbody2D.Rigidbody.velocity = direction * speed;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class JetAnimation : MonoBehaviour
{
    private NetworkRigidbody2D _rb;
    private SpriteRenderer _renderer;

    void Start()
    {
        _rb = GetComponentInParent<NetworkRigidbody2D>();
        _renderer = GetComponent<SpriteRenderer>();

        Flip();
    }


    void LateUpdate()
    {
        if (_rb.Rigidbody.velocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(_rb.Rigidbody.velocity.y, _rb.Rigidbody.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    public void Flip()
    {
        _renderer.flipY = _rb.Rigidbody.velocity.x < 0;
    }

}
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    private NetworkRigidbody2D _rb;
    private SpriteRenderer _renderer;

    void Start()
    {
        _rb = GetComponentInParent<NetworkRigidbody2D>();
        _renderer = GetComponent<SpriteRenderer>();
    }


    void LateUpdate()
    {
        if (_rb.ReadVelocity().x < -.1f)
        {
            _renderer.flipX = true;
        }
        else if (_rb.ReadVelocity().x > .1f)
        {
            _renderer.flipX = false;
        }
    }
}

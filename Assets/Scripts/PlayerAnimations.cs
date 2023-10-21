using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    private Rigidbody2D _rb;
    private SpriteRenderer _renderer;

    void Start()
    {
        _rb = GetComponentInParent<Rigidbody2D>();
        _renderer = GetComponent<SpriteRenderer>();
    }


    void LateUpdate()
    {
        if (_rb.velocity.x < -.1f)
        {
            _renderer.flipX = true;
        }
        else if (_rb.velocity.x > .1f)
        {
            _renderer.flipX = false;
        }
    }
}

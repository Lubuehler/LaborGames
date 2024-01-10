using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class JetAnimation : MonoBehaviour
{
    private NetworkRigidbody2D _rb;
    private Transform _transform;

    void Start()
    {
        _rb = GetComponentInParent<NetworkRigidbody2D>();
        _transform = GetComponentInParent<Transform>();
    }


    void LateUpdate()
    {
        if (_rb.Rigidbody.velocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(_rb.Rigidbody.velocity.y, _rb.Rigidbody.velocity.x) * Mathf.Rad2Deg;
            _transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        if(!GetComponentInParent<Jet>().maneuverStarted)
        {
            Flip();
        }
    }

    private void Flip()
    {
        Vector3 currentScale = _transform.localScale;
        currentScale.y = Mathf.Abs(currentScale.y) * (_rb.Rigidbody.velocity.x < 0 ? -1 : 1);
        _transform.localScale = currentScale;
    }
}
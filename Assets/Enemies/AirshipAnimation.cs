using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class AirshipAnimation : MonoBehaviour
{
    private NetworkRigidbody2D _rb;
    private SpriteRenderer _renderer;

    [SerializeField] private GameObject smoke;

    void Start()
    {
        _rb = GetComponentInParent<NetworkRigidbody2D>();
        _renderer = GetComponent<SpriteRenderer>();
    }


    void LateUpdate()
    {
        if (_rb.ReadVelocity().x < -.1f)
        {
            _renderer.flipX = false;
            AdjustSmokeRotation(345f);
        }
        else if (_rb.ReadVelocity().x > .1f)
        {
            _renderer.flipX = true;
            AdjustSmokeRotation(195f);
        }
    }

    void AdjustSmokeRotation(float targetRotationX)
    {
        if (smoke.activeSelf)
        {
            Vector3 currentRotation = smoke.transform.rotation.eulerAngles;
            smoke.transform.rotation = Quaternion.Euler(targetRotationX, currentRotation.y, currentRotation.z);
        }
    }
}

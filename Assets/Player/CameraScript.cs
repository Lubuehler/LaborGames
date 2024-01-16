using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CameraScript : MonoBehaviour
{
    public NetworkObject target; // Reference to the airplane's transform
    public BoxCollider2D boundariesCollider; // Reference to the boundaries' Box Collider 2D
    public float smoothSpeed = 0.125f; // Speed at which the camera follows the airplane
    public Vector3 offset; // Offset position relative to the airplane

    private float minX, maxX, minY, maxY;

    private void Start()
    {
        // Calculate the camera's half-width and half-height
        float camHalfHeight = Camera.main.orthographicSize;
        float camHalfWidth = camHalfHeight * Camera.main.aspect;

        // Calculate the boundaries for the camera
        Bounds bounds = boundariesCollider.bounds;
        minX = bounds.min.x + camHalfWidth;
        maxX = bounds.max.x - camHalfWidth;
        minY = bounds.min.y + camHalfHeight;
        maxY = bounds.max.y - camHalfHeight;
    }

    private void FixedUpdate()
    {
        
        if (target == null) return; // If no target is assigned, don't proceed
        Transform targetTransform = target.GetComponent<NetworkTransform>().InterpolationTarget;
        
        // Calculate the desired position for the camera
        Vector3 desiredPosition = new Vector3(targetTransform.position.x + offset.x, targetTransform.position.y + offset.y, transform.position.z);

        // Smoothly interpolate between the camera's current position and the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Clamp the camera's position to the boundaries
        smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minX, maxX);
        smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minY, maxY);

        // Update the camera's position
        transform.position = smoothedPosition;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5.0f;
    public float tiltAmount = 15.0f; // The amount of tilt when moving left or right
    private bool facingRight = true;
    private Rigidbody2D rb;

    //public GameObject playArea;

    private BoxCollider2D boxCollider;
    private float minX, maxX, minY, maxY;

    private float width, height;

    void Start()
    {
        // rb = GetComponent<Rigidbody2D>();

        // boxCollider = playArea.GetComponent<BoxCollider2D>();
        // Bounds bounds = boxCollider.bounds;
        // minX = bounds.min.x;
        // maxX = bounds.max.x;
        // minY = bounds.min.y;
        // maxY = bounds.max.y;

        // Bounds ownBounds = GetComponent<BoxCollider2D>().bounds;
        // width = ownBounds.max.x - ownBounds.min.x;
        // height = ownBounds.max.y - ownBounds.min.y;
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector2 movement = new Vector2(horizontal, vertical);
        rb.velocity = movement * speed;

        // Image Tilting
        float tilt = horizontal * -tiltAmount;
        transform.rotation = Quaternion.Euler(0, 0, tilt);

        // Image Flipping
        bool steeringRight = horizontal > 0;
        bool steeringLeft = horizontal < 0;

        if (steeringRight && !facingRight || steeringLeft && facingRight)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            facingRight = !facingRight;
        }

        float clampedX = Mathf.Clamp(transform.position.x, minX + width / 2, maxX - width / 2);
        float clampedY = Mathf.Clamp(transform.position.y, minY + height / 2, maxY - height / 2);
        transform.position = new Vector2(clampedX, clampedY);

    }
}


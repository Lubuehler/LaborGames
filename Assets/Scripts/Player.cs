using Fusion;
using UnityEngine;


public class Player : NetworkBehaviour
{
  protected NetworkRigidbody2D _nrb2d;
  public float speed = 5.0f;
  public float tiltAmount = 15.0f; // The amount of tilt when moving left or right

  private SpriteRenderer m_spriteRenderer;


  private void Awake()
  {
    _nrb2d = GetComponent<NetworkRigidbody2D>();
    m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
  }

  public override void FixedUpdateNetwork()
  {
    if (GetInput(out NetworkInputData data))
    {
      data.direction.Normalize();
      _nrb2d.Rigidbody.velocity = data.direction * speed;

      // Image Tilting
      float tilt = data.direction.x * -tiltAmount;
      transform.rotation = Quaternion.Euler(0, 0, tilt);

      // // Image Flipping
      // bool steeringRight = data.direction.x > 0;
      // bool steeringLeft = data.direction.x < 0;

      // if (steeringRight)
      // {
      //   m_spriteRenderer.flipX = steeringLeft;
      // }
      // else if (steeringLeft)
      // {
      //   m_spriteRenderer.flipX = steeringLeft;

      // }



      // float clampedX = Mathf.Clamp(transform.position.x, minX + width / 2, maxX - width / 2);
      // float clampedY = Mathf.Clamp(transform.position.y, minY + height / 2, maxY - height / 2);
      // transform.position = new Vector2(clampedX, clampedY);
    }
  }

  public override void Spawned()
  {
    if (HasInputAuthority)
    {

      Debug.Log("Has Authority!");
      Camera.main.GetComponent<CameraScript>().target = GetComponent<NetworkTransform>().InterpolationTarget;
    }
  }
}
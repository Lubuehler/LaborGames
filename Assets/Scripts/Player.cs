using Fusion;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class Player : NetworkBehaviour
{
  protected NetworkRigidbody2D _nrb2d;
  public float speed = 5.0f;
  public float tiltAmount = 15.0f; // The amount of tilt when moving left or right

  private SpriteRenderer m_spriteRenderer;

  public Projectile _projectilePrefab;


  private void Awake()
  {
    _nrb2d = GetComponent<NetworkRigidbody2D>();
    m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
  }

  private void Start()
  {
    StartCoroutine(Fire());
  }

  private float delayBetweenCalls = 0.5f;
  private bool repeatCondition = true;



  public override void FixedUpdateNetwork()
  {
    if (GetInput(out NetworkInputData data))
    {
      data.direction.Normalize();
      _nrb2d.Rigidbody.velocity = data.direction * speed;

      // Image Tilting
      float tilt = data.direction.x * -tiltAmount;
      transform.rotation = Quaternion.Euler(0, 0, tilt);


      // float clampedX = Mathf.Clamp(transform.position.x, minX + width / 2, maxX - width / 2);
      // float clampedY = Mathf.Clamp(transform.position.y, minY + height / 2, maxY - height / 2);
      // transform.position = new Vector2(clampedX, clampedY);
    }

  }

  private IEnumerator Fire()
  {
    while (repeatCondition)
    {
      List<NetworkObject> enemies = GameController.Instance.GetEnemies();
      NetworkObject closestEnemy = null;
      float minDistance = float.MaxValue;

      foreach (var enemy in enemies)
      {
        if (enemy != null)
        {
          Vector3 direction = enemy.transform.position - _nrb2d.transform.position;
          float distance = direction.magnitude;

          if (distance < minDistance)
          {
            minDistance = distance;
            closestEnemy = enemy;
          }
        }
      }

      if (closestEnemy != null)
      {
        Vector3 direction = closestEnemy.transform.position - _nrb2d.transform.position;

        // Calculate the rotation to look at the closest enemy
        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);

        var projectile = Runner.Spawn(_projectilePrefab, _nrb2d.transform.position, rotation, Object.InputAuthority);

        if (projectile != null)
        {
          projectile.Fire(direction.normalized);
        }
      }

      yield return new WaitForSeconds(delayBetweenCalls);
    }
  }



  public override void Spawned()
  {
    if (HasInputAuthority)
    {
      Camera.main.GetComponent<CameraScript>().target = GetComponent<NetworkTransform>().InterpolationTarget;
    }
  }
}
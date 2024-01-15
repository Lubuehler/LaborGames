using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody2D _rigidbody;
    public float speed = 20.0f;
    public LayerMask collisionLayers;
    private Weapon weapon;

    private int shotID;

    public void Fire(Vector2 direction, Weapon weapon, int shotID)
    {
        _rigidbody.velocity = direction * speed;
        StartCoroutine(DestroyAfterTime());
        this.weapon = weapon;
        this.shotID = shotID;
    }


    IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(5);
        Destroy(gameObject);
    }

    protected void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if ((collisionLayers.value & (1 << collision.gameObject.layer)) != 0)
        {
            weapon.OnBulletHit(collision.gameObject.GetComponent<Enemy>(), shotID);

            Destroy(gameObject);
        }
    }
}

using System.Collections;
using Fusion;
using UnityEngine;

public class Dash : NetworkBehaviour
{
    public float dashSpeed = 50f;
    public float dashDuration = 1f;

    public void Activate(NetworkRigidbody2D rigidbody)
    {
        StartCoroutine(DoDash(rigidbody.Rigidbody));
    }

    IEnumerator DoDash(Rigidbody2D rigidbody)
    {
        Vector2 dashDirection = rigidbody.velocity.normalized;
        float timer = 0.0f;
        Vector2 initialPosition = rigidbody.position;

        while (timer <= dashDuration)
        {
            timer += Time.deltaTime;
            float t = timer / dashDuration;
            rigidbody.position = Vector2.Lerp(initialPosition, initialPosition + dashDirection * dashSpeed * dashDuration, t);
            yield return null;
        }
    }
}

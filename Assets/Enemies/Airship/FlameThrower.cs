using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FlameThrower : MonoBehaviour
{
    private float throwTime = 5.0f;

    void Update()
    {
        throwTime -= Time.deltaTime;
        if (throwTime <= 0)
        {
            Destroy(gameObject);
        }
    }
}

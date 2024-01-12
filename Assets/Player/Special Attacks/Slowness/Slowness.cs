using Fusion;
using UnityEngine;

public class Slowness : NetworkBehaviour
{
    private float slownessTime = 5.0f;
    private bool slownessActive = false;
    public void Activate()
    {
        EnemySpawner.Instance.speed = 1.5f;
        slownessActive = true;        
    }

    public void OnEnable()
    {
        slownessTime = 5.0f;
    }

    public override void Render()
    {
        if (slownessActive)
        {
            slownessTime -= Time.deltaTime;

            if (slownessTime <= 0.0f)
            {
                slownessActive = false;
                slownessTime = 5.0f;
                EnemySpawner.Instance.speed = 3;
            }
        }
    }

    public void OnDisable()
    {
        EnemySpawner.Instance.speed = 3;
    }
}

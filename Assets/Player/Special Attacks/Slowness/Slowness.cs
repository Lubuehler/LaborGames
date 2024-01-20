using Fusion;
using UnityEngine;

public class Slowness : NetworkBehaviour
{
    [SerializeField] private float slownessFactor = 0.5f;
    private float slownessTime = 10.0f;
    private bool slownessActive = false;

    public void Activate()
    {
        EnemySpawner.Instance.currentSpeed *= slownessFactor;
        slownessActive = true;
    }

    public void OnEnable()
    {
        slownessTime = 10.0f;
        slownessActive = false;
    }

    public override void Render()
    {
        if (slownessActive)
        {
            slownessTime -= Time.deltaTime;

            if (slownessTime <= 0.0f)
            {
                Reset();
            }
        }
    }

    public void OnDisable()
    {
        Reset();
    }

    private void Reset()
    {
        slownessActive = false;
        slownessTime = 10.0f;
        EnemySpawner.Instance.currentSpeed = EnemySpawner.Instance.defaultSpeed;
    }
}

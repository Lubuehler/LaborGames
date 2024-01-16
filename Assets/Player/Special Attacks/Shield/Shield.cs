using Fusion;
using UnityEngine;

public class Shield : NetworkBehaviour
{
    [SerializeField] private GameObject shield;
    [SerializeField] private Animator shieldAnimator;

    private Weapon weapon;

    private float shieldTime = 10.0f;
    private bool animationTriggered = false;

    public override void Spawned()
    {
        weapon = GetComponentInParent<Weapon>();
        shield.SetActive(false);
    }

    public void OnEnable()
    {
        weapon = GetComponentInParent<Weapon>();
        weapon.shieldActive = false;
    }

    public void Activate()
    {
        shield.SetActive(true);
    }

    public override void Render()
    {
        if (shield.activeSelf)
        {
            shieldTime -= Time.deltaTime;

            if (shieldTime <= 2.0f && !animationTriggered)
            {
                shieldAnimator.SetTrigger("ShieldEnd");
                animationTriggered = true;
            }

            if (shieldTime <= 0.0f)
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
        weapon.shieldActive = false;
        shield.SetActive(false);
        shieldTime = 10.0f;
        animationTriggered = false;
    }
}

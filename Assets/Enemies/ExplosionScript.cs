using UnityEngine;

public class ExplosionScript : MonoBehaviour
{
    private void Start()
    {
        RpcPlayExplosion();
    }


    private void RpcPlayExplosion()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();
        ps.Play();
    }
}


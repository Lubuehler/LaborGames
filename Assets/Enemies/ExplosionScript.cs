using Fusion;
using System.Collections;
using System.Collections.Generic;
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

        print("Play explosion RPC called"+ ps.isPlaying);
        ps.Play();
        print(GetComponent<ParticleSystem>());
        print("played" + ps.isPlaying);
    }
}


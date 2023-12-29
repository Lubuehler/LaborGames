using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class Teleport : NetworkBehaviour
{
    
    void OnParticleSystemStopped()
    {
        Runner.Despawn(GetComponent<NetworkObject>());
    }
}

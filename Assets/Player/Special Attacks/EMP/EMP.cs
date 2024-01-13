using UnityEngine;
using Fusion;

public class EMP : NetworkBehaviour
{
    [SerializeField] private GameObject emp;

    public void Activate(NetworkRigidbody2D rigidbody)
    {
        Runner.Spawn(emp, rigidbody.transform.position, rigidbody.transform.rotation);
    }
}

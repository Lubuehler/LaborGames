using Fusion;

public class Player : NetworkBehaviour
{
  protected NetworkRigidbody2D _nrb2d;


  private void Awake()
  {
    _nrb2d = GetComponent<NetworkRigidbody2D>();
  }

  public override void FixedUpdateNetwork()
  {
    if (GetInput(out NetworkInputData data))
    {
      data.direction.Normalize();
      _nrb2d.Rigidbody.velocity = data.direction;
    }
  }
}
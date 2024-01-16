using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance;
    [SerializeField] private GameObject networkControllerPrefab;
    [SerializeField] public GameObject background;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CreateNetworkController()
    {
        Instantiate(networkControllerPrefab);
    }
}

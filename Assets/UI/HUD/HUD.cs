using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private TMP_Text coins;
    [SerializeField] private TMP_Text waveCounter;
    [SerializeField] private TMP_Text waveTimer;

    // Special attack
    [SerializeField] private GameObject specialAttack;
    [SerializeField] private GameObject loadingOverlay;
    [SerializeField] private Image specialAttackIcon;

    [SerializeField] private VerticalLayoutGroup allyHealthBarPanel;
    [SerializeField] private VerticalLayoutGroup allyHealthBarPrefab;

    [SerializeField] private GameObject arrowPrefab;
    private Dictionary<Player, GameObject> offScreenArrows = new Dictionary<Player, GameObject>();

    List<AllyHealthbar> allyHealthbars = new List<AllyHealthbar>();

    private bool initialized = false;

    private float initialHeight;

    private Weapon weapon;

    private bool subscribed = false;

    public void OnEnable()
    {
        if (!initialized)
        {
            initialHeight = loadingOverlay.GetComponent<RectTransform>().rect.height;
            initialized = true;
        }

        if (LevelController.Instance != null && LevelController.Instance.localPlayer != null)
        {
            setupActiveItem();
        }
    }

    private void setupActiveItem()
    {
        weapon = LevelController.Instance.localPlayer.GetBehaviour<Weapon>();

        if (weapon.selectedSpecialAttack != int.MinValue)
        {
            specialAttack.SetActive(true);
            Item selectedItem = ShopSystem.Instance.allItems.FirstOrDefault(item => item.itemID == weapon.selectedSpecialAttack);
            specialAttack.GetComponentInChildren<TMP_Text>().text = selectedItem?.itemName;
            specialAttackIcon.sprite = selectedItem?.icon;
        }
    }

    private void setupSubscriptions()
    {

        RebuildAllyHealthbars();
        UpdateCoinsCounter();
        updateWaveCounter();
        subscribed = true;
    }

    private void unsubscribe()
    {
        if (subscribed)
        {
            LevelController.Instance.OnPlayerListChanged -= RebuildAllyHealthbars;
            LevelController.Instance.localPlayer.OnCoinsChanged -= UpdateCoinsCounter;
            LevelController.Instance.OnCurrentWaveChanged -= updateWaveCounter;
            subscribed = false;
        }
    }

    private void Update()
    {
        if (LevelController.Instance != null && LevelController.Instance.localPlayer != null)
        {
            if (!subscribed)
            {
                setupSubscriptions();
            }

            UpdateSpecialAttackCooldown();
            UpdateOffScreenArrows();
            updateWaveTimer();

            if (LevelController.Instance.players.Count != allyHealthbars.Count)
            {
                RebuildAllyHealthbars();
            }
        }
    }

    private void OnDisable()
    {
        unsubscribe();
    }

    private void UpdateSpecialAttackCooldown()
    {
        if (specialAttack.activeSelf)
        {
            float newHeight = Mathf.Lerp(0, initialHeight, weapon.specialAttackTimer / weapon.cooldown);
            SetImageHeight(newHeight);
        }
    }

    private void SetImageHeight(float height)
    {
        RectTransform rectTransform = loadingOverlay.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, height);
    }

    public void updateWaveCounter()
    {
        waveCounter.text = "WAVE " + LevelController.Instance.currentWave.ToString();
    }

    public void updateWaveTimer()
    {
        waveTimer.text = LevelController.Instance.RemainingWaveTime.ToString("F0");

    }

    public void RebuildAllyHealthbars()
    {
        for (int i = allyHealthBarPanel.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(allyHealthBarPanel.transform.GetChild(i).gameObject);
        }
        allyHealthbars.Clear();
        foreach (Player player in LevelController.Instance.players)
        {
            if (player == null)
            {
                subscribed = false;
                return;
            }
            if (player == LevelController.Instance.localPlayer)
            {
                continue;
            }
            VerticalLayoutGroup allyHealthBar = Instantiate(allyHealthBarPrefab);
            allyHealthBar.transform.SetParent(allyHealthBarPanel.transform, false);
            var barComponent = allyHealthBar.GetComponent<AllyHealthbar>();
            barComponent.SetPlayer(player);
            allyHealthbars.Add(barComponent);
        }
    }

    public void UpdateCoinsCounter()
    {
        coins.text = LevelController.Instance.localPlayer.coins.ToString();
    }

    private void UpdateOffScreenArrows()
    {
        var players = LevelController.Instance.GetLivingPlayers();
        foreach (var player in players)
        {
            // Don't create an arrow for the local player
            if (player == LevelController.Instance.localPlayer) continue;

            Vector3 screenPos = Camera.main.WorldToViewportPoint(player.transform.position);

            // Check if the player is off-screen
            if (screenPos.x >= 0 && screenPos.x <= 1 && screenPos.y >= 0 && screenPos.y <= 1 && screenPos.z > 0)
            {
                // Player is on-screen, remove arrow if it exists
                if (offScreenArrows.ContainsKey(player))
                {
                    Destroy(offScreenArrows[player]);
                    offScreenArrows.Remove(player);
                }
            }
            else
            {
                // Player is off-screen, add or update arrow
                if (!offScreenArrows.ContainsKey(player))
                {
                    offScreenArrows[player] = Instantiate(arrowPrefab, transform);
                }
                PositionArrow(offScreenArrows[player], screenPos);
            }
        }
        var deadPlayers = LevelController.Instance.GetDeadPlayers();
        foreach (var player in deadPlayers)
        {
            if (offScreenArrows.ContainsKey(player))
            {
                Destroy(offScreenArrows[player]);
                offScreenArrows.Remove(player);
            }
        }
    }

    private void PositionArrow(GameObject arrow, Vector3 screenPos)
    {
        screenPos.x *= Screen.width;
        screenPos.y *= Screen.height;

        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);

        // Calculate direction from the center of the screen to the player
        Vector3 direction = screenPos - screenCenter;

        // Normalize the direction
        direction = direction.normalized;

        float padding = 0;

        // Calculate the edge position
        float max = Mathf.Max(Screen.width, Screen.height);
        Vector3 edgePos = screenCenter + direction * max / 2;
        edgePos = new Vector3(Mathf.Clamp(edgePos.x, padding, Screen.width - padding), Mathf.Clamp(edgePos.y, padding, Screen.height - padding), 0);
        edgePos *= 0.95f;

        // Set the position of the arrow
        arrow.transform.position = edgePos;

        // Calculate the angle to rotate the arrow
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        arrow.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 90)); // Adjusting by -90 degrees if arrow graphic points up
    }

    public void OnDebugDieClick()
    {
        LevelController.Instance.localPlayer.TakeDamage(1000);
    }

    public void OnDebugShopClick()
    {
        LevelController.Instance.isShopping = true;
    }

}

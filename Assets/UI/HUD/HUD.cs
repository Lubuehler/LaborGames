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
        else
        {
            specialAttack.SetActive(false);
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
            if (player == LevelController.Instance.localPlayer) continue;

            Vector3 playerPos = Camera.main.WorldToViewportPoint(player.transform.position);

            if (playerPos.x >= 0 && playerPos.x <= 1 && playerPos.y >= 0 && playerPos.y <= 1 && playerPos.z > 0)
            {
                if (offScreenArrows.ContainsKey(player))
                {
                    Destroy(offScreenArrows[player]);
                    offScreenArrows.Remove(player);
                }
            }
            else
            {
                if (!offScreenArrows.ContainsKey(player))
                {
                    offScreenArrows[player] = Instantiate(arrowPrefab, transform);
                }
                PositionArrow(offScreenArrows[player], playerPos);
            }
        }
        var dead = LevelController.Instance.GetDeadPlayers();
        foreach (var player in dead)
        {
            if (offScreenArrows.ContainsKey(player))
            {
                Destroy(offScreenArrows[player]);
                offScreenArrows.Remove(player);
            }
        }
    }

    private void PositionArrow(GameObject arrow, Vector3 playerPos)
    {
        playerPos.x *= Screen.width;
        playerPos.y *= Screen.height;

        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Vector3 direction = playerPos - screenCenter;

        direction = direction.normalized;

        float padding = 0;

        float max = Mathf.Max(Screen.width, Screen.height);
        Vector3 edgePos = screenCenter + (direction * max / 2);
        edgePos = new Vector3(Mathf.Clamp(edgePos.x, padding, Screen.width - padding), Mathf.Clamp(edgePos.y, padding * 3, Screen.height - padding), 0);
        arrow.transform.position = edgePos - (direction * max / 2 * 0.05f);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        arrow.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 90)); // Adjusting by -90 degrees if arrow graphic points up
    }



}

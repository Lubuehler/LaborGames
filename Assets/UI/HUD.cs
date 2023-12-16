using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private TMP_Text coins;
    [SerializeField] private TMP_Text waveCounter;
    [SerializeField] private TMP_Text waveTimer;
    [SerializeField] private GameObject specialAttack;
    [SerializeField] private GameObject loadingOverlay;

    [SerializeField] private VerticalLayoutGroup allyHealthBarPanel;
    [SerializeField] private VerticalLayoutGroup allyHealthBarPrefab;

    [SerializeField] private GameObject arrowPrefab;
    private Dictionary<Player, GameObject> offScreenArrows = new Dictionary<Player, GameObject>();

    private bool playerListSubscribed = false;


    private bool initialized = false;

    private float animationDuration = 2f;
    private float initialHeight;
    private float specialAttackTimer;
    private bool specialAttackAvailable;

    public void OnEnable()
    {
        if(!initialized) 
        {
            initialHeight = loadingOverlay.GetComponent<RectTransform>().rect.height;
            initialized = true;
        }
        
        if (LevelController.Instance.localPlayer.selectedSpecialAttack != int.MinValue)
        {
            specialAttack.SetActive(true);
            ResetLoadingAnimation();
        }
        specialAttackAvailable = false;
        Debug.Log("OnEnable");
    }

    private void Update()
    {
        if (LevelController.Instance != null && LevelController.Instance.initialized)
        {
            waveCounter.text = "WAVE " + LevelController.Instance.currentWave.ToString();
            waveTimer.text = LevelController.Instance.RemainingWaveTime.ToString("F0");
        }
        if (LevelController.Instance.localPlayer != null)
        {
            LevelController.Instance.localPlayer.OnCoinsChanged += UpdateCoinsCounter;
        }

        if (specialAttack.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Space) && specialAttackAvailable)
            {
                UseSpecialAttack();
            }

            if (!specialAttackAvailable)
            {
                specialAttackTimer -= Time.deltaTime;

                float newHeight = Mathf.Lerp(0, initialHeight, specialAttackTimer / animationDuration);
                SetImageHeight(newHeight);

                if (specialAttackTimer <= 0)
                {
                    specialAttackAvailable = true;
                }
            }
        }

        UpdateOffScreenArrows();

        if (!playerListSubscribed)
        {
            LevelController.Instance.OnPlayerListChanged += OnPlayerListChanged;
            playerListSubscribed = true;
        }
    }

    private void OnDisable()
    {
        LevelController.Instance.OnPlayerListChanged -= OnPlayerListChanged;
        playerListSubscribed = false;
    }

    public void UseSpecialAttack()
    {
        LevelController.Instance.localPlayer.DeployEMP();
        ResetLoadingAnimation();
    }

    private void ResetLoadingAnimation()
    {
        specialAttackAvailable = false;
        specialAttackTimer = animationDuration;
        SetImageHeight(initialHeight);
    }

    private void SetImageHeight(float height)
    {
        RectTransform rectTransform = loadingOverlay.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, height);
    }

    public void OnPlayerListChanged()
    {
        for (int i = allyHealthBarPanel.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(allyHealthBarPanel.transform.GetChild(i).gameObject);
        }
        foreach (Player player in LevelController.Instance.players)
        {
            if (player == LevelController.Instance.localPlayer)
            {
                continue;
            }
            VerticalLayoutGroup allyHealthBar = Instantiate(allyHealthBarPrefab);
            allyHealthBar.transform.SetParent(allyHealthBarPanel.transform, false);
            allyHealthBar.GetComponent<AllyHealthbar>().player = player;
        }
    }

    public void UpdateCoinsCounter(int coins)
    {
        this.coins.text = coins.ToString();
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

}

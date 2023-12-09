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
        
        if (LevelController.Instance.localPlayer.selectedSpecialAttack != Guid.Empty)
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

    public void UpdateCoinsCounter(int coins)
    {
        this.coins.text = coins.ToString();
    }
}

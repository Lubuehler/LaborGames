using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public TMP_Text waveCounter;
    public TMP_Text waveTimer;
    public Image healthbarBackground;
    public Image healthbarForeground;
    public TMP_Text coins;

    private void Update()
    {
        if (LevelController.Instance != null)
        {
            waveTimer.text = LevelController.Instance.RemainingWaveTime.ToString("F0");
        }
        if (LevelController.Instance.localPlayer != null)
        {
            LevelController.Instance.localPlayer.OnCoinsChanged += UpdateCoinsCounter;
        }
    }

    public void UpdateCoinsCounter(int coins)
    {
        this.coins.text = coins.ToString();
    }
}

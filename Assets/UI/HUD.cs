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

    private void Update()
    {
        if (LevelController.Instance != null)
        {
            waveTimer.text = LevelController.Instance.RemainingWaveTime.ToString("F0");
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class Spectator : MonoBehaviour
{
    public TMP_Text waveCounter;
    public TMP_Text waveTimer;
    //public GameObject camera;
    public TMP_Text playerName;

    private void OnEnable()
    {
        string name = Camera.main.GetComponent<CameraScript>().target.GetComponent<Player>().playerName;
        this.playerName.text = "You are spectating " + name;
    }

    private void Update()
    {
        if (LevelController.Instance != null)
        {
            waveCounter.text = "WAVE " + LevelController.Instance.currentWave.ToString();
            waveTimer.text = LevelController.Instance.RemainingWaveTime.ToString("F0");
        }
    }
}

using System;
using UnityEngine;


public class DataController : MonoBehaviour
{
    public static DataController Instance;
    public Data playerData = new Data();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void SaveData()
    {
        string jsonData = JsonUtility.ToJson(this.playerData);
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, "PlayerData.json");

        System.IO.File.WriteAllText(filePath, jsonData);
    }

    public void LoadData()
    {
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, "PlayerData.json");
        if (System.IO.File.Exists(filePath))
        {
            string jsonData = System.IO.File.ReadAllText(filePath);
            this.playerData = JsonUtility.FromJson<Data>(jsonData);
        }
    }

    public void SetScreenMode(int fullScreenMode)
    {


        switch (fullScreenMode)
        {
            case 0: // Fullscreen
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
            case 1: // Windowed
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case 2: // Borderless Windowed
                Screen.fullScreenMode = FullScreenMode.MaximizedWindow;
                break;
            default:
                break;
        }
    }

    public void SetResolution(int resolutionIndex)
    {
        QualitySettings.SetQualityLevel(resolutionIndex);
    }

    public void InitializeSettings()
    {
        // Load saved settings or initialize to default values
        var resolutionIndex = PlayerPrefs.GetInt("Resolution", QualitySettings.GetQualityLevel());
        var fullScreenMode = PlayerPrefs.GetInt("ScreenMode", (int)Screen.fullScreenMode);

        SetResolution(resolutionIndex);
        SetScreenMode(fullScreenMode);
    }

}

[Serializable]
public class Data
{
    public string playerName;
}

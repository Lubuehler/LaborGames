using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


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

    
}

[Serializable]
public class Data
{
    public string playerName;
}

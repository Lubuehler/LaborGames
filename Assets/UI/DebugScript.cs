using System;
using UnityEngine;

public class DebugScript : MonoBehaviour
{
    public void OnDebugDieClick()
    {
        LevelController.Instance.localPlayer.TakeDamage(1000);
    }

    public void OnDebugShopClick()
    {
        LevelController.Instance.isShopping = true;
    }

    public void onDebugGoldClick()
    {
        LevelController.Instance.localPlayer.coins += 1000;
        LevelController.Instance.localPlayer.TriggerGoldChanged();
    }

    public void onSpawnDroneClick()
    {
        EnemySpawner.Instance.SpawnEnemy(EnemyType.Drone);
    }

    public void onSpawnJetClick()
    {
        EnemySpawner.Instance.SpawnEnemy(EnemyType.Jet);
    }

    public void onSpawnLaserDroneClick()
    {
        EnemySpawner.Instance.SpawnEnemy(EnemyType.LaserDrone);
    }

    public void onSpawnAirshipClick()
    {
        EnemySpawner.Instance.SpawnEnemy(EnemyType.Airship);
    }

    public void onToggleSpawn(Boolean isSpawning)
    {
        if (!isSpawning)
        {
            EnemySpawner.Instance.RPC_DespawnEverything();
        }
        LevelController.Instance.isSpawning = isSpawning;
        print(isSpawning);
    }
}

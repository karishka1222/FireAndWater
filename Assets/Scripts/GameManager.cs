using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

// Кладётся на пустой GameManager в каждом уровне.
// Спавнит игрока по сети: мастер = Огонь, второй игрок = Вода.
// Также работает как синглтон — DangerZone запрашивает у него точки спауна.
public class GameManager : MonoBehaviourPun
{
    public static GameManager Instance { get; private set; }

    public Transform fireSpawnPoint;
    public Transform waterSpawnPoint;

    void Awake()
    {
        // Синглтон живёт в пределах одной сцены, при смене сцены создастся заново
        Instance = this;
    }

    // Используется DangerZone для телепорта погибшего игрока
    public Transform GetSpawnPoint(string playerTag)
    {
        if (playerTag == "FirePlayer") return fireSpawnPoint;
        if (playerTag == "WaterPlayer") return waterSpawnPoint;
        return null;
    }

    void Start()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("GameManager: не в комнате Photon. Запусти через MainMenu.");
            return;
        }

        string prefabName;
        Vector3 spawnPos;

        if (PhotonNetwork.IsMasterClient)
        {
            prefabName = "FirePlayer";
            spawnPos = fireSpawnPoint != null ? fireSpawnPoint.position : Vector3.zero;
        }
        else
        {
            prefabName = "WaterPlayer";
            spawnPos = waterSpawnPoint != null ? waterSpawnPoint.position : Vector3.zero;
        }

        Debug.Log("[GameManager] Спауним " + prefabName + " (IsMaster=" + PhotonNetwork.IsMasterClient + ")");
        PhotonNetwork.Instantiate(prefabName, spawnPos, Quaternion.identity);

        // Аналитика: старт уровня
        AnalyticsManager.LogEvent("level_start", new Dictionary<string, object> {
            { "level_name", SceneManager.GetActiveScene().name },
            { "player_role", prefabName }
        });
    }
}

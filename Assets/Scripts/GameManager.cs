using UnityEngine;
using Photon.Pun;

// Кладётся на пустой GameManager в Level1.
// Спавнит игрока по сети: мастер = Огонь, второй = Вода.
public class GameManager : MonoBehaviourPun
{
    public Transform fireSpawnPoint;
    public Transform waterSpawnPoint;

    void Start()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("GameManager: не в комнате Photon. Запусти через MainMenu.");
            return;
        }

        Vector3 spawnPos;
        string prefabName;

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

        PhotonNetwork.Instantiate(prefabName, spawnPos, Quaternion.identity);
    }
}

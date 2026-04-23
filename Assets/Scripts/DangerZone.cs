using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

// Опасная зона: убивает одного игрока и перезагружает сцену для всех
[RequireComponent(typeof(PhotonView))]
public class DangerZone : MonoBehaviourPun
{
    public string killTag; // "FirePlayer" или "WaterPlayer"

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(killTag)) return;

        var otherPv = other.GetComponent<PhotonView>();
        if (otherPv == null || !otherPv.IsMine) return;

        // Просим мастер-клиента перезагрузить уровень
        photonView.RPC("RequestReload", RpcTarget.MasterClient);
    }

    [PunRPC]
    void RequestReload()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().name);
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

// Финиш: когда оба игрока зашли, мастер-клиент грузит следующую сцену
[RequireComponent(typeof(PhotonView))]
public class FinishZone : MonoBehaviourPun
{
    public string nextSceneName = "Win";
    private bool fireArrived = false;
    private bool waterArrived = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        var otherPv = other.GetComponent<PhotonView>();
        if (otherPv == null || !otherPv.IsMine) return;

        if (other.CompareTag("FirePlayer"))
            photonView.RPC("SetFireArrived", RpcTarget.All, true);
        else if (other.CompareTag("WaterPlayer"))
            photonView.RPC("SetWaterArrived", RpcTarget.All, true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var otherPv = other.GetComponent<PhotonView>();
        if (otherPv == null || !otherPv.IsMine) return;

        if (other.CompareTag("FirePlayer"))
            photonView.RPC("SetFireArrived", RpcTarget.All, false);
        else if (other.CompareTag("WaterPlayer"))
            photonView.RPC("SetWaterArrived", RpcTarget.All, false);
    }

    [PunRPC]
    void SetFireArrived(bool v) { fireArrived = v; TryLoadNext(); }

    [PunRPC]
    void SetWaterArrived(bool v) { waterArrived = v; TryLoadNext(); }

    void TryLoadNext()
    {
        if (!fireArrived || !waterArrived) return;
        // Только мастер-клиент вызывает переход — остальные подтянутся автоматически
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(nextSceneName);
        }
    }
}

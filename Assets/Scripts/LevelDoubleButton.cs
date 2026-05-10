using UnityEngine;
using Photon.Pun;

// Двойная кнопка: дверь открыта, пока оба игрока на ней стоят.
// Если включён latchOpen — после первого совместного нажатия дверь
// остаётся открытой навсегда (нужно для уровней, где после нажатия
// игроки должны сойти с кнопки и пройти в дверь).
[RequireComponent(typeof(PhotonView))]
public class LevelDoubleButton : MonoBehaviourPun
{
    public GameObject linkedDoor;
    public bool latchOpen = false;
    private bool fireOnButton = false;
    private bool waterOnButton = false;
    private bool latched = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        var otherPv = other.GetComponent<PhotonView>();
        if (otherPv == null || !otherPv.IsMine) return;

        if (other.CompareTag("FirePlayer"))
            photonView.RPC("SetFireOn", RpcTarget.All, true);
        else if (other.CompareTag("WaterPlayer"))
            photonView.RPC("SetWaterOn", RpcTarget.All, true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var otherPv = other.GetComponent<PhotonView>();
        if (otherPv == null || !otherPv.IsMine) return;

        if (other.CompareTag("FirePlayer"))
            photonView.RPC("SetFireOn", RpcTarget.All, false);
        else if (other.CompareTag("WaterPlayer"))
            photonView.RPC("SetWaterOn", RpcTarget.All, false);
    }

    [PunRPC]
    void SetFireOn(bool v) { fireOnButton = v; UpdateDoor(); }

    [PunRPC]
    void SetWaterOn(bool v) { waterOnButton = v; UpdateDoor(); }

    void UpdateDoor()
    {
        if (linkedDoor == null) return;
        if (latchOpen && fireOnButton && waterOnButton) latched = true;
        bool open = (fireOnButton && waterOnButton) || latched;
        linkedDoor.SetActive(!open);
    }
}

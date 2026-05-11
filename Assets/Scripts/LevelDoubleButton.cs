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
    public AudioClip buttonSound; // звук нажатия (опционально)
    public AudioClip doorSound;   // звук открытия двери (опционально)
    private bool fireOnButton = false;
    private bool waterOnButton = false;
    private bool latched = false;
    private bool wasOpen = false; // отслеживаем переход закрыто→открыто, чтобы не спамить звук двери

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
    void SetFireOn(bool v)
    {
        bool changed = fireOnButton != v;
        fireOnButton = v;
        if (changed && v && buttonSound != null)
            AudioSource.PlayClipAtPoint(buttonSound, transform.position);
        UpdateDoor();
    }

    [PunRPC]
    void SetWaterOn(bool v)
    {
        bool changed = waterOnButton != v;
        waterOnButton = v;
        if (changed && v && buttonSound != null)
            AudioSource.PlayClipAtPoint(buttonSound, transform.position);
        UpdateDoor();
    }

    void UpdateDoor()
    {
        if (linkedDoor == null) return;
        if (latchOpen && fireOnButton && waterOnButton) latched = true;
        bool open = (fireOnButton && waterOnButton) || latched;
        linkedDoor.SetActive(!open);

        if (open && !wasOpen && doorSound != null)
            AudioSource.PlayClipAtPoint(doorSound, linkedDoor.transform.position);
        wasOpen = open;
    }
}

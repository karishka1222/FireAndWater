using UnityEngine;
using Photon.Pun;

// Кнопка: когда на неё наступает правильный персонаж — дверь открывается у всех по сети
[RequireComponent(typeof(PhotonView))]
public class LevelButton : MonoBehaviourPun
{
    public GameObject linkedDoor;
    public string activatorTag;    // "FirePlayer" или "WaterPlayer"
    private bool isPressed = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isPressed) return;
        if (!other.CompareTag(activatorTag)) return;

        // Сработает только у игрока, который реально наступил на кнопку
        var otherPv = other.GetComponent<PhotonView>();
        if (otherPv == null || !otherPv.IsMine) return;

        // Шлём всем команду "открыть дверь"
        photonView.RPC("OpenDoorRPC", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void OpenDoorRPC()
    {
        isPressed = true;
        if (linkedDoor != null) linkedDoor.SetActive(false);
    }
}

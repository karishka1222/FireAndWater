using UnityEngine;
using Photon.Pun;

// Кнопка: когда на неё наступает правильный персонаж — дверь открывается у всех по сети.
// Состояние "открыто" сохраняется до конца уровня — смерть напарника не закрывает дверь.
[RequireComponent(typeof(PhotonView))]
public class LevelButton : MonoBehaviourPun
{
    public GameObject linkedDoor;
    public string activatorTag;    // "FirePlayer" или "WaterPlayer"
    public AudioClip buttonSound;  // звук нажатия кнопки (опционально)
    public AudioClip doorSound;    // звук открытия двери (опционально)
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

        if (buttonSound != null)
            AudioSource.PlayClipAtPoint(buttonSound, transform.position);
        if (doorSound != null && linkedDoor != null)
            AudioSource.PlayClipAtPoint(doorSound, linkedDoor.transform.position);
    }
}

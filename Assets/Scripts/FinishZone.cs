using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

// Финиш: когда оба игрока зашли, мастер-клиент грузит следующую сцену
[RequireComponent(typeof(PhotonView))]
public class FinishZone : MonoBehaviourPun
{
    public string nextSceneName = "Win";
    public AudioClip winSound; // звук победы (опционально)
    private bool fireArrived = false;
    private bool waterArrived = false;
    private bool soundPlayed = false;

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
        if (soundPlayed) return; // защита от повторного срабатывания
        soundPlayed = true;

        // Звук победы — играем локально на этом клиенте (RPC уже на всех привёл сюда)
        if (winSound != null)
            AudioSource.PlayClipAtPoint(winSound, transform.position);

        // Аналитика: уровень пройден
        AnalyticsManager.LogEvent("level_complete", new Dictionary<string, object> {
            { "level_name", SceneManager.GetActiveScene().name },
            { "next_scene", nextSceneName }
        });

        // Только мастер-клиент вызывает переход. Перед сменой сцены ждём,
        // чтобы успел доиграть звук победы — иначе сцена выгрузится мгновенно
        // и временный AudioSource из PlayClipAtPoint умрёт с ней.
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(LoadNextAfterDelay(1.5f));
        }
    }

    private System.Collections.IEnumerator LoadNextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        PhotonNetwork.LoadLevel(nextSceneName);
    }
}

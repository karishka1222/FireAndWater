using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

// Опасная зона: телепортирует только погибшего игрока на его точку спауна.
// Кнопки и двери НЕ сбрасываются — партнёр продолжает прогресс,
// иначе случайная смерть одного будет ронять усилия другого.
//
// Поддерживает один или два killTag. Для лужи воды — только killTag = "FirePlayer".
// Для пилы (убивает обоих) — killTag = "FirePlayer" + killTagSecondary = "WaterPlayer".
[RequireComponent(typeof(PhotonView))]
public class DangerZone : MonoBehaviourPun
{
    public string killTag;          // основной тег жертвы
    public string killTagSecondary; // опционально: второй тег (если зона убивает двоих, например пила)
    public AudioClip deathSound;    // звук смерти (опционально)

    // Защита от повторных смертей подряд (несколько коллайдеров / тайлов рядом)
    private float lastDeathTime = -10f;
    private const float DeathCooldown = 0.5f;

    // OnTriggerEnter ловит вход в зону, OnTriggerStay подстраховывает —
    // если из-за лагов или быстрых прыжков Enter не сработал, Stay добьёт.
    void OnTriggerEnter2D(Collider2D other) => HandleHit(other);
    void OnTriggerStay2D(Collider2D other) => HandleHit(other);

    private bool MatchesKillTag(Collider2D other)
    {
        if (!string.IsNullOrEmpty(killTag) && other.CompareTag(killTag)) return true;
        if (!string.IsNullOrEmpty(killTagSecondary) && other.CompareTag(killTagSecondary)) return true;
        return false;
    }

    private void HandleHit(Collider2D other)
    {
        if (Time.time - lastDeathTime < DeathCooldown) return;
        if (!MatchesKillTag(other)) return;

        var otherPv = other.GetComponent<PhotonView>();
        if (otherPv == null || !otherPv.IsMine) return;

        lastDeathTime = Time.time;

        // Звук смерти — играем у всех клиентов через RPC
        photonView.RPC(nameof(PlayDeathSoundRPC), RpcTarget.All);

        // Тег убитого игрока — берём с самого Collider, а не из killTag,
        // потому что в случае двух тегов мы заранее не знаем кто умер.
        string victimTag = other.tag;

        AnalyticsManager.LogEvent("player_death", new Dictionary<string, object> {
            { "level_name", SceneManager.GetActiveScene().name },
            { "player", victimTag }
        });

        if (GameManager.Instance != null)
        {
            Transform spawn = GameManager.Instance.GetSpawnPoint(victimTag);
            if (spawn != null)
            {
                Debug.Log("[DangerZone] Телепорт " + victimTag + " в " + spawn.position);
                TeleportPlayer(other.gameObject, spawn.position);
            }
            else
            {
                Debug.LogWarning("[DangerZone] Точка спауна для " + victimTag + " не назначена в GameManager!");
            }
        }
        else
        {
            Debug.LogWarning("[DangerZone] GameManager.Instance == null — нет синглтона в сцене?");
        }
    }

    [PunRPC]
    void PlayDeathSoundRPC()
    {
        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
    }

    // Надёжный телепорт: синхронизируем и Transform, и Rigidbody2D
    private void TeleportPlayer(GameObject player, Vector3 pos)
    {
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.position = pos;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        player.transform.position = pos;
    }
}

using UnityEngine;
using Photon.Pun;

public class WaterPlayerController : MonoBehaviourPun
{
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public AudioClip jumpSound; // звук прыжка (опционально)
    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded = false;

    // Сглаженный ввод — повторяет поведение Input.GetAxis у огня (плавное нарастание).
    // Без сглаживания velocity скачет мгновенно с 0 на 5, физика 2D иногда
    // "пропускает" вход в триггеры при таких резких изменениях.
    private float currentMoveX = 0f;
    private const float InputSmoothing = 0.1f; // время в секундах от 0 до максимума

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        // Цель ввода от клавиш J/L
        float targetMoveX = 0f;
        if (Input.GetKey(KeyCode.J)) targetMoveX = -1f;
        if (Input.GetKey(KeyCode.L)) targetMoveX = 1f;

        // Плавно подтягиваем currentMoveX к target за InputSmoothing секунд
        currentMoveX = Mathf.MoveTowards(currentMoveX, targetMoveX, Time.deltaTime / InputSmoothing);

        rb.linearVelocity = new Vector2(currentMoveX * moveSpeed, rb.linearVelocity.y);

        // Прыжок: I
        if (Input.GetKeyDown(KeyCode.I) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            photonView.RPC(nameof(PlayJumpSoundRPC), RpcTarget.All);
        }

        // Параметры аниматора — синхронизируются на удалённых через PhotonAnimatorView
        if (animator != null)
        {
            animator.SetBool("isRunning", Mathf.Abs(rb.linearVelocity.x) > 0.1f);
            animator.SetBool("isGrounded", isGrounded);
            animator.SetFloat("ySpeed", rb.linearVelocity.y);
        }

        // Поворот персонажа через localScale — Photon синхронизирует scale через PhotonTransformView
        float vx = rb.linearVelocity.x;
        Vector3 s = transform.localScale;
        float absX = Mathf.Abs(s.x);
        if (vx > 0.1f) transform.localScale = new Vector3(absX, s.y, s.z);
        else if (vx < -0.1f) transform.localScale = new Vector3(-absX, s.y, s.z);
    }

    [PunRPC]
    void PlayJumpSoundRPC()
    {
        if (jumpSound != null)
            AudioSource.PlayClipAtPoint(jumpSound, transform.position);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
}

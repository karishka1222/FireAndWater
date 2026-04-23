using UnityEngine;
using Photon.Pun;

public class WaterPlayerController : MonoBehaviourPun
{
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    private Rigidbody2D rb;
    private bool isGrounded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Управляем только своим персонажем (по сети)
        if (!photonView.IsMine) return;

        // Движение: J/L
        float moveX = 0f;
        if (Input.GetKey(KeyCode.J)) moveX = -1f;
        if (Input.GetKey(KeyCode.L)) moveX = 1f;
        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);

        // Прыжок: I
        if (Input.GetKeyDown(KeyCode.I) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
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

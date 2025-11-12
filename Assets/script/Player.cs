using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    EventLoot loot;

    [SerializeField] float speed = 5f;
    [SerializeField] float idleScaleSpeed;
    [SerializeField] float idleScaleAmount;
    private bool isTouchingPlayer = false;
    private Vector3 originalScale;
    private Rigidbody2D rb;

    private Vector2 moveInput; // Ajout

    private void Start()
    {
        originalScale = transform.localScale;
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
    }

    void Update()
    {
        moveInput = Vector2.zero;
        if (Input.GetKey(KeyCode.W))
            moveInput.y = 1f;
        if (Input.GetKey(KeyCode.S))
            moveInput.y = -1f;
        if (Input.GetKey(KeyCode.A))
            moveInput.x = -1f;
        if (Input.GetKey(KeyCode.D))
            moveInput.x = 1f;
        moveInput = moveInput.normalized;

        if (!isTouchingPlayer)
        {
            if (!Input.anyKey)
            {
                IdleEffect();
            }
        }
    }

    void FixedUpdate()
    {
        if (moveInput != Vector2.zero)
        {
            rb.MovePosition(rb.position + moveInput * speed * Time.fixedDeltaTime);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            isTouchingPlayer = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            isTouchingPlayer = false;
        }
    }

    void IdleEffect()
    {
        float scaleOffset = Mathf.Sin(Time.time * idleScaleSpeed) * idleScaleAmount;
        transform.localScale = originalScale + new Vector3(scaleOffset, scaleOffset, 0);
    }
}
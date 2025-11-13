using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    EventLoot loot;

    [SerializeField] float speed = 5f; 
    [SerializeField] float idleScaleSpeed;
    [SerializeField] float idleScaleAmount;

    [SerializeField] private PlayerEquipment equipment; 

    private bool isTouchingPlayer = false;
    private Vector3 originalScale;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    private float lastSpeed = float.NaN; 

    private void Start()
    {
        originalScale = transform.localScale;
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        if (equipment == null)
            equipment = GetComponent<PlayerEquipment>();
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
        float bonus = equipment != null ? equipment.GetMoveSpeedBonus() : 0f;
        float currentSpeed = Mathf.Max(0f, speed + bonus);
        if (Mathf.Abs(currentSpeed - lastSpeed) > 0.01f)
        {
            Debug.Log($"[PlayerMovement] Speed update base={speed}, bonus={bonus}, current={currentSpeed}");
            lastSpeed = currentSpeed;
        }
    }

    void FixedUpdate()
    {
        if (moveInput != Vector2.zero)
        {
            float bonus = equipment != null ? equipment.GetMoveSpeedBonus() : 0f;
            float currentSpeed = Mathf.Max(0f, speed + bonus);
            rb.MovePosition(rb.position + moveInput * currentSpeed * Time.fixedDeltaTime);
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
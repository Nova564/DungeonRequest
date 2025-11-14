using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    EventLoot loot;

    [SerializeField] float speed = 5f;
    [SerializeField] float idleScaleSpeed;
    [SerializeField] float idleScaleAmount;

    [SerializeField] private PlayerEquipment equipment;

    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _hitSound;

    [Header("Combat")]
    [SerializeField] private float maxHealth = 10f;
    [SerializeField] private float flashDuration = 0.08f;
    [SerializeField] private float healPotionEffect = 3f;
    public bool isDead = false;

    private float currentHealth;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private bool isTouchingPlayer = false;
    private Vector3 originalScale;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float walkAnimTime = 0f; 


    private float lastSpeed = float.NaN;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private void Start()
    {
        originalScale = transform.localScale;
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        if (equipment == null)
            equipment = GetComponent<PlayerEquipment>();

        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
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
            if (moveInput.magnitude > 0.01f)
            {
                walkAnimTime += Time.deltaTime * speed;
                float scaleY = originalScale.y + Mathf.Sin(walkAnimTime * 8f) * 0.08f;
                transform.localScale = new Vector3(originalScale.x, scaleY, originalScale.z);
            }
            else
            {
                IdleEffect();
                walkAnimTime = 0f; 
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
        if (collision.gameObject.CompareTag("Heal"))
        {
            currentHealth = Mathf.Min(currentHealth + healPotionEffect, maxHealth);
            Destroy(collision.gameObject); 
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

    public void ApplyHit(float damage)
    {
        if (isDead) return;

        currentHealth -= Mathf.Max(0f, damage);

        if (_audioSource != null && _hitSound != null)
        {
            _audioSource.Stop();
            _audioSource.clip = _hitSound;
            _audioSource.Play();
        }

        StartCoroutine(FlashCoroutine());

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator FlashCoroutine()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
        }
    }

    private void Die()
    {
        isDead = true;
        GameOverMenu gameOverMenu = FindFirstObjectByType<GameOverMenu>();
        if (gameOverMenu != null)
            gameOverMenu.ShowGameOver();

        gameObject.SetActive(false);
    }
}
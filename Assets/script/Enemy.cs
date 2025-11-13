using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GameObject Player;
    private bool isFollowing = false;
    private bool isTouchingPlayer = false;

    [SerializeField] float speed;
    [SerializeField] float idleScaleSpeed;
    [SerializeField] float idleScaleAmount;
    [SerializeField] public float detectionRadius;
    [SerializeField] LayerMask obstacleMask;

    [Header("Combat")]
    [SerializeField] private float maxHealth = 20f;
    [SerializeField] private float knockbackDuration = 0.12f;
    [SerializeField] private float flashDuration = 0.08f;

    private float currentHealth;
    private bool isKnockedBack = false;

    private Vector3 originalScale;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Rigidbody2D rb;
    private Collider2D col;

    void Start()
    {
        obstacleMask = LayerMask.GetMask("Player", "Wall");

        originalScale = transform.localScale;

        currentHealth = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }
    }

    void Update()
    {
        if (!isTouchingPlayer)
        {
            IdleEffect();
        }

        DetectPlayer();

        if (isFollowing)
        {
            Follow(Player);
        }
    }

    void IdleEffect()
    {
        float scaleOffset = Mathf.Sin(Time.time * idleScaleSpeed) * idleScaleAmount;
        transform.localScale = originalScale + new Vector3(scaleOffset, scaleOffset, 0);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isTouchingPlayer = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isTouchingPlayer = false;
        }
    }

    void DetectPlayer()
    {
        if (Player == null) return;

        Vector2 direction = (Player.transform.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, Player.transform.position);

        if (distance <= detectionRadius)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, obstacleMask);

            if (hit.collider == null)
            {
                isFollowing = true;
            }
            else
            {
                isFollowing = false;
            }
        }
        else
        {
            isFollowing = false;
        }
    }

    void Follow(GameObject target)
    {
        if (isKnockedBack) return;
        if (isTouchingPlayer) return;

        Vector3 enemyCenter = GetComponent<Collider2D>().bounds.center;
        Vector3 playerCenter = target.GetComponent<Collider2D>().bounds.center;
        Vector2 direction = (playerCenter - enemyCenter).normalized;
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }
    public void ApplyHit(Vector2 hitOrigin, float damage, float knockbackForce)
    {
        Vector2 dir = ((Vector2)transform.position - hitOrigin).normalized;
        StopAllCoroutines();
        StartCoroutine(KnockbackCoroutine(dir, knockbackForce));
        StartCoroutine(FlashCoroutine());
        TakeDamage(damage);
    }

    private void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private IEnumerator KnockbackCoroutine(Vector2 direction, float force)
    {
        isKnockedBack = true;

        float elapsed = 0f;
        Vector2 startPos = transform.position;

        if (rb != null)
        {
            rb.linearVelocity = direction * force;
            while (elapsed < knockbackDuration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            Vector2 targetOffset = direction * force * 0.05f; 
            Vector2 targetPos = (Vector2)startPos + targetOffset;

            while (elapsed < knockbackDuration)
            {
                float t = elapsed / knockbackDuration;
                transform.position = Vector2.Lerp(startPos, targetPos, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = targetPos;
        }

        isKnockedBack = false;
    }

    private IEnumerator FlashCoroutine()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
        }
        else
        {
            yield return null;
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
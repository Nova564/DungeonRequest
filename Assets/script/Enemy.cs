using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GameObject Player;
    private bool isFollowing = false;
    private bool isTouchingPlayer = false;
    private bool wasFollowing = false;

    [SerializeField] float speed;
    [SerializeField] float idleScaleSpeed;
    [SerializeField] float idleScaleAmount;
    [SerializeField] public float detectionRadius;
    [SerializeField] LayerMask obstacleMask;
    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _hitSound;
    [SerializeField] private AudioClip _followSound;

    [Header("Combat")]
    [SerializeField] private float maxHealth = 20f;
    [SerializeField] private float knockbackDuration = 0.12f;
    [SerializeField] private float flashDuration = 0.08f;
    [SerializeField] private float contactDamage = 4f;
    [SerializeField] private float attackCooldown = 0.75f;
    [SerializeField] private float attackRange = 0.25f;

    private float currentHealth;
    private bool isKnockedBack = false;
    private float nextAttackTime = 0f;

    private Vector3 originalScale;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Rigidbody2D rb;
    private Collider2D col;
    private bool _registered;

    private Vector2 desiredMoveDir = Vector2.zero;

    //enable register pour les ennemies 
    private void OnEnable()
    {
        if (!_registered)
        {
            EnemyTracker.Register();
            _registered = true;
        }
    }
    void Start()
    {
        //petite sécurité 
        if (Player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) Player = go;
        }
        if (obstacleMask == 0)
            obstacleMask = LayerMask.GetMask("Wall", "Obstacle");

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

        UpdateChaseAndAttack();
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            if (isKnockedBack) return;

            rb.linearVelocity = desiredMoveDir * speed;

            if (desiredMoveDir == Vector2.zero && rb.linearVelocity != Vector2.zero)
                rb.linearVelocity = Vector2.zero;
        }
        else
        {
            if (desiredMoveDir != Vector2.zero)
                transform.position += (Vector3)(desiredMoveDir * speed * Time.fixedDeltaTime);
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
            isTouchingPlayer = true;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            isTouchingPlayer = false;
    }

    void DetectPlayer()
    {
        if (Player == null) { isFollowing = false; wasFollowing = false; return; }

        Vector2 direction = (Player.transform.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, Player.transform.position);

        bool previousFollowing = isFollowing;

        if (distance <= detectionRadius)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, obstacleMask);
            isFollowing = (hit.collider == null);
        }
        else
        {
            isFollowing = false;
        }
        //roar son quand il follow
        if (!previousFollowing && isFollowing)
        {
            if (_audioSource != null && _followSound != null)
            {
                _audioSource.Stop();
                _audioSource.clip = _followSound;
                _audioSource.Play();
            }
        }

        wasFollowing = isFollowing;
    }

    void UpdateChaseAndAttack()
    {
        if (Player == null || !isFollowing)
        {
            desiredMoveDir = Vector2.zero;
            return;
        }
        if (isKnockedBack)
        {
            desiredMoveDir = Vector2.zero;
            return;
        }
        //knockback call
        var targetCol = Player.GetComponent<Collider2D>();
        Vector3 enemyCenter = (col != null) ? col.bounds.center : transform.position;
        Vector3 playerCenter = (targetCol != null) ? targetCol.bounds.center : Player.transform.position;
        Vector2 dir = ((Vector2)(playerCenter - enemyCenter)).normalized;

        bool inRange = false;
        if (col != null && targetCol != null)
        {
            ColliderDistance2D cd = col.Distance(targetCol);
            inRange = cd.distance <= attackRange;
        }
        else
        {
            inRange = Vector2.Distance(enemyCenter, playerCenter) <= (attackRange + 0.5f);
        }

        if (inRange)
        {
            desiredMoveDir = Vector2.zero;
            TryAttack();
        }
        else
        {
            desiredMoveDir = dir;
        }
    }

    void TryAttack()
    {   //cooldown logique ici
        if (Time.time < nextAttackTime) return;

        DealDamageToPlayer();

        nextAttackTime = Time.time + attackCooldown;
    }

    void DealDamageToPlayer()
    {   
        if (Player == null) return;

        var player = Player.GetComponent<PlayerMovement>();
        if (player == null) return;
        if (player.isDead) return;

        player.ApplyHit(contactDamage);
    }

    public void ApplyHit(Vector2 hitOrigin, float damage, float knockbackForce)
    {
        Vector2 dir = ((Vector2)transform.position - hitOrigin).normalized;

        if (_audioSource != null && _hitSound != null)
        {
            _audioSource.Stop();
            _audioSource.clip = _hitSound;
            _audioSource.Play();
        }
        //coroutines
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
            Vector2 startPos = transform.position;
            Vector2 targetPos = startPos + direction * force * 0.05f;
            //fluidité knockback
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
    //indicateur dégats
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
        if (rb != null) rb.linearVelocity = Vector2.zero;
        Destroy(gameObject);
    }
    private void OnDestroy()
    {
        if (_registered)
        {
            EnemyTracker.Unregister();
            _registered = false;
        }
    }
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public float GetHealthPercent()
    {
        return Mathf.Clamp01(currentHealth / maxHealth);
    }
}
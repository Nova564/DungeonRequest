using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GameObject Player;
    private bool isFollowing = false;
    private bool isTouchingPlayer = false;

    [SerializeField] float speed;
    [SerializeField] float idleScaleSpeed;
    [SerializeField] float idleScaleAmount;
    [SerializeField] public  float detectionRadius;
    [SerializeField] LayerMask obstacleMask;

    private Vector3 originalScale;

    void Start()
    {
        obstacleMask = LayerMask.GetMask("Player", "obstacle");
        originalScale = transform.localScale;
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
        if (isTouchingPlayer) return; 

        Vector3 enemyCenter = GetComponent<Collider2D>().bounds.center;
        Vector3 playerCenter = target.GetComponent<Collider2D>().bounds.center;
        Vector2 direction = (playerCenter - enemyCenter).normalized;

        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }
}

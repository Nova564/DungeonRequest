using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    EventLoot loot;
    
    [SerializeField] float speed;
    [SerializeField] float idleScaleSpeed;
    [SerializeField] float idleScaleAmount;
    private bool isTouchingPlayer = false;
    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        MovePlayer();
        if (!isTouchingPlayer)
        {
            if (!Input.anyKey) 
            {
                IdleEffect();

            }
            
        }
    }

    void MovePlayer()
    {
        float moveX = 0f;
        float moveY = 0f;

        if (Input.GetKey(KeyCode.W))
            moveY = 1f;
        if (Input.GetKey(KeyCode.S))
            moveY = -1f;
        if (Input.GetKey(KeyCode.A))
            moveX = -1f;
        if (Input.GetKey(KeyCode.D))
            moveX = 1f;

        Vector2 move = new Vector2(moveX, moveY).normalized;

        transform.position += (Vector3)(move * speed * Time.deltaTime);
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
        transform.localScale = originalScale + new Vector3(scaleOffset, scaleOffset,0);
    }
}

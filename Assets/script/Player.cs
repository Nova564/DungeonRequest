using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float idleScaleSpeed;
    [SerializeField] float idleScaleAmount;
    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        MovePlayer();
        if (!Input.anyKey)
        {
            IdleEffect();
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

    void IdleEffect()
    {
        float scaleOffset = Mathf.Sin(Time.time * idleScaleSpeed) * idleScaleAmount;
        transform.localScale = originalScale + new Vector3(scaleOffset, scaleOffset,0);
    }
}

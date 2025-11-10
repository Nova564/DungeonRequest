using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float speed = 5f;

    void Update()
    {
        MovePlayer();
    }

    void MovePlayer()
    {
        float moveX = 0f;
        float moveY = 0f;


        if (Input.GetKey(KeyCode.Z))
            moveY = 1f;
        if (Input.GetKey(KeyCode.S))
            moveY = -1f;
        if (Input.GetKey(KeyCode.Q))
            moveX = -1f;
        if (Input.GetKey(KeyCode.D))
            moveX = 1f;

        Vector2 move = new Vector2(moveX, moveY).normalized;

        transform.position += (Vector3)(move * speed * Time.deltaTime);
    }
}

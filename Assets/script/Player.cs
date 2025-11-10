using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    private SpriteRenderer sr;
    private Color Originalcolor;
    private bool IsPlayerNear=false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            Originalcolor = sr.color;
    }

    void Update()
    {
        MovePlayer();
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

    void HandlingHightLight()
    {
        if (sr == null)
        {
            return;
        }
        if (IsPlayerNear)
        {
            float alpha = Mathf.PingPong(Time.time, 1f);
            sr.color = new Color(1f, 1f, alpha);
        }
        else
        {
            sr.color = Originalcolor;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("loot"))
            {
                Debug.Log("Le joueur a touché une arme !");
                IsPlayerNear = true;
            }

        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("loot"))
            {
                if (Input.GetKey(KeyCode.Space))
                {
                    Destroy(other.gameObject);
                }
            }
        }
    }
}

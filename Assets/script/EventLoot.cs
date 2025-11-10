using UnityEngine;

public class EventLoot : MonoBehaviour
{
    private SpriteRenderer lootRender;
    private Color Originalcolor;
    private bool IsPlayerNear = false;
    void Start()
    {
        lootRender = GetComponent<SpriteRenderer>();
        if (lootRender != null)
            Originalcolor = lootRender.color;
    }
        
    void Update()
    {
        HandlingHightLight();
    }

    void HandlingHightLight()
    {
        if (lootRender == null)
        {
            return;
        }
        if (IsPlayerNear)
        {
            float intensity = 0.5f + Mathf.PingPong(Time.time * 2, 0.5f);
            lootRender.color = new Color(intensity, intensity, intensity);
        }
        else
        {
            lootRender.color = Originalcolor;

        }
    }

    void OnTriggerEnter2D(Collider2D item)
    {
        if (item.CompareTag("Player"))
        {
            lootRender = GetComponent<SpriteRenderer>();
            Debug.Log("Le joueur a touché une arme !");
            if (lootRender != null)
            {
                Originalcolor = lootRender.color;
                IsPlayerNear = true;
            }
        }

    }

    void OnTriggerStay2D(Collider2D item)
    {
        if (item.CompareTag("Player"))
        {
            if (Input.GetKey(KeyCode.Space))
            {
                Destroy(gameObject);
                lootRender = null;
                IsPlayerNear = false;
            }
        }
    }

    void OnTriggerExit2D(Collider2D item)
    {
        if (item.CompareTag("Player"))
        {
            if (lootRender != null)
            {
                lootRender.color = Originalcolor;
                lootRender = null;
            }

        }

    }
}

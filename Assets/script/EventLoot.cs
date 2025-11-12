using UnityEngine;

public class EventLoot : MonoBehaviour
{
    DicoItem objet; // si tu veux t’y connecter plus tard
    private SpriteRenderer lootRender;
    private Color Originalcolor;
    private bool IsPlayerNear = false;

    public string NomDeLarme;

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
            return;

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
            Debug.Log("Le joueur est proche d’un loot !");
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
                
                if (NomDeLarme == "epee")
                {
                    objet.ActiverObjet(NomDeLarme);
                }
                
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

            IsPlayerNear = false;
        }
    }
}

using UnityEngine;

public class EventLoot : MonoBehaviour
{
    private SpriteRenderer lootRender;
    private bool IsPlayerNear = false;

    void Start()
    {
        lootRender = GetComponent<SpriteRenderer>();
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
            lootRender.color = Color.white;
        }
    }

    void OnTriggerEnter2D(Collider2D item)
    {
        if (item.CompareTag("Player"))
        {
            IsPlayerNear = true;
        }
    }

    void OnTriggerStay2D(Collider2D item)
    {
        if (item.CompareTag("Player"))
        {
            if (Input.GetKey(KeyCode.Space))
            {
                if (gameObject.layer == LayerMask.NameToLayer("Arme") || gameObject.layer == LayerMask.NameToLayer("Armure"))
                {
                    PlayerEquipement equipement = item.GetComponent<PlayerEquipement>();
                    if (equipement != null)
                    {
                        equipement.ActiverObjet(gameObject);
                    }

                    Destroy(gameObject);
                    IsPlayerNear = false;
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D item)
    {
        if (item.CompareTag("Player"))
        {
            IsPlayerNear = false;
        }
    }
}

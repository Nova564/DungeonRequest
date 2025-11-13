using UnityEngine;

public class EventLoot : MonoBehaviour
{
    private SpriteRenderer lootRenderer;
    private bool isPlayerNear = false;
    private GameObject playerRef;

    void Start()
    {
        lootRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        HandleHighlight();


        if (isPlayerNear && Input.GetKeyDown(KeyCode.Space))
        {
            TryPickup();
        }
    }

    void HandleHighlight()
    {
        if (lootRenderer == null)
            return;

        if (isPlayerNear)
        {
            float intensity = 0.5f + Mathf.PingPong(Time.time * 2, 0.5f);
            lootRenderer.color = new Color(intensity, intensity, intensity);
        }
        else
        {
            lootRenderer.color = Color.white;
        }
    }

    void OnTriggerEnter2D(Collider2D item)
    {
        if (item.CompareTag("Player"))
        {
            isPlayerNear = true;
            playerRef = item.gameObject;
        }
    }

    void OnTriggerExit2D(Collider2D item)
    {
        if (item.CompareTag("Player"))
        {
            isPlayerNear = false;
            playerRef = null;
        }
    }

    void TryPickup()
    {
        if (playerRef == null)
            return;

        if (gameObject.layer == LayerMask.NameToLayer("Arme") || gameObject.layer == LayerMask.NameToLayer("Armure"))
        {
            PlayerEquipment equipment = playerRef.GetComponent<PlayerEquipment>();

            if (equipment == null)
            {
                return;
            }

            if (equipment.pickedObject != null && gameObject.layer == playerRef.layer)
            {
                equipment.DropObject();
            }

            equipment.PickUpObject(gameObject);

            isPlayerNear = false;
        }
    }
}
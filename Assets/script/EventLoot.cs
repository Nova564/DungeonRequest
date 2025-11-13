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

        PlayerEquipment equipment = playerRef.GetComponent<PlayerEquipment>();

        if (equipment == null)
            return;

        int itemLayer = gameObject.layer;
        GameObject pickedObject = gameObject;

        if (itemLayer == LayerMask.NameToLayer("Weapon"))
        {
            if (equipment.pickedWeapon != null)
            {
                equipment.DropObject(equipment.pickedWeapon);
            }

            equipment.PickUpObject(pickedObject);
            isPlayerNear = false;
            equipment.pickedWeapon = equipment.pickedObject;
        }
        else if (itemLayer == LayerMask.NameToLayer("Armor"))
        {
            if (equipment.pickedArmor != null)
            {
                equipment.DropObject(equipment.pickedArmor);
            }

            equipment.PickUpObject(pickedObject);
            isPlayerNear = false;
            equipment.pickedArmor = equipment.pickedObject;
        }
    }
}
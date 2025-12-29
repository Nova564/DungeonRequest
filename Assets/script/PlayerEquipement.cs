using System;
using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [Header("Player Visual References")]
    public GameObject sword;
    public GameObject axe;
    public GameObject shield;
    public GameObject paladinShield;
    public GameObject dagger;
    public GameObject hammer;

    [Header("Settings")]
    [SerializeField] private KeyCode pickUpKey = KeyCode.E;
    [SerializeField] private KeyCode dropKey = KeyCode.G;
    [SerializeField] private float pickUpRadius = 2f;
    [SerializeField] private LayerMask pickUpLayer;

    [Header("Picked Objects")]
    public GameObject pickedObject;
    public GameObject pickedWeapon;
    public GameObject pickedArmor;

    [Header("Item Stats")]
    public ItemStats currentWeaponStats;
    public ItemStats currentShieldStats;

    private GameObject nearbyItem;

    void Awake()
    {
        DisableAll();
    }

    void Update()
    {
        DetectNearbyItem();

        if (Input.GetKeyDown(pickUpKey) && nearbyItem != null)
        {
            PickUpObject(nearbyItem);
            nearbyItem = null;
        }

        if (Input.GetKeyDown(dropKey))
        {
            if (pickedWeapon != null)
            {
                DropObject(pickedWeapon);
            }
            else if (pickedArmor != null)
            {
                DropObject(pickedArmor);
            }
        }
    }

    private void DisableAll()
    {
        if (sword != null)
            sword.SetActive(false);
        if (axe != null)
            axe.SetActive(false);
        if (shield != null)
            shield.SetActive(false);
        if (paladinShield != null)
            paladinShield.SetActive(false);
        if (dagger != null)
            dagger.SetActive(false);
        if (hammer != null)
            hammer.SetActive(false);

        currentWeaponStats = null;
        currentShieldStats = null;
    }

    private void DetectNearbyItem()
    {
        // Utilisation de Physics2D pour un jeu 2D
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pickUpRadius, pickUpLayer);

        if (hits.Length > 0)
        {
            nearbyItem = hits[0].gameObject;
            Debug.Log($"Objet détecté: {nearbyItem.name} - Layer: {LayerMask.LayerToName(nearbyItem.layer)}");
        }
        else
        {
            nearbyItem = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickUpRadius);
    }

    public void DropObject(GameObject objectToDrop)
    {
        if (objectToDrop == null)
            return;

        objectToDrop.SetActive(true);
        objectToDrop.transform.position = transform.position + Vector3.right * 1.5f;

        if (pickedWeapon != null && objectToDrop == pickedWeapon)
        {
            pickedWeapon = null;
            currentWeaponStats = null;
            DisableWeapons();
        }
        else if (pickedArmor != null && objectToDrop == pickedArmor)
        {
            pickedArmor = null;
            currentShieldStats = null;
            DisableArmors();
        }
    }

    public void PickUpObject(GameObject pickedUpObject)
    {
        Debug.Log($"Tentative de ramassage: {pickedUpObject.name}");

        ItemStats itemStats = null;
        var pickup = pickedUpObject.GetComponent<ItemPickup>();
        if (pickup != null)
            itemStats = pickup.stats;

        if (pickedUpObject.layer == LayerMask.NameToLayer("Weapon"))
        {
            Debug.Log("Arme détectée!");
            if (pickedWeapon != null)
                DropObject(pickedWeapon);

            pickedWeapon = pickedUpObject;
            pickedWeapon.SetActive(false);

            DisableWeapons();

            switch (pickedUpObject.tag)
            {
                case "Sword":
                    if (sword != null)
                        sword.SetActive(true);
                    break;
                case "Axe":
                    if (axe != null)
                        axe.SetActive(true);
                    break;
                case "Dagger":
                    if (dagger != null)
                        dagger.SetActive(true);
                    break;
                case "Hammer":
                    if (hammer != null)
                        hammer.SetActive(true);
                    break;
            }

            currentWeaponStats = itemStats;
        }
        else if (pickedUpObject.layer == LayerMask.NameToLayer("Armor"))
        {
            Debug.Log("Armure détectée!");

            if (pickedArmor != null)
                DropObject(pickedArmor);

            pickedArmor = pickedUpObject;
            pickedArmor.SetActive(false);

            DisableArmors();

            switch (pickedUpObject.tag)
            {
                case "Shield":
                    if (shield != null)
                        shield.SetActive(true);
                    break;
                case "PaladinShield":
                    if (paladinShield != null)
                        paladinShield.SetActive(true);
                    break;
            }

            currentShieldStats = itemStats;
        }
    }

    private void DisableWeapons()
    {
        if (sword != null)
            sword.SetActive(false);
        if (axe != null)
            axe.SetActive(false);
        if (dagger != null)
            dagger.SetActive(false);
        if (hammer != null)
            hammer.SetActive(false);

        currentWeaponStats = null;
    }

    private void DisableArmors()
    {
        if (shield != null)
            shield.SetActive(false);
        if (paladinShield != null)
            paladinShield.SetActive(false);

        currentShieldStats = null;
    }

    public float GetAttackRate()
    {
        return currentWeaponStats != null ? currentWeaponStats.attackRate : 1f;
    }

    public float GetMoveSpeedBonus()
    {
        float bonus = 0f;
        if (currentWeaponStats != null)
            bonus += currentWeaponStats.moveSpeedBonus;
        if (currentShieldStats != null)
            bonus += currentShieldStats.moveSpeedBonus;
        return bonus;
    }
}
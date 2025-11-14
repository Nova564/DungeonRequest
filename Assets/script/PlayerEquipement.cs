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

    [Header("Picked Objects")]
    public GameObject pickedObject;
    public GameObject pickedWeapon;
    public GameObject pickedArmor;

    [Header("Item Stats")]
    public ItemStats currentWeaponStats;
    public ItemStats currentShieldStats;

    void Awake()
    {
        DisableAll();
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
    public void DropObject(GameObject objectToDrop)
    {
        if (objectToDrop == null)
            return;

        objectToDrop.SetActive(true);
        objectToDrop.transform.position = transform.position;

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
        pickedObject = pickedUpObject;
        pickedObject.SetActive(false);

        ItemStats itemStats = null;
        var pickup = pickedUpObject.GetComponent<ItemPickup>();
        if (pickup != null)
            itemStats = pickup.stats;

        if (pickedUpObject.layer == LayerMask.NameToLayer("Weapon"))
        {
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
using UnityEngine;
using TMPro;

public class displayPlayer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerEquipment equipment;
    [SerializeField] private CombatSystem defalt;

    public TextMeshProUGUI statText;

    void Update()
    {
        displaystat();
    }

    void displaystat()
    {
        if (defalt == null)
        {
            statText.text = "Manque le combat system";
            return;
        }

        if (equipment.currentWeaponStats == null && equipment.currentShieldStats == null)
        {
            statText.text =
                $"Weapon: No Weapon\n" +
                $"Shield: No Shield\n" +
                $"Strong: {defalt.defaultDamage}\n" +
                $"Knockback: {defalt.knockbackForce}\n" +
                $"Move Speed Bonus: 0\n" +
                $"Attack Rate: 1";
            return;
        }

        string weaponName = equipment.currentWeaponStats != null
            ? equipment.currentWeaponStats.itemTag : "None";
        string shieldName = equipment.currentShieldStats != null
            ? equipment.currentShieldStats.itemTag : "None";
        float strongValue = equipment.currentWeaponStats != null
        ? equipment.currentWeaponStats.attack : defalt.defaultDamage;


        statText.text =
            $"Weapon: {weaponName}\n" +
            $"Shield: {shieldName}\n" +
            $"Strong: {strongValue}\n" +
            $"Knockback: {defalt.knockbackForce}\n" +
            $"Move Speed Bonus: {equipment.GetMoveSpeedBonus()}\n" +
            $"Attack Rate: {equipment.GetAttackRate()}";
    }
}
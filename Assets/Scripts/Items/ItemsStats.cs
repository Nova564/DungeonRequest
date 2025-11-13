using UnityEngine;

public enum ItemCategory
{
    Weapon,
    Shield
}

[CreateAssetMenu(menuName = "Items/Item Stats", fileName = "NewItemStats")]
public class ItemStats : ScriptableObject
{
    [Header("Identification")]
    [Tooltip("Doit correspondre aux Tags (pr ex : 'Sword', 'Axe', 'Dagger', 'Hammer', 'Shield', 'PaladinShield')")]
    public string itemTag;

    public ItemCategory category = ItemCategory.Weapon;

    [Header("Stats")]
    [Tooltip("Damage added by the item")]
    public float attack = 0f;

    [Tooltip("Pas encore implementé")]
    public float defense = 0f;

    [Tooltip("Additive ou reductive movespeed du player")]
    public float moveSpeedBonus = 0f;

    [Tooltip("Attack par seconde quand un item est équipé")]
    public float attackRate = 1f;
}


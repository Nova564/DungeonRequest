using System.Collections;
using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerEquipment equipment;

    [Header("Attack Settings")]
    [Tooltip("Combien de temps la hitbox est active")]
    [SerializeField] private float attackDuration = 0.15f;

    [Tooltip("Taille de la hitbox")]
    [SerializeField] private Vector2 hitboxSize = new Vector2(2.0f, 2.0f);

    [Tooltip("Layers that can be hit")]
    [SerializeField] private LayerMask hittableLayers;

    [Header("Damage/Knockback")]
    [SerializeField] private float defaultDamage = 1f;
    [SerializeField] private float knockbackForce = 8f;

    [Header("Move Speed")]
    [Tooltip("Base movespeed du joueur")]
    [SerializeField] private float baseMoveSpeed = 5f;

    private float nextAttackAllowedTime = 0f;
    private Transform hitboxTransform;
    private BoxCollider2D hitboxCollider;
    private SpriteRenderer hitboxSpriteRenderer;

    public float CurrentMoveSpeed
    {
        get
        {
            float bonus = equipment != null ? equipment.GetMoveSpeedBonus() : 0f;
            return baseMoveSpeed + bonus;
        }
    }

    private void Awake()
    {
        if (equipment == null)
            equipment = GetComponent<PlayerEquipment>();

        EnsureHitbox();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryAttack();
        }

        UpdateHitboxTransform();
    }

    private void TryAttack()
    {
        float rawAttackRate = equipment != null ? equipment.GetAttackRate() : 1f;
        float cooldown = Mathf.Max(0.01f, rawAttackRate); 

        if (Time.time < nextAttackAllowedTime)
            return;

        nextAttackAllowedTime = Time.time + cooldown;

        if (equipment != null && equipment.currentWeaponStats != null)
        {
            Debug.Log($"attack animation with {equipment.currentWeaponStats.itemTag} (cooldown={cooldown}s)");
        }
        else
        {
            Debug.Log($"attack animation (no weapon equipped) (cooldown={cooldown:0.###}s)");
        }

        StopAllCoroutines();
        StartCoroutine(PerformAttackCoroutine());
    }

    private IEnumerator PerformAttackCoroutine()
    {
        UpdateHitboxTransform();

        SetHitboxVisible(true);
        hitboxCollider.enabled = true;

        Vector2 worldCenter = hitboxTransform.position;

        Collider2D[] hits = Physics2D.OverlapBoxAll(worldCenter, hitboxSize, 0f, hittableLayers);
        for (int i = 0; i < hits.Length; i++)
        {
            Enemy enemy = hits[i].GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                float damage = defaultDamage;
                if (equipment != null && equipment.currentWeaponStats != null)
                    damage = Mathf.Max(0f, equipment.currentWeaponStats.attack);

                enemy.ApplyHit(worldCenter, damage, knockbackForce);
                continue;
            }

            Debug.Log("Hit: " + hits[i].name);
        }

        yield return new WaitForSeconds(attackDuration);

        hitboxCollider.enabled = false;
        SetHitboxVisible(false);
    }

    private void UpdateHitboxTransform()
    {
        if (hitboxTransform == null) return;

        hitboxTransform.localRotation = Quaternion.identity;
        hitboxTransform.position = transform.position;

        if (hitboxCollider != null)
        {
            hitboxCollider.size = hitboxSize;
            hitboxCollider.offset = Vector2.zero;
        }
    }

    private void EnsureHitbox()
    {
        Transform existing = transform.Find("AttackHitbox");
        if (existing != null)
        {
            hitboxTransform = existing;
            hitboxCollider = existing.GetComponent<BoxCollider2D>();
            hitboxSpriteRenderer = existing.GetComponent<SpriteRenderer>();
        }
        else
        {
            GameObject go = new GameObject("AttackHitbox");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            hitboxTransform = go.transform;

            hitboxCollider = go.AddComponent<BoxCollider2D>();
            hitboxCollider.isTrigger = true;
            hitboxCollider.enabled = false;
            hitboxCollider.size = hitboxSize;

            hitboxSpriteRenderer = go.AddComponent<SpriteRenderer>();
            hitboxSpriteRenderer.color = new Color(1f, 0f, 0f, 0.25f);
            hitboxSpriteRenderer.enabled = false;
            hitboxSpriteRenderer.sortingOrder = 1000;
            hitboxSpriteRenderer.sprite = Resources.GetBuiltinResource<Sprite>("Sprites/Default.psd");
        }
    }

    private void SetHitboxVisible(bool visible)
    {
        if (hitboxSpriteRenderer != null)
            hitboxSpriteRenderer.enabled = visible;
    }

    private void OnDrawGizmosSelected()
    {
        if (hitboxTransform == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(hitboxTransform.position, hitboxSize);
    }
}
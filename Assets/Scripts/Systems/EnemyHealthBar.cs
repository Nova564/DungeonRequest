using UnityEngine;

public class EnemyHealthBar : MonoBehaviour
{
    public enum FollowMode
    {
        WorldSpace,     
        ScreenSpaceUI   
    }

    [Header("References")]
    [SerializeField] private Transform fill;
    [SerializeField] private Enemy enemy;

    //la healthbar continue de spawn en dessous de ma map malgré les différents debug je give up
    [Header("Positioning")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private bool followEnemy = true;
    [SerializeField] private FollowMode followMode = FollowMode.WorldSpace;
    [SerializeField] private Camera worldCamera; 

    [Header("Visibility")]
    [SerializeField] private GameObject healthBarContainer;

    private float maxHealth;
    private float currentHealth;
    private RectTransform rectTransform;

    void Start()
    {
        if (enemy == null)
        {
            enemy = GetComponentInParent<Enemy>();
            if (enemy == null && transform.parent != null)
                enemy = transform.parent.GetComponent<Enemy>();
        }

        rectTransform = GetComponent<RectTransform>();

        if (worldCamera == null)
            worldCamera = Camera.main;

        if (enemy != null)
        {
            maxHealth = enemy.GetMaxHealth();
            currentHealth = maxHealth;
        }

        if (!followEnemy && enemy != null && followMode == FollowMode.WorldSpace)
        {
            transform.localPosition = offset;
        }

        HideHealthBar();
    }

    void Update()
    {
        if (enemy == null)
            return;

        if (followEnemy)
        {
            switch (followMode)
            {
                case FollowMode.WorldSpace:
                    transform.position = enemy.transform.position + offset;
                    break;

                case FollowMode.ScreenSpaceUI:
                    if (rectTransform != null && worldCamera != null)
                    {
                        Vector3 screenPos = worldCamera.WorldToScreenPoint(enemy.transform.position + offset);
                        rectTransform.position = screenPos;
                    }
                    break;
            }
        }

        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (fill == null || enemy == null)
            return;

        currentHealth = enemy.GetCurrentHealth();

        float percent = Mathf.Clamp01(currentHealth / maxHealth);
        Vector3 scale = fill.localScale;
        scale.x = percent;
        fill.localScale = scale;

        if (currentHealth >= maxHealth)
        {
            HideHealthBar();
        }
        else
        {
            ShowHealthBar();
        }
    }

    void ShowHealthBar()
    {
        if (healthBarContainer != null)
        {
            healthBarContainer.SetActive(true);
        }
        else
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }
    }

    void HideHealthBar()
    {
        if (healthBarContainer != null)
        {
            healthBarContainer.SetActive(false);
        }
        else
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }
    }
}
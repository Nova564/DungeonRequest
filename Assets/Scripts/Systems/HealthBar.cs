using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Transform fill;
    [SerializeField] private PlayerMovement player;
    
    void Update()
    {
        if (player != null && fill != null)
        {
            float percent = Mathf.Clamp01(player.CurrentHealth / player.MaxHealth);
            Vector3 scale = fill.localScale;
            scale.x = percent;
            fill.localScale = scale;
        }
    }
}
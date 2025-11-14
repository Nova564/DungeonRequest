using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverMenu : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button tryAgainButton;
    [Header("Healthbar ref")]
    [SerializeField] private GameObject healthBar;
    private void Awake()
    {
        gameOverPanel.SetActive(false);
        tryAgainButton.onClick.AddListener(OnTryAgainClicked);
    }

    public void ShowGameOver()
    {
        healthBar.SetActive(false);
        gameOverPanel.SetActive(true);
    }

    private void OnTryAgainClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
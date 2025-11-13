using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverMenu : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button tryAgainButton;

    private void Awake()
    {
        gameOverPanel.SetActive(false);
        tryAgainButton.onClick.AddListener(OnTryAgainClicked);
    }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
    }

    private void OnTryAgainClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
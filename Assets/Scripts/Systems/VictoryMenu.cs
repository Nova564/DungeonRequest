using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VictoryMenu : MonoBehaviour
{
    public static VictoryMenu Instance { get; private set; }

    [SerializeField] private GameObject _panel;
    [SerializeField] private Button _restartButton;

    private void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
        if (_panel != null) _panel.SetActive(false);
    }

    private void OnEnable()
    {
        EnemyTracker.AllEnemiesCleared += ShowVictory;
    }

    private void OnDisable()
    {
        EnemyTracker.AllEnemiesCleared -= ShowVictory;
    }

    public void ShowVictory()
    {
        if (_panel != null) _panel.SetActive(true);
        Time.timeScale = 0f; 
    }

    public void OnRestartClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
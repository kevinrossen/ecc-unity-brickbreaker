using UnityEngine;

// Example listener component that demonstrates how to subscribe to decoupled events
// This component handles UI updates when game events occur
public class UIManager_DecoupledEvent : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;
    [SerializeField] private TMPro.TextMeshProUGUI livesText;
    [SerializeField] private GameObject gameOverPanel;

    private void OnEnable()
    {
        // Subscribe to GameManager events
        GameManager_DecoupledEvent.OnScoreChanged += UpdateScoreUI;
        GameManager_DecoupledEvent.OnLivesChanged += UpdateLivesUI;
        GameManager_DecoupledEvent.OnGameOver += ShowGameOver;

        // Initialize UI with current values
        if (GameManager_DecoupledEvent.Instance != null)
        {
            UpdateScoreUI(GameManager_DecoupledEvent.Instance.score);
            UpdateLivesUI(GameManager_DecoupledEvent.Instance.lives);
        }
    }

    private void OnDisable()
    {
        // Always unsubscribe to prevent memory leaks
        GameManager_DecoupledEvent.OnScoreChanged -= UpdateScoreUI;
        GameManager_DecoupledEvent.OnLivesChanged -= UpdateLivesUI;
        GameManager_DecoupledEvent.OnGameOver -= ShowGameOver;
    }

    private void UpdateScoreUI(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {newScore}";
        }
    }

    private void UpdateLivesUI(int newLives)
    {
        if (livesText != null)
        {
            livesText.text = $"Lives: {newLives}";
        }
    }

    private void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
}
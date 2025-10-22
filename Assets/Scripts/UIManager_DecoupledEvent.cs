using UnityEngine;

// Example listener component that demonstrates how to subscribe to decoupled events
// This component handles UI updates when game events occur
public class UIManager_DecoupledEvent : MonoBehaviour
{
    private static UIManager_DecoupledEvent instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    [Header("UI References")]
    [SerializeField] private TMPro.TextMeshProUGUI scoreLabelText;
    [SerializeField] private TMPro.TextMeshProUGUI scoreValueText;
    [SerializeField] private TMPro.TextMeshProUGUI livesLabelText;
    [SerializeField] private TMPro.TextMeshProUGUI livesValueText;
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
            // Set label text and alignment
            if (scoreLabelText != null) {
                scoreLabelText.text = "Score:";
                scoreLabelText.alignment = TMPro.TextAlignmentOptions.TopLeft;
            }
            if (livesLabelText != null) {
                livesLabelText.text = "Lives:";
                livesLabelText.alignment = TMPro.TextAlignmentOptions.TopRight;
            }
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
        if (scoreValueText != null)
        {
            scoreValueText.text = newScore.ToString();
            scoreValueText.alignment = TMPro.TextAlignmentOptions.Center;
        }
    }

    private void UpdateLivesUI(int newLives)
    {
        if (livesValueText != null)
        {
            livesValueText.text = newLives.ToString();
            livesValueText.alignment = TMPro.TextAlignmentOptions.Center;
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
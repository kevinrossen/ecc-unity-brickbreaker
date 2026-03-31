using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatUIManager : MonoBehaviour
{
    [Header("Player References")]
    public PlayerStats localPlayer;
    
    [Header("UI Elements - Player")]
    public Slider playerHealthBar;
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI playerLevelText;
    public Slider playerExpBar;
    
    [Header("UI Elements - Enemy")]
    public GameObject enemyStatsPanel;
    public Slider enemyHealthBar;
    public TextMeshProUGUI enemyHealthText;
    public TextMeshProUGUI enemyNameText;
    public TextMeshProUGUI enemyLevelText;
    
    [Header("Combat Info")]
    public TextMeshProUGUI combatLogText;
    public Button attackButton;
    
    private EnemyStats currentDisplayedEnemy;
    
    void Start()
    {
        // Find local player if not assigned
        if (localPlayer == null)
        {
            localPlayer = FindObjectOfType<PlayerStats>();
        }
        
        // Set up attack button
        if (attackButton != null)
        {
            attackButton.onClick.AddListener(OnAttackButtonClicked);
        }
        
        // Hide enemy panel initially
        if (enemyStatsPanel != null)
        {
            enemyStatsPanel.SetActive(false);
        }
    }
    
    void Update()
    {
        UpdatePlayerUI();
        UpdateEnemyUI();
        UpdateCombatButtons();
    }
    
    void UpdatePlayerUI()
    {
        if (localPlayer == null) return;
        
        // Update health
        if (playerHealthBar != null)
        {
            playerHealthBar.maxValue = localPlayer.health.GetValue();
            playerHealthBar.value = localPlayer.currentHealth;
        }
        
        if (playerHealthText != null)
        {
            playerHealthText.text = $"{Mathf.Round(localPlayer.currentHealth)}/{Mathf.Round(localPlayer.health.GetValue())}";
        }
        
        // Update level
        if (playerLevelText != null)
        {
            playerLevelText.text = $"Level {localPlayer.level}";
        }
        
        // Update experience
        if (playerExpBar != null)
        {
            playerExpBar.maxValue = localPlayer.experienceToNextLevel;
            playerExpBar.value = localPlayer.experience;
        }
    }
    
    void UpdateEnemyUI()
    {
        if (localPlayer == null) return;
        
        // Check if player has a target
        if (localPlayer.targetedEnemy != null && localPlayer.targetedEnemy.gameObject.activeInHierarchy)
        {
            currentDisplayedEnemy = localPlayer.targetedEnemy;
            
            // Show enemy panel
            if (enemyStatsPanel != null && !enemyStatsPanel.activeSelf)
            {
                enemyStatsPanel.SetActive(true);
            }
            
            // Update enemy health
            if (enemyHealthBar != null)
            {
                enemyHealthBar.maxValue = currentDisplayedEnemy.health.GetValue();
                enemyHealthBar.value = currentDisplayedEnemy.currentHealth;
            }
            
            if (enemyHealthText != null)
            {
                enemyHealthText.text = $"{Mathf.Round(currentDisplayedEnemy.currentHealth)}/{Mathf.Round(currentDisplayedEnemy.health.GetValue())}";
            }
            
            // Update enemy name
            if (enemyNameText != null)
            {
                enemyNameText.text = currentDisplayedEnemy.characterName;
            }
            
            // Show enemy type/difficulty
            if (enemyLevelText != null)
            {
                enemyLevelText.text = $"Type: {currentDisplayedEnemy.enemyType}";
            }
        }
        else
        {
            // Hide enemy panel if no target
            if (enemyStatsPanel != null && enemyStatsPanel.activeSelf)
            {
                enemyStatsPanel.SetActive(false);
            }
            currentDisplayedEnemy = null;
        }
    }
    
    void UpdateCombatButtons()
    {
        if (attackButton != null)
        {
            // Enable attack button only if player has a target
            attackButton.interactable = (localPlayer != null && localPlayer.targetedEnemy != null);
        }
    }
    
    void OnAttackButtonClicked()
    {
        if (localPlayer != null)
        {
            localPlayer.AttackTarget();
        }
    }
    
    public void AddCombatLog(string message)
    {
        if (combatLogText != null)
        {
            combatLogText.text = message + "\n" + combatLogText.text;
            
            // Limit log to last 10 lines
            string[] lines = combatLogText.text.Split('\n');
            if (lines.Length > 10)
            {
                combatLogText.text = string.Join("\n", lines, 0, 10);
            }
        }
    }
}
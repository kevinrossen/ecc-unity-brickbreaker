using UnityEngine;
using UnityEngine.Events;

// Example of component-based architecture for Week 4 lesson
// This shows how to break down the monolithic GameManager into smaller, focused components

namespace ComponentExample
{
    // Score management component
    public class ScoreManager : MonoBehaviour
    {
        [Header("Score Settings")]
        [SerializeField] private int currentScore = 0;
        [SerializeField] private int highScore = 0;
        
        [Header("Score Multipliers")]
        [Range(1f, 5f)]
        [SerializeField] private float comboMultiplier = 1f;
        
        [Space]
        [Header("Events")]
        public UnityEvent<int> OnScoreChanged;
        public UnityEvent<int> OnHighScoreBeaten;
        public UnityEvent OnComboIncreased;
        
        private void Start()
        {
            LoadHighScore();
        }
        
        public void AddScore(int points)
        {
            int scoreToAdd = Mathf.RoundToInt(points * comboMultiplier);
            currentScore += scoreToAdd;
            OnScoreChanged?.Invoke(currentScore);
            
            if (currentScore > highScore)
            {
                highScore = currentScore;
                SaveHighScore();
                OnHighScoreBeaten?.Invoke(highScore);
            }
        }
        
        public void ResetScore()
        {
            currentScore = 0;
            comboMultiplier = 1f;
            OnScoreChanged?.Invoke(currentScore);
        }
        
        public void IncreaseCombo()
        {
            comboMultiplier = Mathf.Min(comboMultiplier + 0.5f, 5f);
            OnComboIncreased?.Invoke();
        }
        
        public void ResetCombo()
        {
            comboMultiplier = 1f;
        }
        
        private void LoadHighScore()
        {
            highScore = PlayerPrefs.GetInt("HighScore", 0);
        }
        
        private void SaveHighScore()
        {
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }
        
        public int GetScore() => currentScore;
        public int GetHighScore() => highScore;
        public float GetComboMultiplier() => comboMultiplier;
    }
    
    // Lives management component
    public class LivesManager : MonoBehaviour
    {
        [Header("Lives Settings")]
        [Range(1, 10)]
        [SerializeField] private int maxLives = 3;
        [SerializeField] private int currentLives;
        
        [Space]
        [Header("Events")]
        public UnityEvent<int> OnLivesChanged;
        public UnityEvent OnLifeLost;
        public UnityEvent OnGameOver;
        
        private void Start()
        {
            currentLives = maxLives;
            OnLivesChanged?.Invoke(currentLives);
        }
        
        public void LoseLife()
        {
            if (currentLives <= 0) return;
            
            currentLives--;
            OnLifeLost?.Invoke();
            OnLivesChanged?.Invoke(currentLives);
            
            if (currentLives <= 0)
            {
                OnGameOver?.Invoke();
            }
        }
        
        public void AddLife()
        {
            if (currentLives < maxLives)
            {
                currentLives++;
                OnLivesChanged?.Invoke(currentLives);
            }
        }
        
        public void ResetLives()
        {
            currentLives = maxLives;
            OnLivesChanged?.Invoke(currentLives);
        }
        
        public int GetLives() => currentLives;
        public bool IsGameOver() => currentLives <= 0;
    }
    
    // Level progression component
    public class LevelManager : MonoBehaviour
    {
        [Header("Level Settings")]
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private LevelConfiguration[] levelConfigurations;
        
        [Header("Level Progress")]
        [SerializeField] private int bricksRemaining;
        
        [Space]
        [Header("Debug Options")]
        [SerializeField] private bool skipToNextLevel;
        
        [Space]
        [Header("Events")]
        public UnityEvent<int> OnLevelStarted;
        public UnityEvent<int> OnLevelCompleted;
        public UnityEvent OnAllLevelsCompleted;
        public UnityEvent<int> OnBrickDestroyed;
        
        [TextArea(3, 5)]
        [SerializeField] private string designerNotes = "Add level design notes here...";
        
        private void Start()
        {
            LoadLevel(currentLevel);
        }
        
        public void LoadLevel(int levelNumber)
        {
            if (levelNumber <= 0 || levelNumber > levelConfigurations.Length)
            {
                Debug.LogError($"Invalid level number: {levelNumber}");
                return;
            }
            
            currentLevel = levelNumber;
            var config = levelConfigurations[levelNumber - 1];
            bricksRemaining = CountBricksInConfiguration(config);
            
            OnLevelStarted?.Invoke(currentLevel);
            
            // Additional level loading logic would go here
        }
        
        public void BrickDestroyed()
        {
            bricksRemaining--;
            OnBrickDestroyed?.Invoke(bricksRemaining);
            
            if (bricksRemaining <= 0)
            {
                CompleteLevel();
            }
        }
        
        private void CompleteLevel()
        {
            OnLevelCompleted?.Invoke(currentLevel);
            
            if (currentLevel < levelConfigurations.Length)
            {
                currentLevel++;
                LoadLevel(currentLevel);
            }
            else
            {
                OnAllLevelsCompleted?.Invoke();
            }
        }
        
        private int CountBricksInConfiguration(LevelConfiguration config)
        {
            if (config == null || config.brickLayout == null) return 0;
            
            int count = 0;
            foreach (var brick in config.brickLayout)
            {
                if (brick != null) count++;
            }
            return count;
        }
        
        // Called from custom inspector button
        public void SkipLevel()
        {
            CompleteLevel();
        }
        
        public int GetCurrentLevel() => currentLevel;
        public int GetBricksRemaining() => bricksRemaining;
    }
    
    // Assuming this is the LevelConfiguration class definition
    public class LevelConfiguration
    {
        // Existing properties or fields

        // Add the brickLayout property
        public GameObject[] brickLayout;
    }
}
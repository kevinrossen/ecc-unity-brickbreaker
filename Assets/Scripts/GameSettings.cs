using UnityEngine;

[CreateAssetMenu(fileName = "Game Settings", menuName = "Game Data/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Player Progression")]
    public int startingLives = 3;
    public int maxLives = 5;
    public int bonusLifeScore = 10000;
    
    [Header("Scoring")]
    public int perfectLevelBonus = 500;
    public float timeBonus = 10f; // points per second remaining
    public float comboMultiplier = 1.5f;
    
    [Header("Controls")]
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode pauseKey = KeyCode.Escape;
    public KeyCode launchBallKey = KeyCode.Space;
    
    [Header("Visual Settings")]
    public bool screenShakeEnabled = true;
    public float screenShakeIntensity = 0.1f;
    public bool particleEffectsEnabled = true;
    
    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    [Range(0f, 1f)]
    public float sfxVolume = 0.8f;
    [Range(0f, 1f)]
    public float musicVolume = 0.6f;
    
    [Header("Difficulty Settings")]
    public float easyBallSpeed = 4f;
    public float normalBallSpeed = 6f;
    public float hardBallSpeed = 8f;
    
    public enum DifficultyLevel
    {
        Easy,
        Normal,
        Hard
    }
    
    public DifficultyLevel currentDifficulty = DifficultyLevel.Normal;
    
    public float GetBallSpeedForDifficulty()
    {
        switch (currentDifficulty)
        {
            case DifficultyLevel.Easy: return easyBallSpeed;
            case DifficultyLevel.Hard: return hardBallSpeed;
            default: return normalBallSpeed;
        }
    }
}

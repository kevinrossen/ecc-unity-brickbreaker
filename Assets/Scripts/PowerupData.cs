using UnityEngine;

[CreateAssetMenu(fileName = "New Powerup", menuName = "Game Data/Powerup")]
public class PowerupData : ScriptableObject
{
    [Header("Visual")]
    public Sprite powerupSprite;
    public Color powerupColor = Color.white;
    public float fallSpeed = 2f;
    
    [Header("Effect")]
    public string powerupName = "Power Up";
    public string description = "Does something awesome!";
    public float duration = 10f;
    public bool isPermanent = false;
    
    [Header("Audio")]
    public AudioClip collectSound;
    public AudioClip activateSound;
    public AudioClip deactivateSound;
    
    [Header("Scoring")]
    public int pointValue = 50;
    
    public enum PowerupType
    {
        PaddleSize,    // Make paddle bigger/smaller
        BallSpeed,     // Change ball speed
        MultiBall,     // Spawn extra balls
        StickyPaddle,  // Ball sticks to paddle
        Penetration,   // Ball goes through bricks
        ExtraLife,     // Add a life
        Points,        // Bonus points
        SlowMotion     // Slow down time
    }
    
    public PowerupType type = PowerupType.Points;
    
    [Header("Type-Specific Settings")]
    public float effectStrength = 1f; // Multiplier or amount
    public int extraBalls = 2; // For MultiBall
    public int extraLives = 1; // For ExtraLife
    public int bonusPoints = 500; // For Points
}

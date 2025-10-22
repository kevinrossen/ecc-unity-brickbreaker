using UnityEngine;

[CreateAssetMenu(fileName = "New Brick Type", menuName = "Game Data/Brick Type")]
public class BrickData : ScriptableObject
{
    [Header("Visual")]
    public Sprite[] healthStates = new Sprite[0];
    public Color brickColor = Color.white;
    
    [Header("Gameplay")]
    public int pointValue = 100;
    public int points => pointValue; // Alias for editor compatibility
    public bool isUnbreakable = false;
    
    [Header("Special Effects")]
    public bool spawnsPowerup = false;
    public GameObject powerupPrefab;
    [Range(0f, 1f)]
    public float powerupDropChance = 0.1f;
    
    [Header("Audio")]
    public AudioClip hitSound;
    public AudioClip destroySound;
}

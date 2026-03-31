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

    [Header("Multi-Ball")]
    public bool triggersMultiBall;
    [Range(1,4)] public int extraBalls = 2;
    public float spreadAngle = 25f;
    
    [Header("Audio")]
    public AudioClip hitSound;
    public AudioClip destroySound;
}

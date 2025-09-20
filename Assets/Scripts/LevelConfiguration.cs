using UnityEngine;

[CreateAssetMenu(fileName = "New Level Config", menuName = "Game Data/Level Configuration")]
public class LevelConfiguration : ScriptableObject
{
    [Header("Level Info")]
    public string levelName = "Level 1";
    public string levelDescription = "A basic level";
    
    [Header("Gameplay Settings")]
    public float ballSpeed = 5f;
    public float paddleSpeed = 8f;
    public int targetScore = 1000;
    
    [Header("Visual Theme")]
    public Color backgroundColor = Color.black;
    public Sprite backgroundImage;
    public Material ballMaterial;
    public Material paddleMaterial;
    
    [Header("Audio")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;
    
    [Header("Grid Settings")]
    public int rows = 6;
    public int columns = 8;
    public GameObject brickPrefab;
    
    [Header("Brick Layout")]
    public BrickData[] availableBrickTypes;
    public Vector2Int gridSize = new Vector2Int(8, 6);
    public float brickSpacing = 0.1f;
    public Vector2 startPosition = new Vector2(-3.5f, 2f);
    
    [Header("Special Rules")]
    public bool hasTimeLimit = false;
    public float timeLimit = 60f;
    public bool ballSpeedIncreases = false;
    public float speedIncreaseRate = 0.1f;
    
    [Header("Layout Data")]
    public System.Collections.Generic.List<BrickData> brickLayout = new System.Collections.Generic.List<BrickData>();
}

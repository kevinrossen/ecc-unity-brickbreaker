using UnityEngine;
using System.Collections.Generic;

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
    public float brickSpacing = 1.4f;
    public Vector2 startPosition = new Vector2(-13f, 9.5f);
    
    [Header("Special Rules")]
    public bool hasTimeLimit = false;
    public float timeLimit = 60f;
    public bool ballSpeedIncreases = false;
    public float speedIncreaseRate = 0.1f;
    
    [Header("Layout Data")]
    public System.Collections.Generic.List<BrickData> layoutData = new System.Collections.Generic.List<BrickData>();

    [Header("Brick Layout Authoring")]
    public List<BrickData> brickLayout = new List<BrickData>(); // length = gridSize.x * gridSize.y

    private void OnValidate()
    {
        // Keep gridSize in sync with rows/columns for consistency
        gridSize = new Vector2Int(columns, rows);

        // Ensure brickLayout is sized to rows * columns
        int target = Mathf.Max(0, columns * rows);
        if (brickLayout == null)
            brickLayout = new List<BrickData>(target);

        if (brickLayout.Count < target)
            brickLayout.AddRange(new BrickData[target - brickLayout.Count]);
        else if (brickLayout.Count > target)
            brickLayout.RemoveRange(target, brickLayout.Count - target);
    }
}

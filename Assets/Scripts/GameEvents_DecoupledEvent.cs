using UnityEngine;

// Centralized event manager following the lesson plan approach
// This demonstrates the GameEvents singleton pattern for managing game-wide events
public class GameEvents_DecoupledEvent : MonoBehaviour
{
    public static GameEvents_DecoupledEvent Instance { get; private set; }

    // Events that components can subscribe to
    public event System.Action<int> OnBrickDestroyed; // Just pass points, not brick reference
    public event System.Action OnBallMissed;
    public event System.Action<int> OnScoreChanged;
    public event System.Action<int> OnLivesChanged;
    public event System.Action OnGameOver;
    public event System.Action<int> OnLevelCompleted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // Methods that components call to announce events
    public void BrickDestroyed(int points)
    {
        OnBrickDestroyed?.Invoke(points);
    }

    public void BallMissed()
    {
        OnBallMissed?.Invoke();
    }

    public void ScoreChanged(int newScore)
    {
        OnScoreChanged?.Invoke(newScore);
    }

    public void LivesChanged(int newLives)
    {
        OnLivesChanged?.Invoke(newLives);
    }

    public void GameOver()
    {
        OnGameOver?.Invoke();
    }

    public void LevelCompleted(int levelNumber)
    {
        OnLevelCompleted?.Invoke(levelNumber);
    }
}

// Alternative Brick that uses GameEvents instead of static events
public class Brick_GameEventsVersion : MonoBehaviour
{
    [Header("Brick Configuration")]
    public BrickData brickData;
    [Tooltip("Optional visual profile that controls color mapping when no sprites are provided.")]
    public BrickVisualProfile visualProfile;

    // Legacy fields (kept for backwards compatibility but hidden to avoid confusion)
    [HideInInspector] public Sprite[] states = new Sprite[0];
    [HideInInspector] public int points = 100;
    [HideInInspector] public bool unbreakable;

    private SpriteRenderer spriteRenderer;
    private int health;
    private AudioSource audioSource;

    private void EnsureComponents()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Awake()
    {
        EnsureComponents();
    }

    private void Start()
    {
        ResetBrick();
    }

    public void ResetBrick()
    {
        // Ensure components exist even when instantiated in Edit mode (Awake may not have run)
        EnsureComponents();
        gameObject.SetActive(true);

        // Use scriptable object data if available, otherwise fall back to legacy fields
        bool useScriptableObject = brickData != null;
        bool isUnbreakableBlock = useScriptableObject ? brickData.isUnbreakable : unbreakable;

        if (!isUnbreakableBlock)
        {
            if (useScriptableObject)
            {
                health = Mathf.Max(1, brickData.maxHealth);
                if (brickData.healthStates.Length > 0 && spriteRenderer != null && (visualProfile == null || visualProfile.preferSpritesWhenAvailable))
                {
                    int idx = SpriteIndexForHealth(health, brickData.maxHealth, brickData.healthStates.Length);
                    spriteRenderer.sprite = brickData.healthStates[Mathf.Clamp(idx, 0, brickData.healthStates.Length - 1)];
                }
                else if (spriteRenderer != null)
                {
                    // No per-health sprites: encode strength via color
                    spriteRenderer.color = HealthToColor(health, brickData.maxHealth, false);
                }
            }
            else
            {
                // Legacy behavior
                health = Mathf.Max(1, states.Length);
                if (states.Length > 0 && spriteRenderer != null)
                {
                    int idx = SpriteIndexForHealth(health, states.Length, states.Length);
                    spriteRenderer.sprite = states[Mathf.Clamp(idx, 0, states.Length - 1)];
                }
            }
        }
    }

    // Maps current health to a sprite index when using a sequence of sprites
    private int SpriteIndexForHealth(int remaining, int max, int spriteCount)
    {
        if (spriteCount <= 0) return -1;
        max = Mathf.Max(1, max);
        remaining = Mathf.Clamp(remaining, 1, max);
        if (max <= spriteCount)
        {
            return Mathf.Clamp(remaining - 1, 0, spriteCount - 1);
        }
        float t = (remaining - 1f) / (max - 1f);
        int idx = Mathf.RoundToInt(t * (spriteCount - 1));
        return Mathf.Clamp(idx, 0, spriteCount - 1);
    }

    private Color HealthToColor(int remaining, int max, bool unbreakable)
    {
        if (unbreakable) return visualProfile != null ? visualProfile.unbreakableColor : Color.gray;
        remaining = Mathf.Max(0, remaining);
        max = Mathf.Max(1, max);
        float t = 1f - Mathf.Clamp01((remaining - 1) / (float)(max - 1 == 0 ? 1 : max - 1));
        if (visualProfile != null && visualProfile.strengthGradient != null)
        {
            return visualProfile.strengthGradient.Evaluate(t);
        }
        if (t < 0.33f) return Color.red;
        if (t < 0.66f) return new Color(1f, 0.5f, 0f);
        if (t < 0.9f) return Color.green;
        return Color.cyan;
    }

    private void Hit()
    {
        EnsureComponents();
        bool useScriptableObject = brickData != null;
        bool isUnbreakableBlock = useScriptableObject ? brickData.isUnbreakable : unbreakable;

        if (isUnbreakableBlock) {
            return;
        }

        // Play hit sound
        if (useScriptableObject && brickData.hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(brickData.hitSound);
        }

        health--;

        if (health <= 0)
        {
            // Play destroy sound
            if (useScriptableObject && brickData.destroySound != null && audioSource != null)
            {
                audioSource.PlayOneShot(brickData.destroySound);
            }

            // Check for powerup spawn
            if (useScriptableObject && brickData.spawnsPowerup && brickData.powerupPrefab != null)
            {
                if (Random.value <= brickData.powerupDropChance)
                {
                    Instantiate(brickData.powerupPrefab, transform.position, Quaternion.identity);
                }
            }

            gameObject.SetActive(false);
        }
        else
        {
            // Update sprite based on remaining health
            if (useScriptableObject && brickData.healthStates.Length > 0 && spriteRenderer != null && (visualProfile == null || visualProfile.preferSpritesWhenAvailable))
            {
                int spriteIndex = SpriteIndexForHealth(health, brickData.maxHealth, brickData.healthStates.Length);
                spriteRenderer.sprite = brickData.healthStates[Mathf.Clamp(spriteIndex, 0, brickData.healthStates.Length - 1)];
            }
            else if (states.Length > 0 && spriteRenderer != null)
            {
                int idx = SpriteIndexForHealth(health, states.Length, states.Length);
                spriteRenderer.sprite = states[Mathf.Clamp(idx, 0, states.Length - 1)];
            }
            else if (useScriptableObject && spriteRenderer != null)
            {
                spriteRenderer.color = HealthToColor(health, brickData.maxHealth, isUnbreakableBlock);
            }
        }

        // Calculate points
        int pointsToAdd = useScriptableObject ? brickData.pointValue : points;

        // Announce through GameEvents instead of static event
        if (GameEvents_DecoupledEvent.Instance != null)
        {
            GameEvents_DecoupledEvent.Instance.BrickDestroyed(pointsToAdd);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Ball") {
            Hit();
        }
    }
}

// Alternative ResetZone that uses GameEvents
public class ResetZone_GameEventsVersion : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // ... (same collision logic)

        // Announce through GameEvents instead of static event
        if (GameEvents_DecoupledEvent.Instance != null)
        {
            GameEvents_DecoupledEvent.Instance.BallMissed();
        }
    }
}
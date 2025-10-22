using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1)]
public class GameManager_DecoupledEvent : MonoBehaviour
{
    public static GameManager_DecoupledEvent Instance { get; private set; }

    private const int NUM_LEVELS = 2;

    [Header("Game State")]
    public int levelDisplay = 1;
    public int scoreDisplay = 0;
    public int livesDisplay = 3;

    public int level { get { return levelDisplay; } private set { levelDisplay = value; } }
    public int score { get { return scoreDisplay; } private set { scoreDisplay = value; } }
    public int lives { get { return livesDisplay; } private set { livesDisplay = value; } }

    [Header("Central Settings")]
    [SerializeField] private GameSettings settings;

    private Ball ball;
    // Paddle implementation (identified by type name at runtime)
    private MonoBehaviour paddleEnhanced;
    private Brick[] bricks;

    // Events that other systems can subscribe to
    public static event System.Action<int> OnScoreChanged;
    public static event System.Action<int> OnLivesChanged;
    public static event System.Action OnGameOver;
    public static event System.Action<int> OnLevelCompleted;

    private void Awake()
    {
        if (Instance != null) {
            DestroyImmediate(gameObject);
        } else {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            FindSceneReferences();
            ApplySettings();
            SubscribeToEvents();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) {
            Instance = null;
            UnsubscribeFromEvents();
        }
    }

    // Ensure UI shows correct starting values even if scene serialized values were zeroed
    private void Start()
    {
        // Some scenes may have serialized zeros for these fields; initialize sensible defaults
        if (lives <= 0) lives = 3;
        if (level <= 0) level = 1;

        // Notify listeners so UI and audio reflect the initial state immediately
        OnScoreChanged?.Invoke(score);
        OnLivesChanged?.Invoke(lives);
    }

    private void SubscribeToEvents()
    {
        // Subscribe to events from other components
        ResetZone.OnBallMissed += HandleBallMissed;
        Brick.OnBrickHit += HandleBrickHit;
    }

    private void UnsubscribeFromEvents()
    {
        // Always unsubscribe to prevent memory leaks
        ResetZone.OnBallMissed -= HandleBallMissed;
        Brick.OnBrickHit -= HandleBrickHit;
    }

    private void FindSceneReferences()
    {
        ball = FindFirstObjectByType<Ball>();

        // Pick an active PaddleEnhanced (by name to avoid hard compile dependency)
        paddleEnhanced = null;
        var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < behaviours.Length; i++)
        {
            var b = behaviours[i];
            if (b != null && b.isActiveAndEnabled && b.GetType().Name == "PaddleEnhanced")
            {
                paddleEnhanced = b;
                break;
            }
        }

        bricks = FindObjectsByType<Brick>(FindObjectsSortMode.None);
    }

    private void LoadLevel(int level)
    {
        this.level = level;

        if (level > NUM_LEVELS)
        {
            // Start over again at level 1 once you have beaten all the levels
            // You can also load a "Win" scene instead
            LoadLevel(1);
            return;
        }

        SceneManager.sceneLoaded += OnLevelLoaded;
        SceneManager.LoadScene($"Level{level}");
    }

    private void OnLevelLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnLevelLoaded;
        FindSceneReferences();
        ApplySettings();
    }

    // Event handler for ball misses
    private void HandleBallMissed()
    {
        lives--;

        // Fire event so other systems (UI, audio, etc.) can react
        OnLivesChanged?.Invoke(lives);

        if (lives > 0) {
            ResetLevel();
        } else {
            GameOver();
        }
    }

    private void ResetLevel()
    {
        // Ensure references are valid (can be lost on scene reloads or domain refreshes)
        if (paddleEnhanced == null || ball == null)
        {
            // Try a silent rebind first to avoid noisy warnings during normal play
            FindSceneReferences();
            ApplySettings();

            // If still missing after rebind, surface a clear error
            if (paddleEnhanced == null || ball == null)
            {
                Debug.LogError("GameManager: Missing scene references after rebind attempt. Check that a Ball and a PaddleEnhanced exist and are active in the scene.");
            }
        }

        ResetActivePaddle();

        if (ball != null)
        {
            ball.ResetBall();
        }
        else
        {
            Debug.LogError("GameManager: Ball reference is missing; cannot reset ball.");
        }
    }

    private void ApplySettings()
    {
        if (settings == null)
        {
            return;
        }

        // Apply to Ball
        if (ball == null) ball = FindFirstObjectByType<Ball>();
        if (ball != null)
        {
            ball.speed = settings.ballSpeed;
        }

        // Apply to PaddleEnhanced via reflection of known fields
        if (paddleEnhanced == null)
        {
            var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < behaviours.Length; i++)
            {
                var b = behaviours[i];
                if (b != null && b.isActiveAndEnabled && b.GetType().Name == "PaddleEnhanced")
                {
                    paddleEnhanced = b; break;
                }
            }
        }

        if (paddleEnhanced != null)
        {
            var t = paddleEnhanced.GetType();
            var moveSpeedField = t.GetField("moveSpeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var maxAngleField = t.GetField("maxBounceAngle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (moveSpeedField != null) moveSpeedField.SetValue(paddleEnhanced, settings.paddleMoveSpeed);
            if (maxAngleField != null) maxAngleField.SetValue(paddleEnhanced, settings.paddleMaxBounceAngle);
        }

        // Optionally apply to LevelBuilder instances
        if (settings.levelBuilder != null && settings.levelBuilder.applyToLevelBuilders)
        {
            var builders = FindObjectsByType<LevelBuilder>(FindObjectsSortMode.None);
            for (int i = 0; i < builders.Length; i++)
            {
                var lb = builders[i];
                if (lb == null) continue;

                // Use reflection to set serialized private fields
                var lbType = typeof(LevelBuilder);

                void Set<T>(string name, T value)
                {
                    var f = lbType.GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (f != null && f.FieldType == typeof(T)) f.SetValue(lb, value);
                }

                Set("useConfigPlacement", settings.levelBuilder.useConfigPlacement);
                Set("useSpacingFromConfig", settings.levelBuilder.useSpacingFromConfig);
                Set("autoCellSizeFromPrefab", settings.levelBuilder.autoCellSizeFromPrefab);
                Set("startPosition", settings.levelBuilder.startPosition);
                Set("cellWidth", settings.levelBuilder.cellWidth);
                Set("cellHeight", settings.levelBuilder.cellHeight);
                Set("spacing", settings.levelBuilder.spacing);
                Set("groupByRows", settings.levelBuilder.groupByRows);
                Set("anchorMode", settings.levelBuilder.anchorMode);
                Set("verticalGapAbovePaddle", settings.levelBuilder.verticalGapAbovePaddle);
                Set("clampToCameraViewport", settings.levelBuilder.clampToCameraViewport);
                Set("cameraTopMargin", settings.levelBuilder.cameraTopMargin);
                Set("cameraSideMargin", settings.levelBuilder.cameraSideMargin);
                Set("debugPlacement", settings.levelBuilder.debugPlacement);
            }
        }
    }

    private void ResetActivePaddle()
    {
        if (paddleEnhanced != null && paddleEnhanced.isActiveAndEnabled)
        {
            var rb = paddleEnhanced.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            var t = paddleEnhanced.transform;
            t.position = new Vector3(0f, t.position.y, t.position.z);
            t.rotation = Quaternion.identity;
            return;
        }

        Debug.LogError("GameManager: No active PaddleEnhanced found.");
    }

    private void GameOver()
    {
        // Fire event so other systems can react (show game over screen, etc.)
        OnGameOver?.Invoke();

        // Start a new game immediately
        // You can also load a "GameOver" scene instead
        NewGame();
    }

    private void NewGame()
    {
        score = 0;
        lives = 3;

        // Fire events to notify other systems of the reset
        OnScoreChanged?.Invoke(score);
        OnLivesChanged?.Invoke(lives);

        LoadLevel(1);
    }

    // Event handler for brick hits
    private void HandleBrickHit(Brick brick, int pointsEarned)
    {
        Debug.Log($"[GameManager] HandleBrickHit called: brick={brick.name}, points={pointsEarned}");
        
        score += pointsEarned;

        // Fire event so other systems (UI, audio, achievements) can react
        OnScoreChanged?.Invoke(score);

        if (Cleared()) {
            Debug.Log($"[GameManager] Level cleared! Advancing to level {level + 1}");
            OnLevelCompleted?.Invoke(level);
            LoadLevel(level + 1);
        }
    }

    private bool Cleared()
    {
        if (bricks == null || bricks.Length == 0)
        {
            // Attempt to (re)bind bricks array if missing
            bricks = FindObjectsByType<Brick>(FindObjectsSortMode.None);
        }

        int breakableBricksRemaining = 0;
        for (int i = 0; i < bricks.Length; i++)
        {
            if (bricks[i].gameObject.activeInHierarchy && !bricks[i].unbreakable) {
                breakableBricksRemaining++;
            }
        }

        Debug.Log($"[GameManager] Cleared check: {breakableBricksRemaining} breakable bricks remaining out of {bricks.Length} total. Cleared={breakableBricksRemaining == 0}");

        return breakableBricksRemaining == 0;
    }
}
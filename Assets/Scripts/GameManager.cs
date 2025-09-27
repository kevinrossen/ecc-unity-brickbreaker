using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private const int NUM_LEVELS = 2;

    [Header("Central Settings")]
    [SerializeField] private GameSettings settings;

    private Ball ball;
    private Paddle paddle;
    // Optional alternate paddle implementation (identified by type name at runtime)
    private MonoBehaviour paddleEnhanced;
    private Brick[] bricks;

    public int level { get; private set; } = 1;
    public int score { get; private set; } = 0;
    public int lives { get; private set; } = 3;

    private void Awake()
    {
        if (Instance != null) {
            DestroyImmediate(gameObject);
        } else {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            FindSceneReferences();
            ApplySettings();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) {
            Instance = null;
        }
    }

    private void FindSceneReferences()
    {
        ball = FindFirstObjectByType<Ball>();

        // Pick the active standard Paddle if present
        var paddles = FindObjectsByType<Paddle>(FindObjectsSortMode.None);
        paddle = System.Array.Find(paddles, p => p != null && p.isActiveAndEnabled);

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

    public void OnBallMiss()
    {
        lives--;

        if (lives > 0) {
            ResetLevel();
        } else {
            GameOver();
        }
    }

    private void ResetLevel()
    {
        // Ensure references are valid (can be lost on scene reloads or domain refreshes)
        if ((paddle == null && paddleEnhanced == null) || ball == null)
        {
            // Try a silent rebind first to avoid noisy warnings during normal play
            FindSceneReferences();
            ApplySettings();

            // If still missing after rebind, surface a clear error
            if ((paddle == null && paddleEnhanced == null) || ball == null)
            {
                Debug.LogError("GameManager: Missing scene references after rebind attempt. Check that a Ball and a Paddle (or PaddleEnhanced) exist and are active in the scene.");
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

        // Apply to standard Paddle
        if (paddle == null)
        {
            var paddles = FindObjectsByType<Paddle>(FindObjectsSortMode.None);
            paddle = System.Array.Find(paddles, p => p != null && p.isActiveAndEnabled);
        }
        if (paddle != null)
        {
            paddle.speed = settings.paddleMoveSpeed;
            paddle.maxBounceAngle = settings.paddleMaxBounceAngle;
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
        if (paddle != null && paddle.isActiveAndEnabled)
        {
            paddle.ResetPaddle();
            return;
        }

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

        Debug.LogError("GameManager: No active paddle found (neither Paddle nor PaddleEnhanced).");
    }

    private void GameOver()
    {
        // Start a new game immediately
        // You can also load a "GameOver" scene instead
        NewGame();
    }

    private void NewGame()
    {
        score = 0;
        lives = 3;

        LoadLevel(1);
    }

public void OnBrickHit(Brick brick, int pointsEarned = -1)
    {
        // If no points specified, use the brick's points value
        if (pointsEarned == -1)
        {
            pointsEarned = brick.points;
        }
        
        score += pointsEarned;

        if (Cleared()) {
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

        for (int i = 0; i < bricks.Length; i++)
        {
            if (bricks[i].gameObject.activeInHierarchy && !bricks[i].unbreakable) {
                return false;
            }
        }

        return true;
    }

}

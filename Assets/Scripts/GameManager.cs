using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private const int NUM_LEVELS = 2;

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
            Debug.LogWarning("GameManager: Lost scene references. Rebinding...");
            FindSceneReferences();
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

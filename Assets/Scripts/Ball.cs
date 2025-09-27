using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    private Rigidbody2D rb;
    public float speed = 10f;

    [Header("Spawning")]
    [Tooltip("Optional. If set, the ball will spawn at this Transform's position.")]
    [SerializeField] private Transform spawnPoint;
    [Tooltip("Fallback spawn position if no spawn point is assigned. Defaults to the ball's initial position in the scene.")]
    [SerializeField] private Vector2 spawnPosition = Vector2.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // If no explicit spawn point is assigned, remember the initial scene position as the spawn location
        if (spawnPoint == null)
        {
            spawnPosition = transform.position;
        }
    }

    private void Start()
    {
        // If a central settings asset is present, allow it to drive speed
        if (GameManager.Instance != null)
        {
            var gm = GameManager.Instance;
            var settingsField = typeof(GameManager).GetField("settings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (settingsField != null)
            {
                var settings = settingsField.GetValue(gm) as GameSettings;
                if (settings != null)
                {
                    speed = settings.ballSpeed;
                }
            }
        }
        ResetBall();
    }

    public void ResetBall()
    {
        rb.linearVelocity = Vector2.zero;
        // Use the assigned spawn point if available; otherwise, use the remembered initial position
        transform.position = spawnPoint != null ? (Vector3)spawnPoint.position : (Vector3)spawnPosition;

        CancelInvoke();
        Invoke(nameof(SetRandomTrajectory), 1f);
    }

    private void SetRandomTrajectory()
    {
        Vector2 force = new Vector2(Random.Range(-1f, 1f), -1f);
        rb.AddForce(force.normalized * speed, ForceMode2D.Impulse);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = rb.linearVelocity.normalized * speed;
    }

}

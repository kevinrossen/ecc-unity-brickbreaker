using UnityEngine;

// Enhanced Paddle script demonstrating Unity attributes, input handling, and physics.
// Shows component-based architecture for beginner-intermediate game development.

[RequireComponent(typeof(Rigidbody2D))]
public class PaddleEnhanced : MonoBehaviour
{
    // UNITY ATTRIBUTES: Special markers that enhance the Inspector experience
    // [Header] - Creates section titles | [Range] - Slider controls | [Tooltip] - Hover help
    // [SerializeField] - Shows private vars in Inspector | [Space] - Adds spacing
    // [ColorUsage] - HDR color picker | [TextArea] - Multi-line text box
    
    [Header("Movement Settings")]
    [Range(20f, 500f)] [Tooltip("How fast the paddle moves left and right")]
    [SerializeField] private float moveSpeed = 50f;
    
    [Range(0.5f, 2f)] [Tooltip("Multiplier for speed boost powerup")]
    [SerializeField] private float speedBoostMultiplier = 1.5f;
    
    // Boundary clamping removed; rely on physical wall colliders instead
    
    [Space(10)]
    [Header("Bounce Settings")]
    [Tooltip("Maximum angle in degrees the ball can be deflected when hitting paddle edges")]
    [Range(0f, 85f)] [SerializeField] private float maxBounceAngle = 75f;
    
    [Space(10)]
    [Header("Paddle Size")]
    [Range(0.5f, 3f)] [SerializeField] private float paddleWidthMultiplier = 1f;
    [Range(0.5f, 2f)] [SerializeField] private float paddleHeightMultiplier = 1f;
    
    [Space(10)]
    [Header("Visual Effects")]
    [ColorUsage(true, true)] [SerializeField] private Color normalColor = Color.white;
    [ColorUsage(true, true)] [SerializeField] private Color powerupColor = Color.cyan;
    
    [Space(10)]
    [Header("Debug Settings")]
    [SerializeField] private bool showDebugInfo = false;
    [TextArea(3, 5)] [SerializeField] private string designNotes = "Add any design notes here for level designers...";
    
    // PRIVATE VARIABLES: Not visible in Inspector, used for runtime logic
    private Rigidbody2D rb;        // Physics component for movement
    private float currentSpeed;    // Tracks current movement speed (can change with powerups)
    
    // UNITY LIFECYCLE METHODS
    
    // Called once at startup - initialize references and default values
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();  // Get physics component
        // Optionally pull centralized settings from GameManager if assigned
        if (GameManager.Instance != null)
        {
            var gm = GameManager.Instance;
            var settingsField = typeof(GameManager).GetField("settings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (settingsField != null)
                {
                    var settings = settingsField.GetValue(gm) as GameSettings;
                    if (settings != null)
                    {
                        moveSpeed = settings.paddleMoveSpeed;
                        maxBounceAngle = settings.paddleMaxBounceAngle;
                    }
                }
        }
        currentSpeed = moveSpeed;           // Set initial speed
    }
    
    // Called every frame - handle continuous input and updates
    private void Update()
    {
        HandleMovement();  // Delegate to separate method for organization
    }
    
    // MOVEMENT LOGIC: Handles paddle input, physics, and boundaries
    private void HandleMovement()
    {
        // Get horizontal input (-1 to 1) from arrow keys or A/D
        float horizontalInput = Input.GetAxis("Horizontal");
        
        // Calculate movement vector (horizontal only)
        Vector2 movement = new Vector2(horizontalInput * currentSpeed, 0);
        
        // Apply movement with frame-rate independence
        Vector2 newPosition = rb.position + movement * Time.deltaTime;
        
        // Move using physics (respects collisions)
        rb.MovePosition(newPosition);
    }

    // COLLISION LOGIC: Apply angle-based bounce similar to original Paddle
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Ball"))
        {
            return;
        }

        Rigidbody2D ball = collision.rigidbody;
        if (ball == null)
        {
            return;
        }

        Collider2D paddleCollider = collision.otherCollider;

        // Current direction and speed of the ball
        Vector2 ballDirection = ball.linearVelocity.normalized;
        float ballSpeed = ball.linearVelocity.magnitude;

        // Determine how far from the center of the paddle the ball hit
        Vector2 contactOffset = paddleCollider.bounds.center - ball.transform.position;

        // Map horizontal contact offset to a bounce angle within [-maxBounceAngle, maxBounceAngle]
        float normalizedOffset = contactOffset.x / paddleCollider.bounds.size.x; // roughly -0.5..0.5
        float bounceAngle = normalizedOffset * maxBounceAngle;

        // Rotate ball direction by the bounce angle around Z axis
        ballDirection = Quaternion.AngleAxis(bounceAngle, Vector3.forward) * ballDirection;

        // Reapply speed with new direction
        ball.linearVelocity = ballDirection * ballSpeed;
    }
}
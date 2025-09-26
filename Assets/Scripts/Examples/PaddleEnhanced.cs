using UnityEngine;

// Enhanced Paddle script demonstrating Unity attributes, input handling, and physics.
// Shows component-based architecture for beginner-intermediate game development.

public class PaddleEnhanced : MonoBehaviour
{
    // UNITY ATTRIBUTES: Special markers that enhance the Inspector experience
    // [Header] - Creates section titles | [Range] - Slider controls | [Tooltip] - Hover help
    // [SerializeField] - Shows private vars in Inspector | [Space] - Adds spacing
    // [ColorUsage] - HDR color picker | [TextArea] - Multi-line text box
    
    [Header("Movement Settings")]
    [Range(20f, 100f)] [Tooltip("How fast the paddle moves left and right")]
    [SerializeField] private float moveSpeed = 10f;
    
    [Range(0.5f, 2f)] [Tooltip("Multiplier for speed boost powerup")]
    [SerializeField] private float speedBoostMultiplier = 1.5f;
    
    [Space(10)]
    [Header("Boundary Settings")]
    [Tooltip("Maximum distance paddle can move from center")]
    [SerializeField] private float maxHorizontalPosition = 8f;
    
    [SerializeField] private bool useScreenBounds = true;
    
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
        
        // Clamp to screen boundaries
        newPosition.x = Mathf.Clamp(newPosition.x, -maxHorizontalPosition, maxHorizontalPosition);
        
        // Move using physics (respects collisions)
        rb.MovePosition(newPosition);
    }
}
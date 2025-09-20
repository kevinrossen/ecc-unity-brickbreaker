using UnityEngine;

// Enhanced Paddle script with Unity Attributes for Week 4 teaching
public class PaddleEnhanced : MonoBehaviour
{
    [Header("Movement Settings")]
    [Range(5f, 20f)]
    [Tooltip("How fast the paddle moves left and right")]
    [SerializeField] private float moveSpeed = 10f;
    
    [Range(0.5f, 2f)]
    [Tooltip("Multiplier for speed boost powerup")]
    [SerializeField] private float speedBoostMultiplier = 1.5f;
    
    [Space(10)]
    [Header("Boundary Settings")]
    [Tooltip("Maximum distance paddle can move from center")]
    [SerializeField] private float maxHorizontalPosition = 8f;
    
    [SerializeField] private bool useScreenBounds = true;
    
    [Space(10)]
    [Header("Paddle Size")]
    [Range(0.5f, 3f)]
    [SerializeField] private float paddleWidthMultiplier = 1f;
    
    [Range(0.5f, 2f)]
    [SerializeField] private float paddleHeightMultiplier = 1f;
    
    [Space(10)]
    [Header("Visual Effects")]
    [ColorUsage(true, true)]
    [SerializeField] private Color normalColor = Color.white;
    
    [ColorUsage(true, true)]
    [SerializeField] private Color powerupColor = Color.cyan;
    
    [Space(10)]
    [Header("Debug Settings")]
    [SerializeField] private bool showDebugInfo = false;
    
    [TextArea(3, 5)]
    [SerializeField] private string designNotes = "Add any design notes here for level designers...";
    
    private Rigidbody2D rb;
    private float currentSpeed;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = moveSpeed;
    }
    
    private void Update()
    {
        HandleMovement();
    }
    
    private void HandleMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        Vector2 movement = new Vector2(horizontalInput * currentSpeed, 0);
        
        Vector2 newPosition = rb.position + movement * Time.deltaTime;
        newPosition.x = Mathf.Clamp(newPosition.x, -maxHorizontalPosition, maxHorizontalPosition);
        
        rb.MovePosition(newPosition);
    }
}
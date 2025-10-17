using UnityEngine;
// NEW INPUT SYSTEM: Using the new Input System package for modern input handling
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

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
    
    [Space(10)]
    [Header("NEW INPUT SYSTEM: Button Speed Controls")]
    [Range(0.5f, 1f)] [Tooltip("Multiplier when slow button (A/Cross) is pressed")]
    [SerializeField] private float slowSpeedMultiplier = 0.5f;
    
    [Range(1f, 2.5f)] [Tooltip("Multiplier when fast button (Y/Triangle) is pressed")]
    [SerializeField] private float fastSpeedMultiplier = 2f;
    
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
        // OLD INPUT SYSTEM (deprecated): Get horizontal input from Input Manager
        // float horizontalInput = Input.GetAxis("Horizontal");
        
        // NEW INPUT SYSTEM: Using Keyboard and Gamepad from UnityEngine.InputSystem
        float horizontalInput = 0f;
        
        // DEBUG: Log gamepad connection status (remove after testing)
        if (showDebugInfo)
        {
            DebugGamepadStatus();
        }
        
        // NEW INPUT SYSTEM: Check for gamepad/controller input (left stick)
        // Gamepad.current is null if no controller is connected
        if (Gamepad.current != null)
        {
            // ReadValue() returns a float between -1 and 1
            horizontalInput = Gamepad.current.leftStick.x.ReadValue();
            // Debug.Log($"Gamepad Input: {horizontalInput}");
        }
        // NEW: Check for Joystick input (generic USB gamepads that aren't recognized as standard Gamepad)
        else if (Joystick.current != null)
        {
            // Joystick.current.stick gives us the left analog stick position
            horizontalInput = Joystick.current.stick.x.ReadValue();
            // Debug.Log($"Joystick Input: {horizontalInput}");
        }
        
        // NEW INPUT SYSTEM: Check for keyboard input if no gamepad input detected
        if (horizontalInput == 0f && Keyboard.current != null)
        {
            // NEW INPUT SYSTEM: isPressed returns true/false for key state
            bool isLeftPressed = Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed;
            bool isRightPressed = Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed;
            
            if (isLeftPressed) horizontalInput = -1f;
            if (isRightPressed) horizontalInput = 1f;
        }
        
        // NEW INPUT SYSTEM: Check button presses for speed modifiers
        // Start with base speed
        float speedMultiplier = 1f;
        
        // Check Gamepad buttons (PS5, Xbox controllers)
        if (Gamepad.current != null)
        {
            // NEW INPUT SYSTEM: For PS5 - X button (aButton/Blue) for FAST mode
            if (Gamepad.current.aButton.isPressed)
            {
                speedMultiplier = fastSpeedMultiplier;
                if (showDebugInfo) Debug.Log("🚀 PADDLE FAST MODE (X Button / aButton)");
            }
            // NEW INPUT SYSTEM: For PS5 - Circle button (bButton/Red) for SLOW mode
            else if (Gamepad.current.bButton.isPressed)
            {
                speedMultiplier = slowSpeedMultiplier;
                if (showDebugInfo) Debug.Log("🐢 PADDLE SLOW MODE (Circle Button / bButton)");
            }
        }
        // Check Joystick buttons (generic USB gamepad)
        // Generic joysticks don't have named buttons like Gamepad, so we use allControls
        else if (Joystick.current != null)
        {
            // NEW INPUT SYSTEM: Access buttons through allControls
            // DEBUG: Check which button is actually being pressed to find correct mapping
            if (showDebugInfo && Joystick.current.allControls.Count > 0)
            {
                // Log all button presses to help identify button mapping
                for (int i = 0; i < Joystick.current.allControls.Count; i++)
                {
                    if (Joystick.current.allControls[i] is ButtonControl button && button.isPressed)
                    {
                        Debug.Log($"📍 Joystick Button Pressed: Index {i} - {button.displayName}");
                    }
                }
            }
            
            // CORRECTED BUTTON MAPPING for this generic USB gamepad:
            // Index 2 = A button, Index 3 = B button, Index 1 = X (trigger), Index 4 = Y button
            if (Joystick.current.allControls.Count > 4)
            {
                // Button B (index 3) for FAST mode - changed from index 4
                if (Joystick.current.allControls[3] is ButtonControl bButton && bButton.isPressed)
                {
                    speedMultiplier = fastSpeedMultiplier;
                    if (showDebugInfo) Debug.Log("🚀 FAST MODE (B Button - Joystick Index 3)");
                }
                // Button A (index 2) for SLOW mode - changed from index 0
                else if (Joystick.current.allControls[2] is ButtonControl aButton && aButton.isPressed)
                {
                    speedMultiplier = slowSpeedMultiplier;
                    if (showDebugInfo) Debug.Log("🐢 SLOW MODE (A Button - Joystick Index 2)");
                }
            }
        }
        
        // Calculate movement vector with speed multiplier applied
        Vector2 movement = new Vector2(horizontalInput * currentSpeed * speedMultiplier, 0);
        
        // Apply movement with frame-rate independence
        Vector2 newPosition = rb.position + movement * Time.deltaTime;
        
        // Move using physics (respects collisions)
        rb.MovePosition(newPosition);
    }

    // DEBUG METHOD: Test gamepad connection and input values
    private void DebugGamepadStatus()
    {
        // Check if standard Gamepad is connected
        if (Gamepad.current != null)
        {
            // Gamepad found - log detailed info
            Debug.Log($"✅ GAMEPAD CONNECTED: {Gamepad.current.displayName}");
            Debug.Log($"   Left Stick X: {Gamepad.current.leftStick.x.ReadValue()}");
            Debug.Log($"   Left Stick Y: {Gamepad.current.leftStick.y.ReadValue()}");
            return;
        }

        // NEW: Check for Joystick (generic USB gamepads)
        if (Joystick.current != null)
        {
            Debug.Log($"✅ JOYSTICK DETECTED: {Joystick.current.displayName}");
            Debug.Log($"   Stick X: {Joystick.current.stick.x.ReadValue()}");
            Debug.Log($"   Stick Y: {Joystick.current.stick.y.ReadValue()}");
            // FIXED: Joystick doesn't have a buttons property; use allControls instead
            Debug.Log($"   Total Controls: {Joystick.current.allControls.Count}");
            return;
        }

        // No gamepad or joystick found
        Debug.LogWarning("⚠️ NO GAMEPAD/JOYSTICK DETECTED! Check device connection.");
        
        // NEW: List ALL connected input devices to diagnose the issue
        Debug.Log("📋 CONNECTED INPUT DEVICES:");
        var allDevices = InputSystem.devices;
        foreach (var device in allDevices)
        {
            Debug.Log($"   - {device.displayName} (Type: {device.GetType().Name})");
        }
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
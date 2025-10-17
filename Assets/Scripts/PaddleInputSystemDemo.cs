using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// EDUCATIONAL DEMO: New Input System vs. Old Input System
/// 
/// This script demonstrates how to use Unity's NEW INPUT SYSTEM package
/// which is the modern, recommended approach for handling player input.
/// It replaces the old Input.GetKey() and Input.GetAxis() methods.
/// </summary>

[RequireComponent(typeof(Rigidbody2D))]
public class PaddleInputSystemDemo : MonoBehaviour
{
    [Header("Movement Settings")]
    [Range(20f, 500f)]
    [SerializeField] private float moveSpeed = 50f;

    // NEW INPUT SYSTEM: Reference to the automatically generated input actions
    // You can also create a custom InputActions asset in the Project folder
    private Rigidbody2D rb;
    private float currentSpeed;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = moveSpeed;
    }

    private void Update()
    {
        // ==============================================================================
        // APPROACH 1: Using Gamepad (Controller) Input - NEW INPUT SYSTEM
        // ==============================================================================
        
        // NEW INPUT SYSTEM: Access gamepad/controller input
        // This is the modern way to read controller input
        var gamepad = Gamepad.current;
        
        if (gamepad != null)  // Check if a gamepad is connected
        {
            // Read the left stick's horizontal axis (-1 to 1)
            float gamepadHorizontalInput = gamepad.leftStick.x.ReadValue();
            
            // You can also read right stick, buttons, triggers, etc.
            // float gamepadVerticalInput = gamepad.leftStick.y.ReadValue();
            // bool isAButtonPressed = gamepad.aButton.isPressed;
            // float rightTriggerValue = gamepad.rightTrigger.ReadValue();
            
            Debug.Log($"Gamepad Input: {gamepadHorizontalInput}");
        }

        // ==============================================================================
        // APPROACH 2: Using Keyboard Input - NEW INPUT SYSTEM
        // ==============================================================================
        
        // NEW INPUT SYSTEM: Access keyboard input
        var keyboard = Keyboard.current;
        
        if (keyboard != null)  // Check if keyboard is available
        {
            // Method A: Check if a key is pressed (on/off)
            bool isAPressed = keyboard.aKey.isPressed;
            bool isDPressed = keyboard.dKey.isPressed;
            
            // Method B: Read arrow keys
            bool isLeftPressed = keyboard.leftArrowKey.isPressed;
            bool isRightPressed = keyboard.rightArrowKey.isPressed;
            
            // Calculate horizontal input from keyboard
            // float keyboardInput = 0f;
            // if (isAPressed || isLeftPressed) keyboardInput = -1f;
            // if (isDPressed || isRightPressed) keyboardInput = 1f;
            
            // Debug.Log($"Keyboard Input: {keyboardInput}");
        }

        // ==============================================================================
        // APPROACH 3: Using InputActions (RECOMMENDED FOR PRODUCTION)
        // ==============================================================================
        // This is the most flexible and professional approach!
        // You create an InputActions asset that maps controls to logical actions
        
        // Example pseudocode (requires InputActions asset setup):
        // float horizontalInput = playerInputActions.Player.Move.ReadValue<float>();
        // This single line handles ALL input devices seamlessly!

        // ==============================================================================
        // APPROACH 4: OLD INPUT SYSTEM (Deprecated but still works)
        // ==============================================================================
        
        // OLD SYSTEM: These still work but are not recommended for new projects
        // float oldInput = Input.GetAxis("Horizontal");  // Works for keyboard + gamepad
        // bool keyDown = Input.GetKeyDown(KeyCode.Space);
        
        HandleMovement();
    }

    private void HandleMovement()
    {
        // For now, use a combination approach for this demo
        float horizontalInput = 0f;

        // NEW INPUT SYSTEM: Check gamepad
        if (Gamepad.current != null)
        {
            horizontalInput = Gamepad.current.leftStick.x.ReadValue();
        }

        // NEW INPUT SYSTEM: Check keyboard (only if no gamepad input)
        if (horizontalInput == 0f && Keyboard.current != null)
        {
            bool isLeft = Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed;
            bool isRight = Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed;
            
            if (isLeft) horizontalInput = -1f;
            if (isRight) horizontalInput = 1f;
        }

        // Apply movement
        Vector2 movement = new Vector2(horizontalInput * currentSpeed, 0);
        Vector2 newPosition = rb.position + movement * Time.deltaTime;
        rb.MovePosition(newPosition);
    }

    // ==============================================================================
    // BONUS: How to read different input device types
    // ==============================================================================
    
    private void ExampleInputDeviceHandling()
    {
        // Gamepad/Controller
        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            // Analog sticks (return -1 to 1)
            float leftStickX = gamepad.leftStick.x.ReadValue();
            float leftStickY = gamepad.leftStick.y.ReadValue();
            
            // Buttons (return true/false)
            bool aButtonPressed = gamepad.aButton.isPressed;
            bool bButtonPressed = gamepad.bButton.isPressed;
            bool xButtonPressed = gamepad.xButton.isPressed;
            bool yButtonPressed = gamepad.yButton.isPressed;
            
            // Triggers/Shoulders (return 0 to 1)
            float leftTrigger = gamepad.leftTrigger.ReadValue();
            float rightTrigger = gamepad.rightTrigger.ReadValue();
        }

        // Keyboard
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            bool spacePressed = keyboard.spaceKey.isPressed;
            bool enterPressed = keyboard.enterKey.isPressed;
            bool escapePressed = keyboard.escapeKey.isPressed;
            
            // You can access any key by name
            bool wPressed = keyboard.wKey.isPressed;
            bool aPressed = keyboard.aKey.isPressed;
            bool sPressed = keyboard.sKey.isPressed;
            bool dPressed = keyboard.dKey.isPressed;
        }

        // Mouse
        var mouse = Mouse.current;
        if (mouse != null)
        {
            Vector2 mousePosition = mouse.position.ReadValue();
            bool leftClickPressed = mouse.leftButton.isPressed;
            bool rightClickPressed = mouse.rightButton.isPressed;
            float mouseScrollWheel = mouse.scroll.y.ReadValue();
        }

        // Touchscreen (Mobile)
        var touchscreen = Touchscreen.current;
        if (touchscreen != null)
        {
            int touchCount = touchscreen.touches.Count;
            if (touchCount > 0)
            {
                var touch = touchscreen.touches[0];
                Vector2 touchPosition = touch.position.ReadValue();
                float touchPressure = touch.pressure.ReadValue();
            }
        }
    }

    // ==============================================================================
    // COMPARISON: Old vs New Input System
    // ==============================================================================
    
    /*
     * OLD INPUT SYSTEM (Still works, but deprecated):
     * ================================================
     * float input = Input.GetAxis("Horizontal");
     * bool jump = Input.GetKeyDown(KeyCode.Space);
     * Vector2 mousePos = Input.mousePosition;
     * 
     * Pros:
     *   - Simple and quick for basic input
     *   - Works immediately with default Input Manager settings
     * 
     * Cons:
     *   - Limited device support
     *   - No mobile touch support
     *   - Hard to rebind controls
     *   - Not designed for modern hardware
     * 
     * 
     * NEW INPUT SYSTEM (Recommended):
     * ================================
     * float input = Gamepad.current?.leftStick.x.ReadValue() ?? 0f;
     * bool jump = Keyboard.current?.spaceKey.isPressed ?? false;
     * Vector2 mousePos = Mouse.current?.position.ReadValue() ?? Vector2.zero;
     * 
     * Pros:
     *   - Full support for all device types (gamepad, keyboard, mouse, touch)
     *   - Easy input rebinding at runtime
     *   - InputActions for clean, organized input handling
     *   - Better performance
     *   - Cross-platform ready
     *   - Modern Unity standard
     * 
     * Cons:
     *   - Slightly more verbose
     *   - Requires InputSystem package
     *   - Learning curve for InputActions workflow
     */
}

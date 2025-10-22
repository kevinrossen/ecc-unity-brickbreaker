using UnityEngine;
// NEW INPUT SYSTEM: Using the new Input System package for modern input handling
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

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

    [Space(10)]
    [Header("NEW INPUT SYSTEM: Button Speed Controls")]
    [Range(0.5f, 1f)] [Tooltip("Multiplier when slow button (X) is pressed")]
    [SerializeField] private float slowSpeedMultiplier = 0.5f;
    
    [Range(1f, 2.5f)] [Tooltip("Multiplier when fast button (Y) is pressed")]
    [SerializeField] private float fastSpeedMultiplier = 2f;
    
    [SerializeField] private bool showDebugInfo = false;

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
        if (GameManager_DecoupledEvent.Instance != null)
        {
            var gm = GameManager_DecoupledEvent.Instance;
            var settingsField = typeof(GameManager_DecoupledEvent).GetField("settings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
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
        // NEW INPUT SYSTEM: Check button presses for speed modifiers
        float speedMultiplier = 1f;
        
        // Check Gamepad buttons (PS5, Xbox controllers)
        if (Gamepad.current != null)
        {
            // NEW INPUT SYSTEM: For PS5 - Square button (xButton/Pink) for FAST mode
            if (Gamepad.current.xButton.isPressed)
            {
                speedMultiplier = fastSpeedMultiplier;
                if (showDebugInfo) Debug.Log("🚀 BALL FAST MODE (Square Button / xButton)");
            }
            // NEW INPUT SYSTEM: For PS5 - Triangle button (yButton/Green) for SLOW mode
            else if (Gamepad.current.yButton.isPressed)
            {
                speedMultiplier = slowSpeedMultiplier;
                if (showDebugInfo) Debug.Log("🐢 BALL SLOW MODE (Triangle Button / yButton)");
            }
        }
        // Check Joystick buttons (generic USB gamepad)
        // Based on your gamepad mapping: Index 1 = X, Index 4 = Y
        else if (Joystick.current != null)
        {
            // DEBUG: Log all button presses
            if (showDebugInfo && Joystick.current.allControls.Count > 0)
            {
                for (int i = 0; i < Joystick.current.allControls.Count; i++)
                {
                    if (Joystick.current.allControls[i] is ButtonControl button && button.isPressed)
                    {
                        Debug.Log($"📍 Joystick Button Pressed: Index {i}");
                    }
                }
            }
            
            // CORRECTED BUTTON MAPPING for generic USB gamepad:
            // Index 1 = X (trigger), Index 4 = Y button
            if (Joystick.current.allControls.Count > 4)
            {
                // Button Y (index 4) for FAST mode
                if (Joystick.current.allControls[4] is ButtonControl yButton && yButton.isPressed)
                {
                    speedMultiplier = fastSpeedMultiplier;
                    if (showDebugInfo) Debug.Log("🚀 BALL FAST MODE (Y Button - Joystick Index 4)");
                }
                // Button X (index 1) for SLOW mode
                else if (Joystick.current.allControls[1] is ButtonControl xButton && xButton.isPressed)
                {
                    speedMultiplier = slowSpeedMultiplier;
                    if (showDebugInfo) Debug.Log("🐢 BALL SLOW MODE (X Button - Joystick Index 1)");
                }
            }
        }
        
        // Apply speed with multiplier
        rb.linearVelocity = rb.linearVelocity.normalized * speed * speedMultiplier;
    }

}

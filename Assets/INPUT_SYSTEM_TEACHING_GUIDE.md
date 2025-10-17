# New Input System - Teaching Guide for Homeschool Co-op

## Overview
This guide demonstrates how to use **Unity's New Input System** (UnityEngine.InputSystem) - the modern, recommended approach for handling player input in games.

## Key Concepts for Students

### Why New Input System?
The old `Input.GetKey()` and `Input.GetAxis()` still work, but they're deprecated. The New Input System offers:
- ✅ Support for more device types (gamepad, keyboard, mouse, touch, VR controllers)
- ✅ Easy rebinding controls at runtime
- ✅ Better performance
- ✅ Cross-platform ready
- ✅ Modern Unity standard

---

## Approach 1: Direct Device Access (Simplest)

This is the most direct way to read input from specific devices:

### Gamepad/Controller
```csharp
if (Gamepad.current != null)
{
    float horizontalInput = Gamepad.current.leftStick.x.ReadValue();  // -1 to 1
    bool aButton = Gamepad.current.aButton.isPressed;                 // true/false
    float trigger = Gamepad.current.rightTrigger.ReadValue();         // 0 to 1
}
```

**What students should understand:**
- `Gamepad.current` returns the currently active gamepad, or `null` if none connected
- `.ReadValue()` returns a continuous value
- `.isPressed` returns true/false for button state
- Different controls return different ranges: sticks (-1 to 1), triggers (0 to 1)

### Keyboard
```csharp
if (Keyboard.current != null)
{
    bool wPressed = Keyboard.current.wKey.isPressed;
    bool spacePressed = Keyboard.current.spaceKey.isPressed;
}
```

### Mouse
```csharp
if (Mouse.current != null)
{
    Vector2 mousePosition = Mouse.current.position.ReadValue();
    bool leftClick = Mouse.current.leftButton.isPressed;
    float scroll = Mouse.current.scroll.y.ReadValue();
}
```

### Touch (Mobile)
```csharp
if (Touchscreen.current != null)
{
    int touchCount = Touchscreen.current.touches.Count;
    if (touchCount > 0)
    {
        var touch = Touchscreen.current.touches[0];
        Vector2 touchPos = touch.position.ReadValue();
    }
}
```

---

## Approach 2: InputActions Asset (Professional/Production)

This is the recommended approach for larger projects.

### Steps:
1. Right-click in Project folder → Create → Input Actions
2. In the Input Actions asset, create:
   - **Action Map**: "Player"
   - **Action**: "Move" (Value Type: Axis1D or Axis2D)
3. Bind it to keyboard (WASD/Arrow keys) and gamepad (Left Stick)
4. Save and enable "Generate C# Class"

### Usage:
```csharp
private PlayerInputActions inputActions;

private void OnEnable()
{
    inputActions = new PlayerInputActions();
    inputActions.Enable();
}

private void OnDisable()
{
    inputActions.Disable();
}

private void Update()
{
    // This single line handles ALL input devices!
    float horizontalInput = inputActions.Player.Move.ReadValue<float>();
}
```

**Benefits:**
- Changes to input bindings don't require code changes
- Students can create custom controls
- Easier to support multiple platforms

---

## Real Game Implementation: Brick Breaker Controls

This project demonstrates practical input handling in a real game. Here's what students can learn:

### **PS5 DualSense Controller Mapping:**

#### Paddle Controls:
| Input | Button | Effect |
|-------|--------|--------|
| Left Stick | Analog | Smooth, proportional movement |
| X Button | Blue | Speed up paddle |
| Circle Button | Red | Slow down paddle |

#### Ball Controls:
| Input | Button | Effect |
|-------|--------|--------|
| Square Button | Pink | Speed up ball |
| Triangle Button | Green | Slow down ball |

**Future expansions:** D-pad is reserved for future features!

---

## Understanding Analog Input: Proportional vs Binary

A key learning point when using controllers:

### Analog Stick Input (Continuous Values)
```csharp
float horizontalInput = Gamepad.current.leftStick.x.ReadValue();
// Returns: -1.0 (fully left), -0.5 (half left), 0 (centered), 0.5 (half right), 1.0 (fully right)

// Movement is proportional:
Vector2 movement = new Vector2(horizontalInput * currentSpeed, 0);
```

**Result:** The further you push the stick, the faster the paddle moves!

This is **intentional and realistic** - it's how modern games work. Players can:
- Gently nudge the stick for slow, precise movement
- Push fully for maximum speed

### Button Input (Binary Values)
```csharp
if (Gamepad.current.xButton.isPressed)  // True or False - no in-between!
{
    speedMultiplier = fastSpeedMultiplier;
}
```

**Result:** Buttons are all-or-nothing. You're either pressing the button or not.

### Generic USB Gamepad Button Mapping
Your generic gamepad may report buttons differently:

```csharp
// Generic USB Gamepad Button Layout (Your Mapping):
// Index 0 = A button
// Index 1 = X button (trigger)
// Index 2 = A button
// Index 3 = B button  
// Index 4 = Y button

if (Joystick.current != null && Joystick.current.allControls.Count > 4)
{
    if (Joystick.current.allControls[3] is ButtonControl bButton && bButton.isPressed)
    {
        // B button pressed
    }
}
```

**Teaching point:** Generic hardware doesn't always follow standard layouts. Real-world code must be flexible!

---

## Code Examples in Your Project

### PaddleEnhanced.cs
Shows a practical example of the new Input System in action with:
- **Multiple device support**: Handles both standard Gamepads (PS5, Xbox) and generic USB Joysticks
- **Analog stick support**: Movement proportional to stick deflection (-1 to 1 range)
- **Button-based speed control**: 
  - **PS5 Controller:** X button to speed up, Circle button to slow down
  - **Generic USB Gamepad:** B button (Index 3) to speed up, A button (Index 2) to slow down
- **Keyboard fallback**: Arrow keys or A/D if no controller connected
- **Debug diagnostics**: Shows which device type is connected and current input values

**Key teaching points:**
- Analog sticks return continuous values for smooth, proportional control
- Buttons are binary (true/false) for discrete actions
- Different controllers present themselves differently (Gamepad vs Joystick)
- Using `allControls` to access generic device button inputs
- Type casting with `is ButtonControl` pattern
- Combining analog stick + button inputs for complex control schemes

### Ball.cs
Demonstrates similar input handling for a different game object:
- **PS5 Controller:** Square button to speed up, Triangle button to slow down
- **Generic USB Gamepad:** Y button (Index 4) to speed up, X button (Index 1) to slow down
- Same debug capabilities and button mapping logic as the paddle

**Teaching opportunity:** Shows how to apply the same input patterns across multiple objects in a game!

### PaddleInputSystemDemo.cs
Complete educational example showing:
- All major device types (gamepad, keyboard, mouse, touch)
- Multiple input reading methods
- Comparison between old and new systems
- Bonus methods for each device type

---

## Advanced: Handling Different Controller Types

One important lesson: **Not all controllers report themselves the same way!**

### Standard Gamepads (PS5, Xbox, Nintendo Pro)
```csharp
if (Gamepad.current != null)
{
    // These controllers have named buttons
    float yButton = Gamepad.current.yButton.isPressed;
    float aButton = Gamepad.current.aButton.isPressed;
}
```

### Generic USB Joysticks
```csharp
if (Joystick.current != null)
{
    // Generic joysticks don't have named buttons
    // Instead, access through allControls array
    // Typical mapping: 0=A, 1=B, 2=X, 3=Y
    if (Joystick.current.allControls.Count > 3)
    {
        if (Joystick.current.allControls[3] is ButtonControl yButton && yButton.isPressed)
        {
            // Y button pressed
        }
    }
}
```

**Teaching point for students:** The real world is messy! Different hardware manufacturers implement things differently. Good code must handle these variations gracefully.

---

## Speed Control Example from PaddleEnhanced.cs

The paddle now supports dynamic speed adjustment:

```csharp
// Start with base speed
float speedMultiplier = 1f;

// Y Button (Triangle on PS5) for FAST mode
if (Gamepad.current != null && Gamepad.current.yButton.isPressed)
{
    speedMultiplier = fastSpeedMultiplier;  // 2.0 by default
}
// A Button (Cross on PS5) for SLOW mode
else if (Gamepad.current != null && Gamepad.current.aButton.isPressed)
{
    speedMultiplier = slowSpeedMultiplier;  // 0.5 by default
}

// Apply the multiplier to movement
Vector2 movement = new Vector2(horizontalInput * currentSpeed * speedMultiplier, 0);
```

**Customizable values in the Inspector:**
- `slowSpeedMultiplier` - Default: 0.5 (half speed)
- `fastSpeedMultiplier` - Default: 2.0 (double speed)

---

## Comparison Chart

| Feature | Old Input System | New Input System |
|---------|------------------|------------------|
| Keyboard Support | ✅ | ✅ |
| Gamepad Support | ✅ | ✅✅ |
| Mouse Support | ✅ | ✅✅ |
| Touch Support | ❌ | ✅ |
| VR Controllers | ❌ | ✅ |
| Input Rebinding | Difficult | Easy ✅ |
| Performance | Good | Better ✅ |
| Code Simplicity | Simple | Slightly more verbose |

---

## Common Input Patterns for Students

### Example 1: Simple Movement
```csharp
float moveX = Gamepad.current?.leftStick.x.ReadValue() ?? 0f;
float moveY = Gamepad.current?.leftStick.y.ReadValue() ?? 0f;
Vector2 movement = new Vector2(moveX, moveY);
```

**Teaching point:** The `?.` operator checks for null safely. The `?? 0f` provides a default value if null.

### Example 2: Button Handling
```csharp
if (Gamepad.current?.aButton.wasReleasedThisFrame ?? false)
{
    // Jump!
}
```

**Properties:**
- `.isPressed` - True while held
- `.wasPressedThisFrame` - True once when first pressed
- `.wasReleasedThisFrame` - True once when released

### Example 3: Combining Multiple Inputs
```csharp
float input = 0f;

// Gamepad has priority
if (Gamepad.current != null)
{
    input = Gamepad.current.leftStick.x.ReadValue();
}
// Fall back to keyboard
else if (Keyboard.current != null)
{
    if (Keyboard.current.aKey.isPressed) input = -1f;
    if (Keyboard.current.dKey.isPressed) input = 1f;
}
```

---

## Discussion Questions for Your Class

1. **Why might we want to support multiple input devices?**
   - Answer: Different players prefer different controls; accessibility; cross-platform

2. **What's the difference between `.isPressed` and `.wasPressedThisFrame`?**
   - Answer: isPressed is continuous; wasPressedThisFrame is event-based (happens once per frame)

3. **When would you use InputActions vs direct device access?**
   - Answer: InputActions for flexibility and rebinding; direct access for quick prototypes

4. **How would you handle a player with no input device connected?**
   - Answer: Check for null; provide default AI behavior or pause game

---

## Resources
- [Unity Input System Documentation](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest)
- [InputSystem GitHub](https://github.com/Unity-Technologies/InputSystem)

---

## Debugging: How to Test Your Gamepad Connection

The `PaddleEnhanced.cs` script includes debug diagnostics:

1. **Select the Paddle object in the scene**
2. **Enable "Show Debug Info"** in the Inspector
3. **Run the game** and check the Console

**What to look for:**

✅ **If you see:**
```
✅ GAMEPAD CONNECTED: Xbox 360 Controller
   Left Stick X: 0.5
   Left Stick Y: 0
```
→ Your standard gamepad is recognized!

✅ **If you see:**
```
✅ JOYSTICK DETECTED: USB Gamepad
   Stick X: 0.25
   Stick Y: 0
   Total Controls: 12
```
→ Your generic USB controller is working!

⚠️ **If you see:**
```
⚠️ NO GAMEPAD/JOYSTICK DETECTED! Check device connection.
📋 CONNECTED INPUT DEVICES:
   - Keyboard (Type: Keyboard)
   - Mouse (Type: Mouse)
```
→ Your gamepad isn't being recognized. Check:
  - USB connection
  - Device drivers (Windows Device Manager)
  - Edit → Project Settings → Input System Package → Supported Devices

---

## Setup Checklist
- ✅ Input System package installed (com.unity.inputsystem v1.14.2+)
- ✅ PaddleEnhanced.cs - Paddle movement with analog stick + button speed controls
- ✅ Ball.cs - Ball speed controls using different buttons
- ✅ PaddleInputSystemDemo.cs - Educational reference for all input device types
- ✅ PS5 DualSense Controller tested and working
- ✅ Generic USB Gamepad button mapping documented
- ✅ Keyboard fallback (A/D and arrow keys) implemented

## Discussion Questions for Your Class

1. **Why does pressing the analog stick harder make the paddle move faster?**
   - Answer: The analog stick returns a continuous value (-1 to 1), not just on/off. The magnitude of the value determines the speed.

2. **How is analog stick input different from button input?**
   - Answer: Analog = proportional/continuous values; Buttons = binary true/false values

3. **Why do different gamepads have different button mappings?**
   - Answer: Hardware manufacturers implement controllers differently. Generic devices don't always follow standard layouts.

4. **What's the advantage of having speed modifiers (slow/fast buttons)?**
   - Answer: Gives players more control and precision; good game design for accessibility

5. **How would you implement support for the D-pad?**
   - Answer: Similar to analog stick - check `Gamepad.current.dpad.left/right.isPressed`

## Resources
- [Unity Input System Documentation](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest)
- [InputSystem GitHub](https://github.com/Unity-Technologies/InputSystem)
- [PS5 Controller Buttons Reference](https://www.playstation.com/en-us/accessories/dualsense-wireless-controller/)

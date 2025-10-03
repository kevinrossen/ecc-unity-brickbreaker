# Week 5: Event Systems and Communication
## Simplified 50-Minute Lesson Plan for High School Students

---

## Learning Objectives
By the end of this lesson, students will:
1. Understand what events are and why they're useful in game development
2. Implement UnityEvents to respond to game actions
3. Create simple C# events for communication between game objects
4. Apply events to improve their brick breaker game

---

## Materials Needed
- Unity project (brick breaker game)
- Pre-made example scripts (optional)
- Completed reference project for demonstration

---

## Pre-Class Preparation (Optional Homework - 10 minutes)
Watch: "Unity Events Explained" (YouTube - search for beginner-friendly tutorial)
**Think about:** In your brick breaker game, what happens when a brick is destroyed? How many different things need to know about it? (Score UI, sound effects, particle effects, etc.)

---

## Lesson Outline

### **Part 1: Introduction to Events (8 minutes)**

#### Hook & Real-World Analogy (3 minutes)
"Think about a fire alarm in school:
- When it goes off, MANY things happen automatically:
  - Lights flash
  - Doors unlock
  - Office gets notified
  - Announcement system activates
- The fire alarm doesn't need to know about each of these systems
- It just 'announces' that there's an alarm
- Each system listens for the alarm and reacts on its own

**Events in games work the same way!**"

#### Problem Without Events (5 minutes)
Show this problematic code example on screen:
```csharp
// BAD: Brick knows about too many things!
public class Brick : MonoBehaviour
{
    void Hit()
    {
        // Brick has to find and talk to everyone directly
        FindObjectOfType<ScoreManager>().AddScore(100);
        FindObjectOfType<AudioManager>().PlaySound("brick_break");
        FindObjectOfType<ParticleManager>().SpawnParticles(transform.position);
        FindObjectOfType<UIManager>().ShowBrickDestroyed();
        FindObjectOfType<ComboSystem>().IncrementCombo();
        
        Destroy(gameObject);
    }
}
```

**Ask students:** "What problems do you see with this code?"
- Hard to add new features
- Slow (FindObjectOfType is expensive)
- Brick needs to know about everything
- What if one of these objects doesn't exist?

---

### **Part 2: UnityEvents - Visual Events (12 minutes)**

#### Introduction (2 minutes)
"UnityEvents let you connect responses in the Inspector - no code required for listeners!"

#### Hands-On Activity: Add Event to Ball Miss (10 minutes)

**Students follow along:**

1. **Open Ball script or ResetZone script**
   
2. **Add UnityEvent at the top:**
```csharp
using UnityEngine.Events;

public class ResetZone : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent OnBallMissed;  // This will show in Inspector!
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Ball")
        {
            // Invoke the event - anyone listening will respond
            OnBallMissed?.Invoke();
            
            GameManager.Instance.OnBallMiss();
        }
    }
}
```

3. **In Unity Inspector:**
   - Select the ResetZone object
   - Find "On Ball Missed" event
   - Click "+" to add a listener
   - Drag the Camera object to the empty slot
   - Select function: `Camera > Shake` (if you have a shake script)
   - OR connect to an audio source to play a sound

4. **Test it!** When the ball is missed, the event triggers whatever you connected.

**Benefits of UnityEvents:**
- Designers can configure responses without coding
- Easy to add/remove responses
- Visual in the Inspector
- Great for level-specific behaviors

---

### **Part 3: C# Events - Code-Based Events (15 minutes)**

#### Introduction (2 minutes)
"C# events are more powerful and better for system-to-system communication. They're faster and more flexible."

#### Guided Example: Brick Destroyed Event (13 minutes)

**Step 1: Create a Simple Event Manager (5 minutes)**

Create new script: `GameEvents.cs`
```csharp
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    // Singleton pattern - only one instance
    public static GameEvents Instance;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    // Define events - these are the "announcements"
    // System.Action is a simple event with no parameters
    public event System.Action OnBrickDestroyed;
    
    // System.Action<int> is an event that sends an integer
    public event System.Action<int> OnScoreChanged;
    
    // Call these methods to "announce" that something happened
    public void BrickDestroyed()
    {
        OnBrickDestroyed?.Invoke();  // The ? makes it safe if no one is listening
    }
    
    public void ScoreChanged(int newScore)
    {
        OnScoreChanged?.Invoke(newScore);
    }
}
```

**Step 2: Make Brick Announce When Destroyed (3 minutes)**

Modify the `Brick.cs` Hit method:
```csharp
private void Hit()
{
    health--;
    
    if (health <= 0)
    {
        // Announce that this brick was destroyed
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.BrickDestroyed();
        }
        
        gameObject.SetActive(false);
    }
    
    GameManager.Instance.OnBrickHit(this, pointsToAdd);
}
```

**Step 3: Create a Listener (5 minutes)**

Create new script: `BrickDestroyedListener.cs`
```csharp
using UnityEngine;

public class BrickDestroyedListener : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private AudioClip destroySound;
    [SerializeField] private GameObject particlePrefab;
    
    private AudioSource audioSource;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    
    private void OnEnable()
    {
        // Subscribe - start listening to the event
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnBrickDestroyed += HandleBrickDestroyed;
        }
    }
    
    private void OnDisable()
    {
        // Unsubscribe - stop listening (VERY IMPORTANT!)
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnBrickDestroyed -= HandleBrickDestroyed;
        }
    }
    
    // This method runs whenever ANY brick is destroyed
    private void HandleBrickDestroyed()
    {
        Debug.Log("A brick was destroyed!");
        
        if (audioSource != null && destroySound != null)
        {
            audioSource.PlayOneShot(destroySound);
        }
    }
}
```

**Important Rules for C# Events:**
1. Always subscribe in `OnEnable()` and unsubscribe in `OnDisable()`
2. Use `?.Invoke()` to safely invoke events (won't crash if no listeners)
3. The `+=` means "add a listener"
4. The `-=` means "remove a listener" (prevents memory leaks!)

---

### **Part 4: Comparison & When to Use Each (5 minutes)**

**Quick Reference Chart** (show on screen):

| Feature | UnityEvents | C# Events |
|---------|-------------|-----------|
| **Setup** | In Inspector | In code |
| **Good For** | Designer tweaks, level-specific | System communication |
| **Performance** | Slower | Faster |
| **Flexibility** | Less flexible | Very flexible |
| **Parameters** | Limited types | Any type you want |
| **Learning Curve** | Easy | Medium |

**Use UnityEvents when:**
- Designers need to configure responses
- One-off, specific situations (this button opens this door)
- Prototyping quickly

**Use C# Events when:**
- Communicating between game systems
- Performance matters
- Need complex data passed with the event
- Building reusable systems

---

### **Part 5: Class Activity - Add Events to Your Game (8 minutes)**

**Challenge Options** (students pick one):

**Option 1: Beginner - Add UnityEvent for Paddle Hit**
- Add a UnityEvent to the Paddle that fires when the ball hits it
- Connect it to play a sound or particle effect in the Inspector

**Option 2: Intermediate - Create Lives Changed Event**
- Add a C# event to GameEvents for when lives change
- Make GameManager announce when lives decrease
- Create a listener that logs to console or updates UI

**Option 3: Advanced - Create Combo System**
- Track consecutive brick hits without missing
- Fire an event when combo reaches 5, 10, 15
- Create a listener that shows "COMBO x5!" message

**Instructor circulates to help students**

---

### **Part 6: Wrap-Up & Preview (2 minutes)**

**Key Takeaways:**
1. Events let objects communicate without knowing about each other
2. UnityEvents are great for Inspector-based setup
3. C# Events are better for system-to-system communication
4. Always remember to unsubscribe from C# events!

**Preview Homework:**
"For homework, you'll add a complete event system to your brick breaker game with at least 3 different events and multiple listeners for each."

---

## Homework Assignment

### Project: Event-Driven Brick Breaker

**Core Requirements:**

1. **Create GameEvents Manager** with at least 3 events:
   - `OnBrickDestroyed` - when any brick breaks
   - `OnLivesChanged` - when player gains or loses a life
   - `OnLevelComplete` - when all bricks are destroyed

2. **Implement Event Invoking**
   - Make the appropriate scripts announce these events at the right time

3. **Create At Least 6 Listeners** (2 per event), such as:
   - Audio system that plays sounds
   - Particle effects spawner
   - UI updater for score/lives
   - Debug logger
   - Camera shake effect
   - Screen flash effect

4. **Add One UnityEvent**
   - Add a UnityEvent to any script
   - Connect at least 2 responses in the Inspector
   - Take a screenshot showing your setup

**Bonus Challenges:**
- Create an event with parameters (e.g., `OnScoreChanged(int newScore)`)
- Implement a combo system that tracks consecutive hits
- Add a "game paused" event that multiple systems respond to

**Submission Requirements:**
- Zip your Unity project folder (or push to Git)
- Include a text file listing:
  - What events you created
  - What listeners you made
  - Any challenges you faced
  - Screenshots of Inspector setup for UnityEvents

**Common Mistakes to Avoid:**
- ❌ Forgetting to unsubscribe from C# events
- ❌ Not checking if GameEvents.Instance is null
- ❌ Using FindObjectOfType in frequently-called methods
- ❌ Having events that are too specific (e.g., OnRedBrickInTopLeftDestroyed)

---

## Teacher Notes

**Timing Adjustments:**
- If students struggle with C# events, spend less time on Part 3 and more on Part 2 (UnityEvents)
- Advanced classes can skip Part 2 entirely and focus on C# events
- Part 4 activity can extend to 15 minutes if students are engaged

**Common Student Questions:**
- **"Why not just use FindObjectOfType?"** - It's slow and creates tight coupling
- **"What does the ? do in `?.Invoke()`?"** - It's the null-conditional operator; prevents crashes if null
- **"Do I always need a singleton?"** - For event managers, yes; it makes them easy to access globally

**Differentiation:**
- **Struggling students:** Focus only on UnityEvents, skip C# events
- **Advanced students:** Challenge them with events that pass complex data (Vector3, custom classes)
- **Mixed ability:** Pair programming with one student writing announcements, other writing listeners

**Extension Activities:**
- Research the Observer pattern in software design
- Explore Unity's ScriptableObject-based event architecture
- Implement an achievement system using events

---

## Additional Resources

**For Students:**
- Unity Documentation: UnityEvent
- C# Documentation: Events (Microsoft Learn)
- YouTube: "Beginner's Guide to Unity Events"

**For Teachers:**
- Unity Learn: "Create Modular Game Architecture"
- Book: "Game Programming Patterns" by Robert Nystrom (Observer chapter)
- Video: "Events vs Delegates vs UnityEvents" (Brackeys or similar)

---

*Created: October 2, 2025*
*Target Time: 50 minutes*
*Difficulty: Intermediate*

# Week 4: Unity Component Systems & Custom Inspectors
## 45-Minute Teaching Outline

---

## **PART 1: Component Architecture** (15 minutes)

### **Opening Demo** (3 min)
- ✅ Run the brick breaker game
- ✅ Show the current GameManager script (all-in-one example)
- ✅ Ask: *"What happens when we want to add multiplayer? Or save systems?"*

### **The Problem with Monolithic Scripts** (5 min)
#### Live Example - Show GameManager.cs:
- 🔴 **Problem**: Everything mixed together (score + lives + UI + levels)
- 🔴 **Problem**: Can't reuse parts in other projects  
- 🔴 **Problem**: Multiple people can't work on it simultaneously
- 🔴 **Problem**: One bug breaks everything

#### The Solution - Component-Based Design:
```csharp
// BEFORE: One giant script
public class GameManager : MonoBehaviour {
    // 200+ lines handling everything
}

// AFTER: Focused components
public class ScoreManager : MonoBehaviour { }
public class LivesManager : MonoBehaviour { }
public class UIManager : MonoBehaviour { }
```

### **Live Refactor Demo** (7 min)
#### Extract ScoreManager from GameManager:
1. Create new `ScoreManager.cs`
2. Move score variables and methods
3. Add UnityEvents for communication:
```csharp
[Header("Events")]
public UnityEvent<int> OnScoreChanged;
```
4. Connect in Inspector (drag & drop)
5. **Run game - still works!** ✨

**Key Point**: *"Each component has ONE job and does it well"*

---

## **PART 2: Unity Attributes - Quick Wins** (10 minutes)

### **Transform Your Inspector in 60 Seconds** (5 min)
#### Live Code on Paddle.cs:
```csharp
public class Paddle : MonoBehaviour 
{
    [Header("Movement Settings")]
    [Range(5f, 20f)]
    [Tooltip("How fast the paddle moves")]
    public float moveSpeed = 10f;
    
    [Space(10)]
    
    [Header("Boundaries")]
    [SerializeField] private float maxPosition = 8f;
    
    [Header("Power-ups")]
    public bool hasPowerUp = false;
    [Range(1f, 10f)]
    public float powerUpDuration = 5f;
    
    [Space]
    
    [TextArea(3, 5)]
    public string designerNotes = "Add level notes here...";
}
```

### **Student Practice** (5 min)
- 📝 Students add attributes to Ball.cs
- 📝 Must include: Header, Range, Tooltip
- 📝 Test in Inspector immediately
- 💡 **Instant visual improvement!**

---

## **PART 3: Custom Inspector Magic** (15 minutes)

### **The "WOW" Demo** (3 min)
1. Show default LevelConfiguration inspector (boring list)
2. Show custom inspector with:
   - Visual grid preview
   - "Generate Pattern" buttons
   - Validation warnings
3. *"Let's build this together!"*

### **Live Coding - Custom Inspector Basics** (10 min)

#### Step 1: Create the Editor Script
```csharp
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelConfiguration))]
public class LevelConfigurationEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        // Draw the normal inspector
        DrawDefaultInspector();
        
        // Add space
        EditorGUILayout.Space();
        
        // Cast target to our type
        LevelConfiguration config = (LevelConfiguration)target;
```

#### Step 2: Add Interactive Buttons
```csharp
        // Add section label
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        
        // Create buttons
        if (GUILayout.Button("Generate Random Level", GUILayout.Height(30)))
        {
            GenerateRandomLevel(config);
        }
        
        if (GUILayout.Button("Clear Level", GUILayout.Height(30)))
        {
            ClearLevel(config);
        }
```

#### Step 3: Add Validation Feedback
```csharp
        // Show warnings
        if (config.rows <= 0 || config.columns <= 0)
        {
            EditorGUILayout.HelpBox("Set rows and columns!", MessageType.Error);
        }
        
        // Show stats
        EditorGUILayout.LabelField($"Total Bricks: {CountBricks(config)}");
    }
```

#### Step 4: Implement Button Methods
```csharp
    void GenerateRandomLevel(LevelConfiguration config)
    {
        config.brickLayout = new List<BrickData>();
        for (int i = 0; i < config.rows * config.columns; i++)
        {
            if (Random.Range(0f, 1f) > 0.3f)  // 70% chance
            {
                int randomType = Random.Range(0, config.availableBrickTypes.Count);
                config.brickLayout.Add(config.availableBrickTypes[randomType]);
            }
        }
        EditorUtility.SetDirty(config);  // Save changes
    }
}
```

### **Test Together** (2 min)
- Click buttons - see immediate results
- Show how it speeds up level design
- **"This is the power of tool development!"**

---

## **PART 4: Practice & Wrap-Up** (5 minutes)

### **Quick Challenge** (3 min)
Students add to their own scripts:
1. One custom button that does something useful
2. One validation warning
3. Test it works

### **Key Takeaways** (2 min)
✅ **Components** = Single responsibility, reusable, testable  
✅ **Attributes** = Better inspector UX with 5 seconds of work  
✅ **Custom Inspectors** = Make Unity work for YOUR workflow  

---

## **HOMEWORK ASSIGNMENT**

### **"Inspector Enhancement Project"**
Transform ONE script from your game:

1. **Add Attributes** (Required):
   - Minimum 3 `[Header]` sections
   - Minimum 2 `[Range]` sliders  
   - At least 1 `[Tooltip]` and `[TextArea]`

2. **Create Custom Inspector** (Required):
   - Add 2 functional buttons
   - Include 1 validation message
   - Display 1 calculated statistic

3. **Refactor One Component** (Bonus):
   - Break any large script into 2+ components
   - Use UnityEvents for communication

### **Resources to Share:**
- Unity Attributes Documentation
- Custom Editor Scripting Guide
- Your example code from class

### **Next Week Preview:**
"We'll use these custom tools to build procedural level generation!"

---

## **Teaching Tips & Time Management**

### **If Running Behind:**
- Skip component refactoring details → Focus on custom inspector
- Provide pre-written attribute examples
- Have students follow along rather than code themselves

### **If Time Permits:**
- Show property drawers
- Add visual preview grid
- Demonstrate OnSceneGUI for in-scene editing

### **Common Issues to Watch For:**
- ⚠️ Students forgetting `using UnityEditor;`
- ⚠️ Editor scripts not in Editor folder
- ⚠️ Forgetting `EditorUtility.SetDirty()` for changes
- ⚠️ NullReference from unassigned GameManager

### **Success Indicators:**
- Students say "That's so much better!" when seeing custom inspector
- Students immediately use [Header] in their own projects
- Students ask "Can we make a button that does X?"

---

*Remember: The goal isn't to cover everything - it's to inspire students to explore these tools on their own!*
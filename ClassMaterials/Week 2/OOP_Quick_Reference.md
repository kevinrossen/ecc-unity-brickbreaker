# OOP Class - Quick Reference Guide

## 🚀 **SETUP CHECKLIST**
- [ ] Unity project open with platformer scene
- [ ] `OOP_Demo_Manager` exists and is configured
- [ ] Console window visible for students
- [ ] `BadExample_DuplicateCode.cs` ready to show
- [ ] Test: Press Play → Press 1, 2, 3 (enemies should spawn)

---

## 🎮 **DEMO CONTROLS**

### **Main Demo Controls:**
```
Press 1: Spawn Goblin (green, sneaky)
Press 2: Spawn Orc (red, strong, rage mode)  
Press 3: Spawn Skeleton (white, magical, reassembles)
Press P: POLYMORPHISM DEMO! (All enemies attack)
Press O: Damage all enemies (shows inheritance)
Press C: Clear all enemies
```

### **Player Movement:**
- Students can move player with **WASD** while demo runs
- Demo keys don't conflict with movement

---

## ⏰ **CLASS TIMING**

| Time | Activity | Key Action |
|------|----------|------------|
| 0-5 min | Opening Hook | Show duplicate code problem |
| 5-10 min | Identify Problem | Draw "Same vs Different" columns |
| 10-25 min | Live Code Solution | Walk through BaseEnemy.cs |
| 25-33 min | **POLYMORPHISM DEMO** | **Press P key - BIG MOMENT!** |
| 33-48 min | Students Code | Distribute starter code |
| 48-50 min | Wrap Up | Three key takeaways |

---

## 💬 **KEY TALKING POINTS**

### **Opening Hook:**
- "I need 10 more enemy types = 500+ lines of duplicate code!"
- "There HAS to be a better way!"

### **Polymorphism Moment:**
- **[Press P]** "ONE line of code, THREE different behaviors!"
- "Same method call, different implementations!"

### **Wrap Up:**
- "This is how professional games are built!"
- "90% less code to maintain!"

---

## 🎯 **LEARNING OBJECTIVES**

Students should understand:
1. **Inheritance** = Common code in parent class
2. **Override** = Same method name, different implementation  
3. **Polymorphism** = One interface, many behaviors

---

## 📝 **CODE SNIPPETS TO SHOW**

### **Bad Example (Show First):**
```csharp
// Look at all this duplicate code!
public class Goblin {
    int health = 50;
    void Attack() { ... }
    void TakeDamage() { ... }
}
public class Orc {
    int health = 100;  // DUPLICATE!
    void Attack() { ... }
    void TakeDamage() { ... }  // DUPLICATE!
}
```

### **Good Example (BaseEnemy):**
```csharp
public abstract class BaseEnemy : MonoBehaviour
{
    protected int health;           // Common to ALL
    public abstract void Attack();  // MUST implement
    public virtual void TakeDamage(int damage) // CAN override
    {
        health -= damage;
        if (health <= 0) Die();
    }
}
```

### **Child Class Example:**
```csharp
public class Goblin : BaseEnemy
{
    void Start() {
        health = 50;  // Set unique values
    }
    
    public override void Attack()  // Unique behavior
    {
        Debug.Log("Goblin sneak attacks!");
    }
}
```

### **Polymorphism Magic:**
```csharp
BaseEnemy[] enemies = {goblin, orc, skeleton};
foreach (BaseEnemy enemy in enemies)
{
    enemy.Attack();  // ONE line, MANY behaviors!
}
```

---

## 🛠️ **TROUBLESHOOTING**

### **Demo Not Working:**
| Problem | Quick Fix |
|---------|-----------|
| No enemies spawn | Check Enemy Template assigned in OOP_Demo_Manager |
| Console errors | Focus on concepts, comment out broken code |
| Unity crashes | Restart Unity, use backup scene |

### **Student Coding Issues:**
| Problem | Solution |
|---------|----------|
| "I don't understand inheritance" | Analogy: "Cookie cutter (parent) makes different cookies (children)" |
| "What's abstract mean?" | "It's a rule: you MUST implement this method" |
| "Virtual vs Override?" | "Virtual = you CAN change it, Override = you ARE changing it" |

### **Time Management:**
| Behind Schedule | Skip/Adjust |
|-----------------|-------------|
| 10 min behind | Skip detailed code walkthrough |
| 15 min behind | Demo only, assign coding as homework |
| 20 min behind | Focus on polymorphism demo only |

---

## 🎪 **DEMO SEQUENCE**

### **Opening Demo:**
1. Hit Play
2. Press 1, 2, 3 (spawn all enemies)
3. "Look at all this duplicate code!"

### **Polymorphism Demo:**
1. Make sure all 3 enemies are spawned
2. **Press P** (the big moment!)
3. Read console output aloud
4. "ONE method call, THREE behaviors!"

### **Inheritance Demo:**
1. Press O (damage all)
2. Point out different damage behaviors
3. "Same method, different results!"

---

## 📊 **SUCCESS METRICS**

### **Must Achieve:**
- [ ] Students see polymorphism working (P key demo)
- [ ] Students understand inheritance concept
- [ ] Students successfully create one child class

### **Bonus Goals:**
- [ ] Students create custom enemy type
- [ ] Students explain concepts in their own words
- [ ] Students get excited about OOP possibilities

---

## 🔥 **BACKUP PLANS**

### **Plan B - No Coding:**
- Focus entirely on demo and concepts
- Show pre-written code examples
- Assign coding as homework

### **Plan C - Technical Issues:**
- Draw UML diagrams on board
- Use pseudocode instead of real code
- Focus on conceptual understanding

### **Plan D - Lost Audience:**
- Return to Unity demo
- Let students control the demo
- Make it more interactive/game-like

---

## 💡 **TEACHER TIPS**

### **Energy Boosters:**
- **Press P** when energy drops - polymorphism is always exciting!
- Let students suggest enemy types during coding
- Encourage creative custom enemies

### **Difficult Concepts:**
- **Abstract:** "It's a contract - you MUST implement this"
- **Polymorphism:** "Many shapes, one interface"  
- **Inheritance:** "Don't repeat yourself - write once, use everywhere"

### **Engagement Tactics:**
- Ask: "What enemy type should we add next?"
- Challenge: "Can you break this code?"
- Predict: "What will happen when I press P?"

---

## 🎓 **HOMEWORK ASSIGNMENT**

> "Add two more enemy types to your starter code. Make them as creative as possible! Examples: Dragon (flies), Robot (electric attacks), Wizard (magic spells), Giant Spider (web attacks). Due next class!"

---

**Remember: The P key demo is your secret weapon - use it whenever you need to re-engage the class!** 🚀

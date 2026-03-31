using UnityEngine;

public class PlayerStats : CharacterStats
{
    [Header("Player Specific")]
    public int level = 1;
    public int experience = 0;
    public int experienceToNextLevel = 100;
    
    [Header("Combat Context")]
    public EnemyStats targetedEnemy;
    public float criticalChance = 0.1f;
    public float criticalMultiplier = 2f;
    
    protected override void Start()
    {
        base.Start();
        isPlayer = true;
        characterName = $"Player {playerID}";
    }
    
    public void SetTarget(EnemyStats enemy)
    {
        // Clear previous target highlight if exists
        if (targetedEnemy != null)
        {
            targetedEnemy.SetTargeted(false);
        }
        
        targetedEnemy = enemy;
        
        // Highlight new target
        if (targetedEnemy != null)
        {
            targetedEnemy.SetTargeted(true);
            Debug.Log($"{characterName} is now targeting {targetedEnemy.characterName}");
        }
    }
    
    public void AttackTarget()
    {
        if (targetedEnemy == null)
        {
            Debug.LogWarning("No enemy targeted!");
            return;
        }
        
        if (!targetedEnemy.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("Target is no longer active!");
            targetedEnemy = null;
            return;
        }
        
        float damage = CalculateDamageAgainstEnemy(targetedEnemy);
        targetedEnemy.TakeDamage(damage);
        
        // Check if enemy died
        if (targetedEnemy.currentHealth <= 0)
        {
            GainExperience(targetedEnemy.experienceReward);
            targetedEnemy = null; // Clear target
        }
    }
    
    public float CalculateDamageAgainstEnemy(EnemyStats enemy)
    {
        float baseDamage = attack.GetValue();
        
        // Apply critical hit
        bool isCritical = Random.Range(0f, 1f) < criticalChance;
        if (isCritical)
        {
            baseDamage *= criticalMultiplier;
            Debug.Log("Critical hit!");
        }
        
        // Apply enemy-specific modifiers (e.g., elemental weaknesses)
        if (enemy.weaknessMultiplier > 0)
        {
            baseDamage *= enemy.weaknessMultiplier;
        }
        
        return baseDamage;
    }
    
    public void GainExperience(int amount)
    {
        experience += amount;
        Debug.Log($"{characterName} gained {amount} experience!");
        
        while (experience >= experienceToNextLevel)
        {
            LevelUp();
        }
    }
    
    void LevelUp()
    {
        level++;
        experience -= experienceToNextLevel;
        experienceToNextLevel = Mathf.RoundToInt(experienceToNextLevel * 1.5f);
        
        // Increase stats on level up
        health.baseValue += 10;
        attack.baseValue += 2;
        defense.baseValue += 1;
        speed.baseValue += 0.5f;
        
        // Fully heal on level up
        currentHealth = health.GetValue();
        
        Debug.Log($"{characterName} leveled up to level {level}!");
    }
    
    protected override void Die()
    {
        base.Die();
        // Respawn logic for multiplayer
        // You might want to handle this differently for multiplayer
        Debug.Log($"Player {playerID} needs to respawn!");
    }
}
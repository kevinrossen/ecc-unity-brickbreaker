using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Stat
{
    public float baseValue;
    public float currentValue;
    
    private List<float> modifiers = new List<float>();
    
    public Stat(float baseValue)
    {
        this.baseValue = baseValue;
        this.currentValue = baseValue;
    }
    
    public float GetValue()
    {
        float finalValue = baseValue;
        foreach (float modifier in modifiers)
        {
            finalValue += modifier;
        }
        return finalValue;
    }
    
    public void AddModifier(float modifier)
    {
        modifiers.Add(modifier);
        currentValue = GetValue();
    }
    
    public void RemoveModifier(float modifier)
    {
        modifiers.Remove(modifier);
        currentValue = GetValue();
    }
}

public class CharacterStats : MonoBehaviour
{
    [Header("Base Stats")]
    public Stat health = new Stat(100);
    public Stat attack = new Stat(10);
    public Stat defense = new Stat(5);
    public Stat speed = new Stat(5);
    
    [Header("Current Values")]
    public float currentHealth;
    
    [Header("Character Info")]
    public string characterName;
    public bool isPlayer = false;
    public int playerID = -1; // For multiplayer identification
    
    protected virtual void Start()
    {
        currentHealth = health.GetValue();
    }
    
    public virtual void TakeDamage(float damage)
    {
        // Calculate actual damage after defense
        float actualDamage = Mathf.Max(damage - defense.GetValue(), 1);
        currentHealth -= actualDamage;
        
        Debug.Log($"{characterName} took {actualDamage} damage. Health: {currentHealth}/{health.GetValue()}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    protected virtual void Die()
    {
        Debug.Log($"{characterName} has died!");
        // Override in derived classes for specific death behavior
    }
    
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, health.GetValue());
        Debug.Log($"{characterName} healed for {amount}. Health: {currentHealth}/{health.GetValue()}");
    }
    
    public virtual float CalculateDamage()
    {
        return attack.GetValue();
    }
}
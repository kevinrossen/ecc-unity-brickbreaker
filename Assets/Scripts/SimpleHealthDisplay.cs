using UnityEngine;
using TMPro;

/// <summary>
/// INTENTIONALLY BROKEN - Simpler Version for Debugging Practice
/// This script has fewer bugs, making it easier for beginners
/// </summary>
public class SimpleHealthDisplay : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text healthText;
    
    [Header("Settings")]
    public int maxHealth = 100;
    
    private int currentHealth;
    
    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthDisplay();
    }
    
    void Update()
    {
        // Test damage with number keys
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TakeDamage(10);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TakeDamage(25);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Heal(15);
        }
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Took {damage} damage! Health: {currentHealth}");
        
        // BUG: Not checking if healthText is assigned before using it
        UpdateHealthDisplay();
    }
    
    public void Heal(int amount)
    {
        currentHealth += amount;
        
        // Make sure we don't go over max health
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        
        Debug.Log($"Healed {amount}! Health: {currentHealth}");
        UpdateHealthDisplay();
    }
    
    void UpdateHealthDisplay()
    {
        // BUG: healthText might not be assigned in the Inspector
        healthText.text = $"Health: {currentHealth}/{maxHealth}";
    }
}

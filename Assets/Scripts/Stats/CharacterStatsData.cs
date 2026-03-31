using UnityEngine;

[CreateAssetMenu(fileName = "New Character Stats", menuName = "Stats/Character Stats")]
public class CharacterStatsData : ScriptableObject
{
    [Header("Base Stats")]
    public float baseHealth = 100f;
    public float baseAttack = 10f;
    public float baseDefense = 5f;
    public float baseSpeed = 5f;
    
    [Header("Character Info")]
    public string characterName = "Character";
    public Sprite characterIcon;
    
    [Header("Combat Modifiers")]
    public float criticalChance = 0.1f;
    public float criticalMultiplier = 2f;
    
    [Header("Resistances and Weaknesses")]
    [Range(0f, 2f)]
    public float physicalResistance = 1f; // 1 = normal, 0.5 = 50% resistance, 2 = 200% damage
    [Range(0f, 2f)]
    public float magicalResistance = 1f;
    [Range(0f, 2f)]
    public float elementalResistance = 1f;
    
    [Header("Rewards (For Enemies)")]
    public int experienceReward = 50;
    public int goldReward = 10;
    
    // Method to create a copy with modified stats for different difficulties
    public CharacterStatsData CreateScaledVersion(float scaleFactor)
    {
        CharacterStatsData scaledStats = ScriptableObject.CreateInstance<CharacterStatsData>();
        scaledStats.baseHealth = baseHealth * scaleFactor;
        scaledStats.baseAttack = baseAttack * scaleFactor;
        scaledStats.baseDefense = baseDefense * scaleFactor;
        scaledStats.baseSpeed = baseSpeed * scaleFactor;
        scaledStats.characterName = characterName + " (Scaled)";
        scaledStats.experienceReward = Mathf.RoundToInt(experienceReward * scaleFactor);
        scaledStats.goldReward = Mathf.RoundToInt(goldReward * scaleFactor);
        return scaledStats;
    }
}
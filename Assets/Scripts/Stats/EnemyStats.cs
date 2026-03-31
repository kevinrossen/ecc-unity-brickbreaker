using UnityEngine;

public class EnemyStats : CharacterStats
{
    [Header("Enemy Specific")]
    public EnemyType enemyType;
    public int experienceReward = 50;
    public float detectionRange = 10f;
    public float attackRange = 2f;
    
    [Header("Enemy Modifiers")]
    public float weaknessMultiplier = 1f; // e.g., 1.5f for 50% more damage from certain attacks
    public float resistanceMultiplier = 1f; // e.g., 0.5f for 50% less damage
    
    [Header("Targeting")]
    public bool isTargeted = false;
    private GameObject targetIndicator;
    
    [Header("AI Behavior")]
    public PlayerStats currentTarget;
    public float attackCooldown = 2f;
    private float nextAttackTime = 0f;
    
    protected override void Start()
    {
        base.Start();
        isPlayer = false;
        
        // Set stats based on enemy type
        InitializeEnemyType();
        
        // Create target indicator (you can replace this with a prefab)
        CreateTargetIndicator();
    }
    
    void InitializeEnemyType()
    {
        switch (enemyType)
        {
            case EnemyType.Goblin:
                characterName = "Goblin";
                health = new Stat(50);
                attack = new Stat(5);
                defense = new Stat(2);
                speed = new Stat(7);
                experienceReward = 25;
                break;
                
            case EnemyType.Orc:
                characterName = "Orc";
                health = new Stat(100);
                attack = new Stat(15);
                defense = new Stat(8);
                speed = new Stat(3);
                experienceReward = 75;
                break;
                
            case EnemyType.Dragon:
                characterName = "Dragon";
                health = new Stat(500);
                attack = new Stat(50);
                defense = new Stat(20);
                speed = new Stat(5);
                experienceReward = 500;
                resistanceMultiplier = 0.5f; // Resistant to physical
                break;
                
            default:
                characterName = "Enemy";
                break;
        }
        
        currentHealth = health.GetValue();
    }
    
    void CreateTargetIndicator()
    {
        // Create a simple target indicator (ring around enemy)
        targetIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        targetIndicator.transform.SetParent(transform);
        targetIndicator.transform.localPosition = new Vector3(0, 0.1f, 0);
        targetIndicator.transform.localScale = new Vector3(2, 0.1f, 2);
        
        // Make it red and remove collider
        targetIndicator.GetComponent<Renderer>().material.color = Color.red;
        Destroy(targetIndicator.GetComponent<Collider>());
        targetIndicator.SetActive(false);
    }
    
    public void SetTargeted(bool targeted)
    {
        isTargeted = targeted;
        if (targetIndicator != null)
        {
            targetIndicator.SetActive(targeted);
        }
    }
    
    void Update()
    {
        if (!isPlayer && gameObject.activeInHierarchy)
        {
            FindNearestPlayer();
            
            if (currentTarget != null)
            {
                float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
                
                if (distance <= attackRange && Time.time >= nextAttackTime)
                {
                    AttackPlayer();
                    nextAttackTime = Time.time + attackCooldown;
                }
            }
        }
    }
    
    void FindNearestPlayer()
    {
        PlayerStats[] allPlayers = FindObjectsOfType<PlayerStats>();
        float nearestDistance = detectionRange;
        PlayerStats nearestPlayer = null;
        
        foreach (PlayerStats player in allPlayers)
        {
            if (player.currentHealth > 0)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestPlayer = player;
                }
            }
        }
        
        currentTarget = nearestPlayer;
    }
    
    void AttackPlayer()
    {
        if (currentTarget != null)
        {
            float damage = CalculateDamage();
            currentTarget.TakeDamage(damage);
            Debug.Log($"{characterName} attacks {currentTarget.characterName} for {damage} damage!");
        }
    }
    
    public override void TakeDamage(float damage)
    {
        // Apply resistance if applicable
        damage *= resistanceMultiplier;
        base.TakeDamage(damage);
    }
    
    protected override void Die()
    {
        base.Die();
        SetTargeted(false);
        
        // Drop loot, play death animation, etc.
        // For now, just destroy after a delay
        Destroy(gameObject, 2f);
    }
}

public enum EnemyType
{
    Goblin,
    Orc,
    Dragon,
    Skeleton,
    Zombie,
    Custom
}
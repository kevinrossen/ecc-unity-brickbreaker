using UnityEngine;

public class TargetingSystem : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;
    public Camera playerCamera;
    
    [Header("Targeting Settings")]
    public float maxTargetingDistance = 50f;
    public LayerMask enemyLayer = -1; // Set in inspector
    public KeyCode targetKey = KeyCode.Tab;
    public KeyCode attackKey = KeyCode.Space;
    
    [Header("Visual Feedback")]
    public Color targetHighlightColor = Color.yellow;
    
    void Start()
    {
        // Get references if not set
        if (playerStats == null)
        {
            playerStats = GetComponent<PlayerStats>();
        }
        
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }
    
    void Update()
    {
        HandleMouseTargeting();
        HandleTabTargeting();
        HandleCombatInput();
    }
    
    void HandleMouseTargeting()
    {
        // Right-click to target
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, maxTargetingDistance, enemyLayer))
            {
                EnemyStats enemy = hit.collider.GetComponent<EnemyStats>();
                if (enemy != null && enemy.currentHealth > 0)
                {
                    playerStats.SetTarget(enemy);
                }
            }
            else
            {
                // Clear target if clicking on nothing
                playerStats.SetTarget(null);
            }
        }
    }
    
    void HandleTabTargeting()
    {
        // Tab to cycle through nearby enemies
        if (Input.GetKeyDown(targetKey))
        {
            EnemyStats[] allEnemies = FindObjectsOfType<EnemyStats>();
            EnemyStats nearestEnemy = null;
            float nearestDistance = maxTargetingDistance;
            
            foreach (EnemyStats enemy in allEnemies)
            {
                if (enemy.currentHealth > 0)
                {
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);
                    
                    // If we already have a target, find the next closest enemy
                    if (playerStats.targetedEnemy == enemy)
                    {
                        continue;
                    }
                    
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestEnemy = enemy;
                    }
                }
            }
            
            if (nearestEnemy != null)
            {
                playerStats.SetTarget(nearestEnemy);
            }
        }
    }
    
    void HandleCombatInput()
    {
        // Space or left-click to attack
        if (Input.GetKeyDown(attackKey) || Input.GetMouseButtonDown(0))
        {
            if (playerStats.targetedEnemy != null)
            {
                // Check if in range
                float distance = Vector3.Distance(transform.position, playerStats.targetedEnemy.transform.position);
                float attackRange = 5f; // You can make this a variable
                
                if (distance <= attackRange)
                {
                    playerStats.AttackTarget();
                }
                else
                {
                    Debug.Log("Target is out of range!");
                }
            }
        }
    }
    
    void OnDrawGizmos()
    {
        // Visualize targeting range in editor
        if (playerStats != null && playerStats.targetedEnemy != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, playerStats.targetedEnemy.transform.position);
        }
    }
}
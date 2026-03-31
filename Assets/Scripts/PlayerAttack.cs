using UnityEngine;

/// <summary>
/// INTENTIONALLY BROKEN SCRIPT for debugging practice
/// This script demonstrates common beginner mistakes with Unity components
/// Students should use AI to help debug this!
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 2f;
    public int attackDamage = 10;
    public float attackCooldown = 0.5f;

    [Header("Effects")]
    public ParticleSystem attackEffect;
    public AudioClip attackSound;
    
    private Transform currentTarget;
    private float lastAttackTime;
    private AudioSource audioSource;
    
    void Start()
    {
        // BUG #1: Trying to get a component that might not exist
        audioSource = GetComponent<AudioSource>();
    }
    
    void Update()
    {
        // Check if player pressed attack button
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryAttack();
        }
        
        // BUG #2: Trying to access currentTarget without checking if it's null
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
        
        if (distanceToTarget > 10f)
        {
            currentTarget = null;
        }
    }
    
    void TryAttack()
    {
        // Check cooldown
        if (Time.time - lastAttackTime < attackCooldown)
        {
            Debug.Log("Attack on cooldown!");
            return;
        }
        
        // BUG #3: Not checking if currentTarget exists before using it
        if (Vector3.Distance(transform.position, currentTarget.position) <= attackRange)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }
        else
        {
            Debug.Log("Target out of range!");
        }
    }
    
    void PerformAttack()
    {
        Debug.Log($"Attacking {currentTarget.name} for {attackDamage} damage!");
        
        // BUG #4: Trying to play effect without checking if it's assigned
        attackEffect.Play();
        
        // BUG #5: Trying to play audio without checking if clip or source exists
        audioSource.PlayOneShot(attackSound);
    }
    
    // This method should be called by other scripts to set a target
    public void SetTarget(Transform target)
    {
        currentTarget = target;
        Debug.Log($"Target set to: {target.name}");
    }
    
    public bool HasTarget()
    {
        // BUG #6: Not handling null properly in a boolean return
        return currentTarget.gameObject.activeSelf;
    }
}

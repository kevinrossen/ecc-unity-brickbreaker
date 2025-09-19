using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Brick : MonoBehaviour
{
    [Header("Brick Configuration")]
    public BrickData brickData;
    
    // Legacy fields (keep for backwards compatibility)
    public Sprite[] states = new Sprite[0];
    public int points = 100;
    public bool unbreakable;

    private SpriteRenderer spriteRenderer;
    private int health;
    private AudioSource audioSource;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        ResetBrick();
    }

public void ResetBrick()
    {
        gameObject.SetActive(true);

        // Use scriptable object data if available, otherwise fall back to legacy fields
        bool useScriptableObject = brickData != null;
        bool isUnbreakableBlock = useScriptableObject ? brickData.isUnbreakable : unbreakable;
        
        if (!isUnbreakableBlock)
        {
            if (useScriptableObject)
            {
                health = brickData.maxHealth;
                if (brickData.healthStates.Length > 0)
                {
                    spriteRenderer.sprite = brickData.healthStates[health - 1];
                }
                spriteRenderer.color = brickData.brickColor;
            }
            else
            {
                // Legacy behavior
                health = states.Length;
                if (states.Length > 0)
                {
                    spriteRenderer.sprite = states[health - 1];
                }
            }
        }
    }

private void Hit()
    {
        bool useScriptableObject = brickData != null;
        bool isUnbreakableBlock = useScriptableObject ? brickData.isUnbreakable : unbreakable;
        
        if (isUnbreakableBlock) {
            return;
        }

        // Play hit sound
        if (useScriptableObject && brickData.hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(brickData.hitSound);
        }

        health--;

        if (health <= 0) 
        {
            // Play destroy sound
            if (useScriptableObject && brickData.destroySound != null && audioSource != null)
            {
                audioSource.PlayOneShot(brickData.destroySound);
            }
            
            // Check for powerup spawn
            if (useScriptableObject && brickData.spawnsPowerup && brickData.powerupPrefab != null)
            {
                if (Random.value <= brickData.powerupDropChance)
                {
                    Instantiate(brickData.powerupPrefab, transform.position, Quaternion.identity);
                }
            }
            
            gameObject.SetActive(false);
        } 
        else 
        {
            // Update sprite based on remaining health
            if (useScriptableObject && brickData.healthStates.Length > 0)
            {
                int spriteIndex = Mathf.Clamp(health - 1, 0, brickData.healthStates.Length - 1);
                spriteRenderer.sprite = brickData.healthStates[spriteIndex];
            }
            else if (states.Length > 0)
            {
                // Legacy behavior
                spriteRenderer.sprite = states[health - 1];
            }
        }

        // Calculate points
        int pointsToAdd = useScriptableObject ? brickData.pointValue : points;
        GameManager.Instance.OnBrickHit(this, pointsToAdd);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Ball") {
            Hit();
        }
    }

}

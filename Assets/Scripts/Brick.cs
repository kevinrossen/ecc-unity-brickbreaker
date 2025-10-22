using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Brick : MonoBehaviour
{
    [Header("Brick Configuration")]
    public BrickData brickData;
    [Tooltip("Optional visual profile that controls color mapping when no sprites are provided.")]
    public BrickVisualProfile visualProfile;
    
    // Legacy fields (kept for backwards compatibility but hidden to avoid confusion)
    [HideInInspector] public Sprite[] states = new Sprite[0];
    [HideInInspector] public int points = 100;
    [HideInInspector] public bool unbreakable;

    private SpriteRenderer spriteRenderer;
    private int health;
    private AudioSource audioSource;

    // Event that other systems can subscribe to (decoupled event system)
    // Passes the brick instance and points earned
    public static event System.Action<Brick, int> OnBrickHit;

    private void EnsureComponents()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    // Maps current health to a sprite index when using a sequence of sprites
    // Rule: if there are enough sprites, use index = health - 1 (1-based health to 0-based index)
    // If there are fewer sprites than max health, scale health across available sprite indices
    private int SpriteIndexForHealth(int remaining, int max, int spriteCount)
    {
        if (spriteCount <= 0) return -1;
        max = Mathf.Max(1, max);
        remaining = Mathf.Clamp(remaining, 1, max);
        if (max <= spriteCount)
        {
            // Direct mapping: health 1 -> index 0, ..., health max -> index max-1
            return Mathf.Clamp(remaining - 1, 0, spriteCount - 1);
        }
        // Scale health 1..max to indices 0..spriteCount-1
        if (max == 1) return 0;
        float t = (remaining - 1f) / (max - 1f); // 0..1
        int idx = Mathf.RoundToInt(t * (spriteCount - 1));
        return Mathf.Clamp(idx, 0, spriteCount - 1);
    }

    private int ParseBrickTypeValue(string name)
    {
        if (string.IsNullOrEmpty(name)) return 1;
        int i = 0;
        while (i < name.Length && char.IsWhiteSpace(name[i])) i++;
        int start = i;
        while (i < name.Length && char.IsDigit(name[i])) i++;
        if (i > start)
        {
            if (int.TryParse(name.Substring(start, i - start), out int value)) return value;
        }
        return 1; // default
    }

    private Color HealthToColor(int remaining, int max, bool unbreakable)
    {
        if (unbreakable) return visualProfile != null ? visualProfile.unbreakableColor : Color.gray;
        remaining = Mathf.Max(0, remaining);
        max = Mathf.Max(1, max);
        float t = 1f - Mathf.Clamp01((remaining - 1) / (float)(max - 1 == 0 ? 1 : max - 1));
        if (visualProfile != null && visualProfile.strengthGradient != null)
        {
            return visualProfile.strengthGradient.Evaluate(t);
        }
        // Fallback classic palette
        if (t < 0.33f) return Color.red;
        if (t < 0.66f) return new Color(1f, 0.5f, 0f);
        if (t < 0.9f) return Color.green;
        return Color.cyan;
    }

    private void Awake()
    {
        EnsureComponents();
    }

    private void Start()
    {
        ResetBrick();
    }

public void ResetBrick()
    {
        // Ensure components exist even when instantiated in Edit mode (Awake may not have run)
        EnsureComponents();
        gameObject.SetActive(true);

        // Use scriptable object data if available, otherwise fall back to legacy fields
        bool useScriptableObject = brickData != null;
        bool isUnbreakableBlock = useScriptableObject ? brickData.isUnbreakable : unbreakable;
        
        // Update the unbreakable field to match the data
        if (useScriptableObject)
        {
            unbreakable = brickData.isUnbreakable;
        }
        
        if (!isUnbreakableBlock)
        {
            if (useScriptableObject)
            {
                int brickTypeValue = ParseBrickTypeValue(brickData.name);
                health = Mathf.Max(1, brickTypeValue);
                int effectiveMaxHealth = brickTypeValue;
                if (brickData.healthStates.Length > 0 && spriteRenderer != null && (visualProfile == null || visualProfile.preferSpritesWhenAvailable))
                {
                    int idx = SpriteIndexForHealth(health, effectiveMaxHealth, brickData.healthStates.Length);
                    spriteRenderer.sprite = brickData.healthStates[Mathf.Clamp(idx, 0, brickData.healthStates.Length - 1)];
                    // When using per-health sprites, don't override colors; assume sprites carry the visual state
                }
                else if (spriteRenderer != null)
                {
                    // No per-health sprites: encode strength via color
                    spriteRenderer.color = HealthToColor(health, effectiveMaxHealth, false);
                }
            }
            else
            {
                // Legacy behavior
                health = Mathf.Max(1, states.Length);
                if (states.Length > 0 && spriteRenderer != null)
                {
                    int idx = SpriteIndexForHealth(health, states.Length, states.Length);
                    spriteRenderer.sprite = states[Mathf.Clamp(idx, 0, states.Length - 1)];
                }
            }
        }
    }

private void Hit()
    {
        EnsureComponents();
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
            
            // Calculate points
            int pointsToAdd = useScriptableObject ? brickData.pointValue : points;
            
            // Fire event for all listening systems (GameManager, AudioManager, ParticleManager, etc.)
            OnBrickHit?.Invoke(this, pointsToAdd);
        } 
        else 
        {
            // Update sprite based on remaining health (highest index at full health)
            if (useScriptableObject && brickData.healthStates.Length > 0 && spriteRenderer != null && (visualProfile == null || visualProfile.preferSpritesWhenAvailable))
            {
                int brickTypeValue = ParseBrickTypeValue(brickData.name);
                int effectiveMaxHealth = brickTypeValue;
                int spriteIndex = SpriteIndexForHealth(health, effectiveMaxHealth, brickData.healthStates.Length);
                spriteRenderer.sprite = brickData.healthStates[Mathf.Clamp(spriteIndex, 0, brickData.healthStates.Length - 1)];
            }
            else if (states.Length > 0 && spriteRenderer != null)
            {
                // Legacy behavior (highest index = full)
                int idx = SpriteIndexForHealth(health, states.Length, states.Length);
                spriteRenderer.sprite = states[Mathf.Clamp(idx, 0, states.Length - 1)];
            }
            else if (useScriptableObject && spriteRenderer != null)
            {
                // No per-health sprites: update color to reflect remaining strength
                int brickTypeValue = ParseBrickTypeValue(brickData.name);
                int effectiveMaxHealth = brickTypeValue;
                spriteRenderer.color = HealthToColor(health, effectiveMaxHealth, isUnbreakableBlock);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Ball") {
            Hit();
        }
    }

}

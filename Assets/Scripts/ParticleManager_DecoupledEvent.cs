using UnityEngine;

// Example listener component that demonstrates particle effects for decoupled events
public class ParticleManager_DecoupledEvent : MonoBehaviour
{
    [Header("Particle Effects")]
    [SerializeField] private GameObject brickDestroyParticles;
    [SerializeField] private GameObject ballMissParticles;
    [SerializeField] private GameObject levelCompleteParticles;

    private void OnEnable()
    {
        // Subscribe to events from different components
        Brick_DecoupledEvent.OnBrickHit += OnBrickDestroyed;
        ResetZone_DecoupledEvent.OnBallMissed += OnBallMissed;
        GameManager_DecoupledEvent.OnLevelCompleted += OnLevelCompleted;
    }

    private void OnDisable()
    {
        // Always unsubscribe to prevent memory leaks
        Brick_DecoupledEvent.OnBrickHit -= OnBrickDestroyed;
        ResetZone_DecoupledEvent.OnBallMissed -= OnBallMissed;
        GameManager_DecoupledEvent.OnLevelCompleted -= OnLevelCompleted;
    }

    private void OnBrickDestroyed(Brick_DecoupledEvent brick, int points)
    {
        // Only spawn particles when brick is actually destroyed
        // This would need to be modified based on your brick logic
        if (brickDestroyParticles != null)
        {
            Instantiate(brickDestroyParticles, brick.transform.position, Quaternion.identity);
        }
    }

    private void OnBallMissed()
    {
        // Spawn particles at the bottom of the screen or a specific location
        if (ballMissParticles != null)
        {
            Vector3 spawnPosition = new Vector3(0f, -4f, 0f); // Adjust based on your game layout
            Instantiate(ballMissParticles, spawnPosition, Quaternion.identity);
        }
    }

    private void OnLevelCompleted(int levelNumber)
    {
        // Spawn celebration particles in the center of the screen
        if (levelCompleteParticles != null)
        {
            Vector3 spawnPosition = Vector3.zero; // Center of screen
            Instantiate(levelCompleteParticles, spawnPosition, Quaternion.identity);
        }
    }
}
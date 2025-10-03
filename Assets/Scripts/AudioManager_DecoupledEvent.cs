using UnityEngine;

// Example listener component that demonstrates audio responses to decoupled events
public class AudioManager_DecoupledEvent : MonoBehaviour
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip brickDestroySound;
    [SerializeField] private AudioClip ballMissSound;
    [SerializeField] private AudioClip levelCompleteSound;
    [SerializeField] private AudioClip gameOverSound;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnEnable()
    {
        // Subscribe to events from different components
        Brick_DecoupledEvent.OnBrickHit += OnBrickDestroyed;
        ResetZone_DecoupledEvent.OnBallMissed += OnBallMissed;
        GameManager_DecoupledEvent.OnLevelCompleted += OnLevelCompleted;
        GameManager_DecoupledEvent.OnGameOver += OnGameOver;
    }

    private void OnDisable()
    {
        // Always unsubscribe to prevent memory leaks
        Brick_DecoupledEvent.OnBrickHit -= OnBrickDestroyed;
        ResetZone_DecoupledEvent.OnBallMissed -= OnBallMissed;
        GameManager_DecoupledEvent.OnLevelCompleted -= OnLevelCompleted;
        GameManager_DecoupledEvent.OnGameOver -= OnGameOver;
    }

    private void OnBrickDestroyed(Brick_DecoupledEvent brick, int points)
    {
        // Only play sound when brick is actually destroyed (health <= 0)
        // This would need to be modified based on your brick logic
        if (brickDestroySound != null && audioSource != null)
        {
            audioSource.PlayOneShot(brickDestroySound);
        }
    }

    private void OnBallMissed()
    {
        if (ballMissSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(ballMissSound);
        }
    }

    private void OnLevelCompleted(int levelNumber)
    {
        if (levelCompleteSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(levelCompleteSound);
        }
    }

    private void OnGameOver()
    {
        if (gameOverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(gameOverSound);
        }
    }
}
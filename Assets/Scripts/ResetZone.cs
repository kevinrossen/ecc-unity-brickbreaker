using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class ResetZone : MonoBehaviour
{
    [Header("UnityEvent (Inspector-wired, optional)")]
    public UnityEvent OnBallMissedUnityEvent; // Shows in Inspector

    // Event that other systems can subscribe to
    public static event System.Action OnBallMissed;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only react to the Ball object
        if (!other.CompareTag("Ball") && other.GetComponent<Ball>() == null)
        {
            return;
        }

        // Fire event - let other systems handle the response
        // This decouples ResetZone from knowing about lives, UI, audio, etc.
        OnBallMissedUnityEvent?.Invoke();
        OnBallMissed?.Invoke();
    }
}

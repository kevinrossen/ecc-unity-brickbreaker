using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ResetZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only react to the Ball object
        if (!other.CompareTag("Ball") && other.GetComponent<Ball>() == null)
        {
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBallMiss();
        }
        else
        {
            Debug.LogError("ResetZone: GameManager.Instance is null; cannot process ball miss.");
        }
    }

}

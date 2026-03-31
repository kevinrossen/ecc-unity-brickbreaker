using UnityEngine;

public class CreateObstacle : MonoBehaviour
{
    public static void CreateObstacleGameObject()
    {
        // Create a new empty GameObject
        GameObject obstacle = new GameObject("Obstacle");
        
        // Set position to (3, 0, 0)
        obstacle.transform.position = new Vector3(3f, 0f, 0f);
        
        // Add SpriteRenderer component
        SpriteRenderer spriteRenderer = obstacle.AddComponent<SpriteRenderer>();
        
        // Add BoxCollider2D component
        BoxCollider2D boxCollider = obstacle.AddComponent<BoxCollider2D>();
        
        Debug.Log("Obstacle GameObject created at position (3, 0, 0) with SpriteRenderer and BoxCollider2D");
    }
}
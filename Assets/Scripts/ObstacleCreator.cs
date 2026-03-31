using UnityEngine;

public class ObstacleCreator : MonoBehaviour
{
    /// <summary>
    /// Creates an Obstacle GameObject at position (3, 0, 0) with SpriteRenderer and BoxCollider2D
    /// </summary>
    public static GameObject CreateObstacle()
    {
        // Create empty GameObject
        GameObject obstacle = new GameObject("Obstacle");
        
        // Set position to (3, 0, 0)
        obstacle.transform.position = new Vector3(3f, 0f, 0f);
        
        // Add SpriteRenderer component
        SpriteRenderer spriteRenderer = obstacle.AddComponent<SpriteRenderer>();
        spriteRenderer.color = Color.white;
        
        // Add BoxCollider2D component
        BoxCollider2D boxCollider = obstacle.AddComponent<BoxCollider2D>();
        boxCollider.isTrigger = false;
        
        Debug.Log("Obstacle GameObject created successfully at position (3, 0, 0)");
        return obstacle;
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("GameObject/Create Obstacle")]
    private static void CreateObstacleMenuItem()
    {
        CreateObstacle();
    }
#endif
}
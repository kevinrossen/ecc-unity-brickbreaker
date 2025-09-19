using UnityEngine;

public class BrickDataAssigner : MonoBehaviour
{
    [Header("Brick Data Assets")]
    public BrickData basicBrick;
    public BrickData strongBrick;
    public BrickData weakBrick;
    public BrickData unbreakableBrick;
    
    [Header("Assignment Settings")]
    public bool assignBasicToRow1 = true;
    public bool assignStrongToRow2 = true;
    public bool assignWeakToRow3 = true;
    public bool makeLastBrickUnbreakable = true;
    
    [ContextMenu("Assign Brick Data")]
    public void AssignBrickData()
    {
        // Find all brick objects
        Brick[] allBricks = FindObjectsOfType<Brick>();
        
        Debug.Log($"Found {allBricks.Length} bricks to configure");
        
        foreach (Brick brick in allBricks)
        {
            // Get the row based on Y position
            float yPos = brick.transform.position.y;
            
            if (yPos > 7f && assignBasicToRow1) // Top row
            {
                brick.brickData = basicBrick;
                Debug.Log($"Assigned BasicBrick to {brick.name} at Y={yPos}");
            }
            else if (yPos > 6f && yPos <= 7f && assignStrongToRow2) // Second row
            {
                brick.brickData = strongBrick;
                Debug.Log($"Assigned StrongBrick to {brick.name} at Y={yPos}");
            }
            else if (yPos > 5f && yPos <= 6f && assignWeakToRow3) // Third row
            {
                brick.brickData = weakBrick;
                Debug.Log($"Assigned WeakBrick to {brick.name} at Y={yPos}");
            }
            else
            {
                // Assign basic to remaining bricks
                brick.brickData = basicBrick;
                Debug.Log($"Assigned BasicBrick (default) to {brick.name} at Y={yPos}");
            }
            
            // Make the rightmost brick in the top row unbreakable as an example
            if (makeLastBrickUnbreakable && yPos > 7f && brick.transform.position.x > 12f)
            {
                brick.brickData = unbreakableBrick;
                Debug.Log($"Made {brick.name} unbreakable");
            }
        }
        
        Debug.Log("Brick data assignment complete!");
    }
}

using UnityEngine;
using UnityEditor;
using TMPro;

public static class UISetupHelper
{
    [MenuItem("Tools/Brick Breaker/Setup Score UI")]
    public static void SetupScoreUI()
    {
        // Find the UI elements
        GameObject scoreTextObj = GameObject.Find("ScoreText");
        GameObject livesTextObj = GameObject.Find("LivesText");
        GameObject uiManagerObj = GameObject.Find("UI Manager");
        var hudPos = uiManagerObj != null ? uiManagerObj.GetComponent<HUDAutoPositioner>() : null;
        
        if (scoreTextObj != null)
        {
            // Configure ScoreText
            RectTransform scoreRect = scoreTextObj.GetComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0, 1);
            scoreRect.anchorMax = new Vector2(0, 1);
            scoreRect.pivot = new Vector2(0, 1);
            scoreRect.anchoredPosition = new Vector2(20, -20);
            // Slightly taller for two-line layout
            scoreRect.sizeDelta = new Vector2(300, 100);
            
            TextMeshProUGUI scoreText = scoreTextObj.GetComponent<TextMeshProUGUI>();
            scoreText.text = "Score:\n<align=center>0</align>";
            scoreText.fontSize = 32;
            scoreText.alignment = TextAlignmentOptions.TopLeft;
            scoreText.color = Color.white;
            
            Debug.Log("ScoreText configured!");
        }
        
        if (livesTextObj != null)
        {
            // Configure LivesText
            RectTransform livesRect = livesTextObj.GetComponent<RectTransform>();
            livesRect.anchorMin = new Vector2(1, 1);
            livesRect.anchorMax = new Vector2(1, 1);
            livesRect.pivot = new Vector2(1, 1);
            livesRect.anchoredPosition = new Vector2(-20, -20);
            // Slightly taller for two-line layout
            livesRect.sizeDelta = new Vector2(200, 100);
            
            TextMeshProUGUI livesText = livesTextObj.GetComponent<TextMeshProUGUI>();
            livesText.text = "Lives:\n<align=center>3</align>";
            livesText.fontSize = 32;
            livesText.alignment = TextAlignmentOptions.TopRight;
            livesText.color = Color.white;
            
            Debug.Log("LivesText configured!");
        }
        
        if (uiManagerObj != null && scoreTextObj != null && livesTextObj != null)
        {
            // Wire up the UIManager
            UIManager_DecoupledEvent uiManager = uiManagerObj.GetComponent<UIManager_DecoupledEvent>();
            if (uiManager != null)
            {
                SerializedObject serializedManager = new SerializedObject(uiManager);
                serializedManager.FindProperty("scoreText").objectReferenceValue = scoreTextObj.GetComponent<TextMeshProUGUI>();
                serializedManager.FindProperty("livesText").objectReferenceValue = livesTextObj.GetComponent<TextMeshProUGUI>();
                serializedManager.ApplyModifiedProperties();
                
                Debug.Log("UIManager wired up successfully!");
            }

            // Wire up the HUD auto positioner if present
            if (hudPos != null)
            {
                SerializedObject so = new SerializedObject(hudPos);
                so.FindProperty("scoreText").objectReferenceValue = scoreTextObj.GetComponent<TextMeshProUGUI>();
                so.FindProperty("livesText").objectReferenceValue = livesTextObj.GetComponent<TextMeshProUGUI>();
                so.ApplyModifiedProperties();
                Debug.Log("HUDAutoPositioner wired up successfully!");
            }
        }
        
        // Mark scene as dirty to save changes
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log("UI Setup Complete! Score display should now work in Play Mode.");
    }
}

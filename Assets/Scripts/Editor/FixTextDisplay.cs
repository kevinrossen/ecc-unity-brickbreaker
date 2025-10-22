using UnityEngine;
using UnityEditor;
using TMPro;

public static class FixTextDisplay
{
    [MenuItem("Tools/Brick Breaker/Fix Score Text Display")]
    public static void FixTextVisibility()
    {
        // Find the text objects
        GameObject scoreTextObj = GameObject.Find("ScoreText");
        GameObject livesTextObj = GameObject.Find("LivesText");
        
        // Load the default TMP font
        TMP_FontAsset defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (defaultFont == null)
        {
            // Try alternate path
            defaultFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        }
        
        if (defaultFont == null)
        {
            Debug.LogError("Could not find TMP default font! Make sure TextMesh Pro is imported (Window > TextMeshPro > Import TMP Essential Resources)");
            return;
        }
        
        if (scoreTextObj != null)
        {
            TextMeshProUGUI scoreText = scoreTextObj.GetComponent<TextMeshProUGUI>();
            if (scoreText != null)
            {
                Undo.RecordObject(scoreText, "Fix Score Text");
                scoreText.font = defaultFont;
                scoreText.fontSize = 48;
                scoreText.text = "Score:\n<align=center>0</align>";
                scoreText.color = Color.white;
                scoreText.alignment = TextAlignmentOptions.TopLeft;
                scoreText.textWrappingMode = TextWrappingModes.NoWrap;
                EditorUtility.SetDirty(scoreText);
                Debug.Log("ScoreText fixed - font assigned and size increased to 48!");
            }
        }
        
        if (livesTextObj != null)
        {
            TextMeshProUGUI livesText = livesTextObj.GetComponent<TextMeshProUGUI>();
            if (livesText != null)
            {
                Undo.RecordObject(livesText, "Fix Lives Text");
                livesText.font = defaultFont;
                livesText.fontSize = 48;
                livesText.text = "Lives:\n<align=center>3</align>";
                livesText.color = Color.white;
                livesText.alignment = TextAlignmentOptions.TopRight;
                livesText.textWrappingMode = TextWrappingModes.NoWrap;
                EditorUtility.SetDirty(livesText);
                Debug.Log("LivesText fixed - font assigned and size increased to 48!");
            }
        }
        
        // Force update
        Canvas.ForceUpdateCanvases();
        
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log("Text display fixed! Press Play to see the score.");
    }
}

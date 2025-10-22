using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;

public static class FixEventSystemInput
{
    [MenuItem("Tools/Brick Breaker/Fix EventSystem Input Module")]
    public static void FixInputModule()
    {
        EventSystem eventSystem = GameObject.FindFirstObjectByType<EventSystem>();
        
        if (eventSystem == null)
        {
            Debug.LogWarning("No EventSystem found in scene.");
            return;
        }

        // Remove old StandaloneInputModule
        StandaloneInputModule oldModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (oldModule != null)
        {
            Undo.DestroyObjectImmediate(oldModule);
            Debug.Log("Removed old StandaloneInputModule.");
        }

        // Add new InputSystemUIInputModule
        // First check if it already exists
        var existingNewModule = eventSystem.GetComponent(System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem"));
        
        if (existingNewModule == null)
        {
            // Try to add the new Input System UI module
            var inputSystemUIType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            
            if (inputSystemUIType != null)
            {
                Undo.AddComponent(eventSystem.gameObject, inputSystemUIType);
                Debug.Log("Added InputSystemUIInputModule for new Input System.");
            }
            else
            {
                Debug.LogWarning("InputSystemUIInputModule not found. Make sure the Input System package is installed.");
            }
        }
        else
        {
            Debug.Log("InputSystemUIInputModule already exists.");
        }

        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log("EventSystem Input Module fixed!");
    }
}

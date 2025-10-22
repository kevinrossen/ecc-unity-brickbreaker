using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapLoader : MonoBehaviour
{
    [Header("First Gameplay Scene Name")]
    public string firstSceneName = "Level1";

    void Start()
    {
        // Load the first gameplay scene additively so HUD persists
        SceneManager.LoadScene(firstSceneName, LoadSceneMode.Additive);
    }
}

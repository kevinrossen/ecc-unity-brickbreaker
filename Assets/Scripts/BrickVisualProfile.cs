using UnityEngine;

[CreateAssetMenu(fileName = "Brick Visual Profile", menuName = "Game Data/Brick Visual Profile")]
public class BrickVisualProfile : ScriptableObject
{
    [Header("Coloring when no per-health sprites are provided")]
    public Gradient strengthGradient;
    public Color unbreakableColor = Color.gray;
    [Tooltip("When enabled, Brick will use healthStates sprites if available and only fall back to colors when they are empty.")]
    public bool preferSpritesWhenAvailable = true;

    private void OnEnable()
    {
        // Initialize a classic gradient if none was set
        if (strengthGradient == null)
        {
            strengthGradient = new Gradient();
        }
        if (strengthGradient.colorKeys == null || strengthGradient.colorKeys.Length == 0)
        {
            strengthGradient.colorKeys = new[]
            {
                new GradientColorKey(Color.red, 0f),
                new GradientColorKey(new Color(1f, 0.5f, 0f), 0.33f), // Orange
                new GradientColorKey(Color.green, 0.66f),
                new GradientColorKey(Color.cyan, 1f)
            };
            strengthGradient.alphaKeys = new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            };
        }
    }
}

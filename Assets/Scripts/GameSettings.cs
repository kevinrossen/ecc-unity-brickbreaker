using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "BrickBreaker/Game Settings", order = 0)]
public class GameSettings : ScriptableObject
{
    [Header("Ball Settings")]
    [Min(0.1f)] public float ballSpeed = 10f;

    [Space]
    [Header("Paddle Settings")]
    [Min(0.1f)] public float paddleMoveSpeed = 50f;
    [Range(0f, 85f)] public float paddleMaxBounceAngle = 75f;

    [System.Serializable]
    public class LevelBuilderGroup
    {
        public bool applyToLevelBuilders = false;

        [Header("Source Data Overrides")]
        public bool useConfigPlacement = true;
        public bool useSpacingFromConfig = false;
        public bool autoCellSizeFromPrefab = true;
        public Vector2 startPosition = new Vector2(-7.5f, 4f);
        public float cellWidth = 1f;
        public float cellHeight = 0.5f;
        public float spacing = 0.1f;
        public bool groupByRows = true;

        [Header("Adaptive Placement Overrides")]
        public LevelBuilder.AnchorMode anchorMode = LevelBuilder.AnchorMode.MidpointPaddleToTop;
        public float verticalGapAbovePaddle = 2f;
        public bool clampToCameraViewport = true;
        public float cameraTopMargin = 2f;
        public float cameraSideMargin = 0.5f;
        public bool debugPlacement = false;
    }

    [Space]
    [Header("Level Builder Settings (Optional)")]
    public LevelBuilderGroup levelBuilder = new LevelBuilderGroup();
}

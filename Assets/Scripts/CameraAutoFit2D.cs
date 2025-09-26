using UnityEngine;

// Fits an orthographic camera to show the entire level bounded by the 2D walls
// Works in Edit and Play mode. Attach to your Main Camera.
[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class CameraAutoFit2D : MonoBehaviour
{
    public Transform leftWall;
    public Transform rightWall;
    public Transform topWall;
    public Transform bottomWall;

    [Tooltip("Extra world-units to pad around the computed bounds.")]
    public float padding = 0.5f;

    [Tooltip("Recenter the camera XY to the bounds center when fitting.")]
    public bool recenter = true;

    [Tooltip("Automatically fit on load and when values change in the editor.")]
    public bool autoFit = true;

    private Camera cam;

    private void OnEnable()
    {
        cam = GetComponent<Camera>();
        if (autoFit) FitNow();
    }

    private void OnValidate()
    {
        if (autoFit && isActiveAndEnabled)
        {
            if (cam == null) cam = GetComponent<Camera>();
            FitNow();
        }
    }

    [ContextMenu("Fit Now")]
    public void FitNow()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null || !cam.orthographic)
        {
            if (cam != null) cam.orthographic = true; // force orthographic for 2D
        }

        // Try auto-find by common names if not assigned
        AutoFindWallsIfNeeded();

        if (!TryGetBounds(out Bounds b))
        {
            Debug.LogWarning("CameraAutoFit2D: Could not compute bounds. Please assign wall transforms or ensure walls have BoxCollider2D components.");
            return;
        }

        // Calculate target size based on aspect ratio
        float aspect = Mathf.Max(0.0001f, cam.aspect);
        float halfHeight = b.size.y * 0.5f;
        float halfWidth = b.size.x * 0.5f;
        float sizeForWidth = (halfWidth) / aspect;
        float targetSize = Mathf.Max(halfHeight, sizeForWidth) + padding;
        targetSize = Mathf.Max(0.01f, targetSize);

        cam.orthographicSize = targetSize;

        if (recenter)
        {
            var pos = transform.position;
            transform.position = new Vector3(b.center.x, b.center.y, pos.z);
        }
    }

    private void AutoFindWallsIfNeeded()
    {
        if (leftWall == null) leftWall = FindByNames("Left Wall", "LeftWall");
        if (rightWall == null) rightWall = FindByNames("Right Wall", "RightWall");
        if (topWall == null) topWall = FindByNames("Top Wall", "TopWall");
        if (bottomWall == null) bottomWall = FindByNames("Bottom Wall", "BottomWall");
    }

    private Transform FindByNames(params string[] names)
    {
        foreach (var n in names)
        {
            var go = GameObject.Find(n);
            if (go != null) return go.transform;
        }
        return null;
    }

    private bool TryGetBounds(out Bounds b)
    {
        b = new Bounds(Vector3.zero, Vector3.zero);
        bool hasAny = false;
        bool ok;
        Bounds bb;

        ok = TryGetTransformBounds(leftWall, out bb);
        if (ok) { b = bb; hasAny = true; }
        ok = TryGetTransformBounds(rightWall, out bb);
        if (ok) { b = hasAny ? Encapsulate(b, bb) : (hasAny = true, bb).Item2; }
        ok = TryGetTransformBounds(topWall, out bb);
        if (ok) { b = hasAny ? Encapsulate(b, bb) : (hasAny = true, bb).Item2; }
        ok = TryGetTransformBounds(bottomWall, out bb);
        if (ok) { b = hasAny ? Encapsulate(b, bb) : (hasAny = true, bb).Item2; }

        return hasAny;
    }

    private static Bounds Encapsulate(Bounds a, Bounds b)
    {
        a.Encapsulate(b.min);
        a.Encapsulate(b.max);
        return a;
    }

    private bool TryGetTransformBounds(Transform t, out Bounds b)
    {
        b = new Bounds();
        if (t == null) return false;

        // Prefer collider bounds if present
        var col = t.GetComponent<Collider2D>();
        if (col != null)
        {
            b = col.bounds;
            return true;
        }
        var r = t.GetComponent<Renderer>();
        if (r != null)
        {
            b = r.bounds;
            return true;
        }
        // Fallback to a small point bound at transform position
        b = new Bounds(t.position, Vector3.one * 0.01f);
        return true;
    }
}

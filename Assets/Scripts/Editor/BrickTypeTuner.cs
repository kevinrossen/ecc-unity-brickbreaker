#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class BrickTypeTuner
{
    [MenuItem("Tools/Brick Breaker/Tune Default Brick Types")] 
    public static void TuneDefaultBrickTypes()
    {
        // Find BrickData assets
        string[] guids = AssetDatabase.FindAssets("t:BrickData");
        if (guids == null || guids.Length == 0)
        {
            EditorUtility.DisplayDialog("Tune Brick Types", "No BrickData assets found in project.", "OK");
            return;
        }

        // Helper: load by guid
        BrickData LoadBD(string guid) => AssetDatabase.LoadAssetAtPath<BrickData>(AssetDatabase.GUIDToAssetPath(guid));

        var all = guids.Select(LoadBD).Where(b => b != null).ToList();
        if (all.Count == 0)
        {
            EditorUtility.DisplayDialog("Tune Brick Types", "No BrickData assets found after load.", "OK");
            return;
        }

        // Map by name hints
        BrickData FindByName(params string[] keys)
        {
            foreach (var k in keys)
            {
                var match = all.FirstOrDefault(b => b != null && b.name.ToLowerInvariant().Contains(k.ToLowerInvariant()));
                if (match != null) return match;
            }
            return null;
        }

        var weak = FindByName("weak");
        var basic = FindByName("basic");
        var strong = FindByName("strong");
        var unbreak = FindByName("unbreak", "unbreakable");

        // Find sprites by expected names
        Sprite FindSprite(string name)
        {
            string[] spriteGuids = AssetDatabase.FindAssets($"{name} t:Sprite");
            foreach (var sg in spriteGuids)
            {
                var sp = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(sg));
                if (sp != null && sp.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                    return sp;
            }
            // Fallback: first containing name
            if (spriteGuids.Length > 0)
            {
                return AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(spriteGuids[0]));
            }
            return null;
        }

        var sBlue = FindSprite("Brick-Blue");
        var sGreen = FindSprite("Brick-Green");
        var sYellow = FindSprite("Brick-Yellow");
        var sOrange = FindSprite("Brick-Orange");
        var sRed = FindSprite("Brick-Red");

        // Helper to parse leading int
        int ParseLeadingInt(string s)
        {
            if (string.IsNullOrEmpty(s)) return 1;
            int i = 0;
            while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
            int start = i;
            while (i < s.Length && char.IsDigit(s[i])) i++;
            if (i > start && int.TryParse(s.Substring(start, i - start), out int value)) return value;
            return 1;
        }

        // A handy array for multi-state bricks (index 0 = full health)
        Sprite[] fiveState = new[] { sBlue, sGreen, sYellow, sOrange, sRed }.Where(s => s != null).ToArray();

        int updated = 0;

        foreach (var brick in all)
        {
            int brickTypeValue = ParseLeadingInt(brick.name);
            Undo.RecordObject(brick, $"Tune {brick.name}");
            if (brickTypeValue == 0)
            {
                // Unbreakable
                brick.isUnbreakable = true;
                brick.pointValue = 0;
                brick.healthStates = new Sprite[0];
                brick.brickColor = Color.gray;
            }
            else
            {
                // Breakable
                brick.isUnbreakable = false;
                brick.pointValue = brickTypeValue * 50; // e.g., 1=50, 2=100, etc.
                if (fiveState.Length > 0) brick.healthStates = fiveState;
                brick.brickColor = Color.Lerp(Color.red, Color.green, (brickTypeValue - 1f) / 7f); // gradient from red to green
            }
            EditorUtility.SetDirty(brick);
            updated++;
        }

        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("Tune Brick Types", updated > 0
            ? $"Updated {updated} BrickData asset(s) based on their names."
            : "No BrickData assets found.",
            "OK");
    }
}
#endif

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

        // A handy array for multi-state bricks (index 0 = full health)
        Sprite[] fiveState = new[] { sBlue, sGreen, sYellow, sOrange, sRed }.Where(s => s != null).ToArray();

        int updated = 0;

        if (weak != null)
        {
            Undo.RecordObject(weak, "Tune Weak Brick");
            weak.isUnbreakable = false;
            weak.maxHealth = 2;
            weak.pointValue = 50;
            weak.brickColor = Color.red; // fallback color when no sprites
            if (sRed != null) weak.healthStates = new[] { sRed };
            EditorUtility.SetDirty(weak);
            updated++;
        }

        if (basic != null)
        {
            Undo.RecordObject(basic, "Tune Basic Brick");
            basic.isUnbreakable = false;
            basic.maxHealth = 5;
            basic.pointValue = 100;
            if (fiveState.Length > 0) basic.healthStates = fiveState;
            basic.brickColor = new Color(1f, 0.5f, 0f); // orange-ish fallback
            EditorUtility.SetDirty(basic);
            updated++;
        }

        if (strong != null)
        {
            Undo.RecordObject(strong, "Tune Strong Brick");
            strong.isUnbreakable = false;
            strong.maxHealth = 8; // tougher than Basic
            strong.pointValue = 200;
            if (fiveState.Length > 0) strong.healthStates = fiveState;
            strong.brickColor = Color.green; // fallback
            EditorUtility.SetDirty(strong);
            updated++;
        }

        if (unbreak != null)
        {
            Undo.RecordObject(unbreak, "Tune Unbreakable Brick");
            unbreak.isUnbreakable = true;
            unbreak.maxHealth = 1;
            unbreak.pointValue = 0;
            unbreak.healthStates = new Sprite[0];
            unbreak.brickColor = Color.gray;
            EditorUtility.SetDirty(unbreak);
            updated++;
        }

        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("Tune Brick Types", updated > 0
            ? $"Updated {updated} BrickData asset(s).\nWeak/Basic/Strong assigned defaults and sprites when available; Unbreakable set to gray."
            : "No named BrickData assets matched Weak/Basic/Strong/Unbreak.",
            "OK");
    }
}
#endif

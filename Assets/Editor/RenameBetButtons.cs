using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public static class RenameBetButtons
{
    [MenuItem("Tools/Rename Bet Buttons (Pattern-Aware)")]
    public static void RunRenamer()
    {
        var all = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
                        .Where(t => t.name.StartsWith("Bet_"))
                        .ToList();

                        

        int totalRenamed = 0;

        foreach (var t in all)
        {
            string originalName = t.name;

            // Extract duplicate number in parentheses if exists
            var match = Regex.Match(originalName, @"^(Bet_.+?) ?\((\d+)\)$");
            string baseName = originalName;
            int duplicateIndex = 0;

            if (match.Success)
            {
                baseName = match.Groups[1].Value;
                duplicateIndex = int.Parse(match.Groups[2].Value);
            }

            // Extract prefix and base number (e.g. "Bet_Line", "1")
            var prefixMatch = Regex.Match(baseName, @"^(Bet_\w+?)_(\d+)$");
            if (!prefixMatch.Success)
            {
                Debug.LogWarning($"❌ Skipping: Could not parse '{originalName}'");
                continue;
            }

            string prefix = prefixMatch.Groups[1].Value;
            int baseIndex = int.Parse(prefixMatch.Groups[2].Value);
            int newIndex = baseIndex;

            // Apply rename logic based on prefix
            if (prefix.Contains("Corner_Line"))
            {
                newIndex = baseIndex + duplicateIndex;
            }
            else if (prefix.Contains("Line"))
            {
                newIndex = baseIndex + duplicateIndex;
            }
            else if (prefix.Contains("Corner"))
            {
                newIndex = baseIndex + duplicateIndex * 2;
            }
            else if (prefix.Contains("Edge"))
            {
                newIndex = baseIndex + duplicateIndex * 3;
            }

            string newName = $"{prefix}_{newIndex}";

            if (t.name != newName)
            {
                Undo.RecordObject(t, "Rename Bet Button");
                t.name = newName;
                totalRenamed++;
            }
        }

        Debug.Log($"✅ Renamed {totalRenamed} Bet_ objects using pattern-aware logic.");
    }
}

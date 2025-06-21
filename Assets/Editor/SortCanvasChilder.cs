using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

public class SortCanvasChildren
{
    [MenuItem("Tools/Sort Selected Object's Children by Name and Number")]
    public static void SortByPrefixAndNumber()
    {
        var parent = Selection.activeTransform;
        if (parent == null)
        {
            Debug.LogWarning("No object selected.");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(parent.gameObject, "Sort Children");

        var children = new Transform[parent.childCount];
        for (int i = 0; i < parent.childCount; i++)
            children[i] = parent.GetChild(i);

        System.Array.Sort(children, (a, b) =>
        {
            var (prefixA, numA) = SplitName(a.name);
            var (prefixB, numB) = SplitName(b.name);

            int prefixCompare = string.Compare(prefixA, prefixB);
            return prefixCompare != 0 ? prefixCompare : numA.CompareTo(numB);
        });

        for (int i = 0; i < children.Length; i++)
            children[i].SetSiblingIndex(i);

        Debug.Log($"✅ Sorted {children.Length} children by prefix and number.");
    }

    // Splits "Bet_Corner_3" into ("Bet_Corner", 3)
    private static (string prefix, int number) SplitName(string name)
    {
        name = Regex.Replace(name, @"\s*\(\d+\)$", ""); // remove " (n)" suffix
        var match = Regex.Match(name, @"^(.*?)(?:_(\d+))?$");

        string prefix = match.Groups[1].Value;
        int number = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : int.MaxValue;
        return (prefix, number);
    }
}

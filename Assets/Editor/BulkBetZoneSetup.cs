using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public static class BulkBetZoneSetup
{
    private static readonly Dictionary<string, (int[] numbers, int multiplier)> SpecialBets =
        new Dictionary<string, (int[], int)>()
    {
        { "Odd",    (Enumerable.Range(1,36).Where(n => n % 2 == 1).ToArray(), 2) },
        { "Even",   (Enumerable.Range(1,36).Where(n => n % 2 == 0).ToArray(), 2) },
        { "Red",    (new int[]{1,3,5,7,9,12,14,16,18,19,21,23,25,27,30,32,34,36}, 2) },
        { "Black",  (new int[]{2,4,6,8,10,11,13,15,17,20,22,24,26,28,29,31,33,35}, 2) },
        { "1st12",  (Enumerable.Range(1,12).ToArray(), 3) },
        { "2st12",  (Enumerable.Range(13,12).ToArray(), 3) },
        { "3st12",  (Enumerable.Range(25,12).ToArray(), 3) },
        { "1_34",   (Enumerable.Range(1,36).Where(n => (n - 1) % 3 == 0).ToArray(), 3) },
        { "2_35",   (Enumerable.Range(1,36).Where(n => (n - 2) % 3 == 0).ToArray(), 3) },
        { "3_36",   (Enumerable.Range(1,36).Where(n => (n - 3) % 3 == 0).ToArray(), 3) },
        { "1_18",   (Enumerable.Range(1,18).ToArray(), 2) },
        { "19_36",  (Enumerable.Range(19,18).ToArray(), 2) },
        { "0",      (new[]{0}, 35) }
    };

    [MenuItem("Tools/Setup BetZones on Buttons")]
    public static void SetupBetZones()
    {
        int added = 0, updated = 0, skipped = 0, failed = 0;

        var buttons = Object.FindObjectsByType<Button>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (var btn in buttons)
        {
            string name = btn.gameObject.name;
            if (!name.StartsWith("Bet_"))
            {
                skipped++;
                continue;
            }

            string key = name.Substring(4); // Remove "Bet_"
            int[] betNumbers = null;
            int multiplier = 1;

            // Check if it's a special named bet
            if (SpecialBets.TryGetValue(key, out var spec))
            {
                betNumbers = spec.numbers;
                multiplier = spec.multiplier;
            }
            else
            {
                Match match = Regex.Match(key, @"^(Edge|Corner_Line|Line|Corner)_(\d+)$");
                if (match.Success)
                {
                    string type = match.Groups[1].Value;
                    int index   = int.Parse(match.Groups[2].Value);

                    switch (type)
                    {
                        case "Edge":
                            
                            int a = index;
                            int b = index - 3;
                            if (b < 0) { b = 0; }
                            betNumbers = new[] { b, a };
                            multiplier = 17;
                            break;

                        case "Corner_Line":
                            if (index == 1)
                                betNumbers = new[] { 0, 1, 2, 3 };
                            else
                            {
                                int start = 1 + (index - 2) * 3;
                                betNumbers = Enumerable.Range(start, 6).ToArray();
                            }
                            multiplier = 5;
                            break;

                        case "Line":
                            int startLine = 1 + (index - 1) * 3;
                            betNumbers = new[] { startLine, startLine + 1, startLine + 2 };
                            multiplier = 11;
                            break;

                        case "Corner":
                        if (index == 1)
                        {
                            betNumbers = new[] { 0, 1, 2 };
                            multiplier = 11;
                        }
                        else if (index == 2)
                        {
                            betNumbers = new[] { 0, 2, 3 };
                            multiplier = 11;
                        }
                        else
                        {
                            // Corner_3 is 1,2,4,5; Corner_4 is 2,3,5,6; Corner_5 is 4,5,7,8; ...
                            int baseNum = 1;
                            for (int i = 3; i < index; i++)
                                baseNum += (i % 2 == 1) ? 1 : 2;

                            betNumbers = new[] { baseNum, baseNum + 1, baseNum + 3, baseNum + 4 };
                            multiplier = 8;
                        }
                        break;
                    }
                }
                else
                {
                    // Fallback: attempt to parse pure number list
                    string[] parts = key.Split('_');
                    bool ok = parts.All(p => int.TryParse(p, out _));
                    if (!ok)
                    {
                        Debug.LogWarning($"[Setup Failed] '{name}' is not a recognized bet format.");
                        failed++;
                        continue;
                    }

                    betNumbers = parts.Select(int.Parse).ToArray();
                    multiplier = DefaultMultiplierFor(betNumbers.Length);
                }
            }

            if (betNumbers == null)
            {
                Debug.LogWarning($"[Null Bet] Failed to resolve numbers for {name}");
                failed++;
                continue;
            }

            // Add or update BetZone component
            var bz = btn.GetComponent<BetZone>();
            if (bz == null)
            {
                Undo.AddComponent<BetZone>(btn.gameObject);
                bz = btn.GetComponent<BetZone>();
                added++;
            }
            else updated++;

            bz.coveredNumbers = betNumbers;
            bz.payoutMultiplier = multiplier;
        }

        Debug.Log($"✅ BetZone Setup Complete:\n→ Added: {added}\n→ Updated: {updated}\n→ Skipped: {skipped}\n→ Failed: {failed}");
    }

    private static int DefaultMultiplierFor(int count)
    {
        return count switch
        {
            1  => 35,
            2  => 17,
            3  => 11,
            4  => 8,
            6  => 5,
            12 => 3,
            18 => 2,
            36 => 1,
            _  => 1,
        };
    }
}

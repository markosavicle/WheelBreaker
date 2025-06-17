using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public static class BulkBetZoneSetup
{
    // Define all special bets here:
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
        { "1_34",   (Enumerable.Range(1,36).Where(n => n!=0 && (n-1)%3==0).ToArray(), 3) },
        { "2_35",   (Enumerable.Range(1,36).Where(n => n!=0 && (n-2)%3==0).ToArray(), 3) },
        { "3_36",   (Enumerable.Range(1,36).Where(n => n!=0 && (n-3)%3==0).ToArray(), 3) },
        { "1_18",   (Enumerable.Range(1,18).ToArray(), 2) },
        { "19_36",  (Enumerable.Range(19,18).ToArray(), 2) },
        { "0",      (new[]{0}, 35) }  // zero
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

            string key = name.Substring(4); // e.g. "Odd" or "1_2_3" or "1st12"
            int[] betNumbers;
            int multiplier;

            if (SpecialBets.TryGetValue(key, out var spec))
            {
                betNumbers  = spec.numbers;
                multiplier  = spec.multiplier;
            }
            else
            {
                // fallback to numeric parse
                string[] tokens = key.Split('_');
                bool ok = tokens.All(t => int.TryParse(t, out _));
                if (!ok)
                {
                    Debug.LogWarning($"[Setup Failed] '{name}' is not a recognized bet pattern.");
                    failed++;
                    continue;
                }
                betNumbers = tokens.Select(t => int.Parse(t)).ToArray();
                multiplier = DefaultMultiplierFor(betNumbers.Length);
            }

            // Add or get existing BetZone
            var bz = btn.GetComponent<BetZone>();
            if (bz == null)
            {
                Undo.AddComponent<BetZone>(btn.gameObject);
                bz = btn.GetComponent<BetZone>();
                added++;
            }
            else updated++;

            // Assign fields
            bz.coveredNumbers   = betNumbers;
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
            4  => 8,
            6  => 5,
            12 => 3,
            18 => 2,
            36 => 1,
            _  => 1,
        };
    }
}

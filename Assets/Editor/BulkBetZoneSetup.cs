using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class BulkBetZoneSetup
{
    [MenuItem("Tools/Setup BetZones on Buttons")]
    public static void SetupBetZones()
    {
        int added = 0, updated = 0, skipped = 0, failed = 0;

        // Find all Button components in the scene (active and inactive)
        var buttons = Object.FindObjectsByType<Button>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (var btn in buttons)
        {
            string goName = btn.gameObject.name;

            // Skip any GameObject that doesn't start with "Bet_"
            if (!goName.StartsWith("Bet_"))
            {
                skipped++;
                continue;
            }

            // Try to parse the GameObject name as a BetType
            if (System.Enum.TryParse(typeof(BetZone.BetType), goName, out var parsed))
            {
                var existing = btn.GetComponent<BetZone>();
                if (existing == null)
                {
                    Undo.AddComponent<BetZone>(btn.gameObject);
                    existing = btn.GetComponent<BetZone>();
                    added++;
                }
                else
                {
                    updated++;
                }

                existing.betType = (BetZone.BetType)parsed;

                // Optional: set a default multiplier if desired
                // existing.payoutMultiplier = DefaultMultiplierFor(existing.betType);
            }
            else
            {
                Debug.LogWarning($"[Setup Failed] Could not match GameObject '{goName}' to BetType enum.");
                failed++;
            }
        }

        Debug.Log($"✅ BetZone Setup Complete:\n→ Added: {added}\n→ Updated: {updated}\n→ Skipped (non-bet buttons): {skipped}\n→ Failed to parse: {failed}");
    }

    // Optional helper to assign multipliers based on BetType
    // private static float DefaultMultiplierFor(BetZone.BetType type)
    // {
    //     // Example logic
    //     return type.ToString().StartsWith("Bet_") ? 35f : 2f;
    // }
}

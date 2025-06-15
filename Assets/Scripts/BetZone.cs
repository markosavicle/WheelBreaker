using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BetZone : MonoBehaviour
{
    public enum BetType
    {
        Bet_0 = 0, Bet_1 = 1, Bet_2 = 2, Bet_3 = 3, Bet_4 = 4, Bet_5 = 5, Bet_6 = 6, Bet_7 = 7, Bet_8 = 8,
        Bet_9 = 9, Bet_10 = 10, Bet_11 = 11, Bet_12 = 12, Bet_13 = 13, Bet_14 = 14, Bet_15 = 15, Bet_16 = 16,
        Bet_17 = 17, Bet_18 = 18, Bet_19 = 19, Bet_20 = 20, Bet_21 = 21, Bet_22 = 22, Bet_23 = 23, Bet_24 = 24,
        Bet_25 = 25, Bet_26 = 26, Bet_27 = 27, Bet_28 = 28, Bet_29 = 29, Bet_30 = 30, Bet_31 = 31, Bet_32 = 32,
        Bet_33 = 33, Bet_34 = 34, Bet_35 = 35, Bet_36 = 36, Bet_Red, Bet_Black, Bet_Even,
        Bet_Odd, Bet_1st12, Bet_2st12, Bet_3st12, Bet_1_34, Bet_2_35, Bet_3_36,
        Bet_1_18, Bet_19_36
    }

    [Header("Bet Zone Settings")]
    public BetType betType;
    public int payoutMultiplier = 35;
    public GameObject chipPrefab;

    [Header("Slide Settings")]
    [Tooltip("Duration of the slide animation in seconds")] public float slideDuration = 0.5f;
    [Tooltip("Distance chips move along the table's forward axis on slide")] public float slideDistance = 1.0f;

    // Directions along the table's forward (z) axis
    [Tooltip("Direction chips move on a win - towards player (negative Z)")]
    public Vector3 winDirection = Vector3.back;
    [Tooltip("Direction chips move on a loss - away from player (positive Z)")]
    public Vector3 loseDirection = Vector3.forward;

    private Button button;
    private float chipHeight;
    private Vector3 basePosition;
    private List<GameObject> placedChips = new List<GameObject>();

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(OnClick);

        var meshFilter = chipPrefab.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
            chipHeight = meshFilter.sharedMesh.bounds.size.y * chipPrefab.transform.localScale.y;
        else
            chipHeight = 0.2f;

        basePosition = transform.position;
    }

    void OnClick()
    {
        var controller = FindFirstObjectByType<RouletteController>();
        controller.PlaceBet(betType, payoutMultiplier);

        Vector3 spawnPos = basePosition + Vector3.up * (chipHeight * placedChips.Count + chipHeight / 2f);
        GameObject chip = Instantiate(chipPrefab, spawnPos, Quaternion.identity);
        placedChips.Add(chip);
    }

    /// <summary>
    /// Animate and remove chips for a losing bet.
    /// </summary>
    public void CollectLoss()
    {
        StartCoroutine(AnimateChips(loseDirection));
    }

    /// <summary>
    /// Animate, spawn extra, and remove chips for a winning bet.
    /// </summary>
    public void CollectWin(int extraCount)
    {
        for (int i = 0; i < extraCount; i++)
        {
            Vector3 spawnPos = basePosition + Vector3.up * (chipHeight * placedChips.Count + chipHeight / 2f);
            GameObject chip = Instantiate(chipPrefab, spawnPos, Quaternion.identity);
            placedChips.Add(chip);
        }
        StartCoroutine(AnimateChips(winDirection));
    }

    private IEnumerator AnimateChips(Vector3 direction)
    {
        float elapsed = 0f;
        Vector3[] startPositions = new Vector3[placedChips.Count];
        Vector3 moveOffset = direction.normalized * slideDistance;
        for (int i = 0; i < placedChips.Count; i++)
            startPositions[i] = placedChips[i].transform.position;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideDuration;
            for (int i = 0; i < placedChips.Count; i++)
            {
                placedChips[i].transform.position = Vector3.Lerp(
                    startPositions[i],
                    startPositions[i] + moveOffset,
                    t
                );
            }
            yield return null;
        }

        foreach (var chip in placedChips)
            Destroy(chip);
        placedChips.Clear();
    }
}

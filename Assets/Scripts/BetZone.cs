using UnityEngine;
using UnityEngine.UI;

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

    private Button button;
    private float chipHeight;
    private Vector3 basePosition;
    private int placedCount = 0;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(OnClick);

        // Determine chip height in prefab's local space
        var meshFilter = chipPrefab.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            chipHeight = meshFilter.sharedMesh.bounds.size.y * chipPrefab.transform.localScale.y;
        }
        else
        {
            chipHeight = 0.2f;
        }

        // Cache the world-space base position
        basePosition = transform.position;
    }

    void OnClick()
    {
        // Place the bet via controller (deduct chip & update UI)
        var controller = FindFirstObjectByType<RouletteController>();
        controller.PlaceBet(betType, payoutMultiplier);

        // Calculate spawn position in world space (stack vertically)
        Vector3 spawnPos = basePosition + Vector3.up * (chipHeight * placedCount + chipHeight / 2f);

        // Instantiate without parenting under this transform to avoid inherited scale
        Instantiate(chipPrefab, spawnPos, Quaternion.identity);

        placedCount++;
    }
}

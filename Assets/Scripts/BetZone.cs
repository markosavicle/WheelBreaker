using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BetZone : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
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
    public float slideDuration = 0.5f;
    public float slideDistance = 1.0f;
    public Vector3 winDirection = Vector3.back;
    public Vector3 loseDirection = Vector3.forward;

    private RouletteController controller;
    private float chipHeight;
    private Vector3 basePosition;
    private List<GameObject> placedChips = new List<GameObject>();

    private bool isPointerOver = false;
    private bool isHolding = false;
    private float holdTimer = 0f;
    private float holdInterval = 0.25f;
    private bool isRightClick = false;

    void Awake()
    {
        controller   = FindFirstObjectByType<RouletteController>();

        var meshFilter = chipPrefab.GetComponent<MeshFilter>();
        chipHeight = (meshFilter != null && meshFilter.sharedMesh != null)
            ? meshFilter.sharedMesh.bounds.size.y * chipPrefab.transform.localScale.y
            : 0.2f;
        basePosition = transform.position;
    }

    void Update()
    {
        if (isPointerOver && isHolding)
        {
            holdTimer += Time.deltaTime;
            if (holdTimer >= holdInterval)
            {
                holdTimer = 0f;
                HandleBet();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding     = true;
        holdTimer     = 0f;
        isRightClick  = eventData.button == PointerEventData.InputButton.Right;
        HandleBet();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        isHolding     = false;
    }

    private void HandleBet()
    {
        if (isRightClick)
        {
            // remove chip
            if (placedChips.Count > 0)
            {
                Destroy(placedChips[placedChips.Count - 1]);
                placedChips.RemoveAt(placedChips.Count - 1);
                controller.RemoveBet(betType, payoutMultiplier);
            }
        }
        else
        {
            // place chip if possible
            if (controller.PlaceBet(betType, payoutMultiplier))
            {
                Vector3 spawnPos = basePosition +
                    Vector3.up * (chipHeight * placedChips.Count + chipHeight / 2f);
                var chip = Instantiate(chipPrefab, spawnPos, Quaternion.identity);
                placedChips.Add(chip);
            }
        }
    }

    public void CollectLoss()
    {
        StartCoroutine(AnimateChips(loseDirection));
    }

    public void CollectWin(int extraCount)
    {
        for (int i = 0; i < extraCount; i++)
        {
            Vector3 spawnPos = basePosition +
                Vector3.up * (chipHeight * placedChips.Count + chipHeight / 2f);
            var chip = Instantiate(chipPrefab, spawnPos, Quaternion.identity);
            placedChips.Add(chip);
        }
        StartCoroutine(AnimateChips(winDirection));
    }

    private IEnumerator AnimateChips(Vector3 direction)
    {
        float elapsed = 0f;
        Vector3[] starts = new Vector3[placedChips.Count];
        Vector3 offset = direction.normalized * slideDistance;
        for (int i = 0; i < placedChips.Count; i++)
            starts[i] = placedChips[i].transform.position;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideDuration;
            for (int i = 0; i < placedChips.Count; i++)
                placedChips[i].transform.position =
                    Vector3.Lerp(starts[i], starts[i] + offset, t);
            yield return null;
        }

        foreach (var c in placedChips)
            Destroy(c);
        placedChips.Clear();
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BetZone : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Bet Zone Settings")]
    [Tooltip("The pockets this zone covers (e.g. {1,2,3,4} for a line, {1,2,4,5} for a corner, or {5} for a single)")]
    public int[] coveredNumbers;
    [Tooltip("Payout multiplier for this zone (e.g. 35 for single, 5 for line, 8 for corner)")]
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
        controller = FindFirstObjectByType<RouletteController>();

        // compute chip height for stacking
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
                // you can tweak holdInterval here if you want acceleration
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding    = true;
        holdTimer    = 0f;
        isRightClick = eventData.button == PointerEventData.InputButton.Right;
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
            // Remove a chip from this zone
            if (placedChips.Count > 0)
            {
                Destroy(placedChips[placedChips.Count - 1]);
                placedChips.RemoveAt(placedChips.Count - 1);
                controller.RemoveBet(coveredNumbers);
            }
        }
        else
        {
            // Place a chip if you have chips left
            if (controller.PlaceBet(coveredNumbers, payoutMultiplier))
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

        foreach (var c in placedChips) Destroy(c);
        placedChips.Clear();
    }
}

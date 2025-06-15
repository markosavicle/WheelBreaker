using System.Collections;
using UnityEngine;

public class WheelSpinner : MonoBehaviour
{
    [Header("Slots Configuration")]
    [Tooltip("Number of pockets on the wheel (0–36 = 37 slots).")]
    public int slotCount = 37;

    [Tooltip("How many full rotations before slowing into place.")]
    public int extraSpins = 3;

    [Header("Spin Animation")]
    [Tooltip("Total spin duration (seconds).")]
    public float spinDuration = 2f;

    [Tooltip("Optional ease‑out curve (0=start, 1=end).")]
    public AnimationCurve easeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    // Whether a spin is in progress
    private bool isSpinning = false;

    /// <summary>
    /// Spins the wheel so that it lands on a specific slot index.
    /// </summary>
    /// <param name="targetSlot">Index 0..slotCount‑1</param>
    public void SpinToSlot(int targetSlot)
    {
        if (isSpinning) return;
        StartCoroutine(SpinRoutine(targetSlot));
    }

    private IEnumerator SpinRoutine(int targetSlot)
    {
        isSpinning = true;

        // Calculate total angle: full rotations + the specific slot
        float anglePerSlot = 360f / slotCount;
        float totalAngle = 360f * extraSpins + targetSlot * anglePerSlot;

        // Record start rotation
        float startAngle = transform.eulerAngles.y;
        float endAngle   = startAngle + totalAngle;

        float elapsed = 0f;
        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / spinDuration);
            // Apply ease‑out so it slows at the end
            float currentAngle = Mathf.Lerp(
                startAngle,
                endAngle,
                1f - easeOutCurve.Evaluate(t)
            );
            transform.eulerAngles = Vector3.up * currentAngle;
            yield return null;
        }

        // Ensure exact final alignment
        transform.eulerAngles = Vector3.up * endAngle;
        isSpinning = false;
    }
}

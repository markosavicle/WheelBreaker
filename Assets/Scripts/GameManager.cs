using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Starting Values")]
    public int startingChips = 10;
    public int chipValue = 1;        // base score per chip

    [Header("Runtime State (read-only)")]
    public int currentChips;
    public int currentScore;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        currentChips = startingChips;
        currentScore = 0;
    }

    /// <summary>
    /// Spend one chip to bet.
    /// Returns false if no chips left.
    /// </summary>
    public bool SpendChip()
    {
        if (currentChips <= 0) return false;
        currentChips--;
        return true;
    }

    /// <summary>
    /// Award score for a winning bet.
    /// payoutMultiplier = how many chips-worth of score you win per chip bet.
    /// </summary>
    public void AwardScore(int payoutMultiplier, int betAmount = 1)
    {
        int scoreGain = payoutMultiplier * betAmount * chipValue;
        currentScore += scoreGain;
    }
}

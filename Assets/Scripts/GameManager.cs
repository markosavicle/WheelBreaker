using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Starting Values")]
    public int startingChips   = 10;
    public int chipValue       = 1;  // score per chip unit

    [Header("Level & Goals")]
    public int startingLevel   = 1;
    public int baseScoreGoal   = 100;

    [Header("Persistent")]
    public int highestLevelReached = 1;

    [Header("Runtime State (read-only)")]
    public int currentChips;
    public int currentScore;
    public int currentLevel;
    public int currentScoreGoal;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load highest level
        highestLevelReached = PlayerPrefs.GetInt("HighLevel", startingLevel);

        // Initialize state
        ResetGame();
    }

    /// <summary>
    /// Spend one chip to place a bet. Returns false if no chips left.
    /// </summary>
    public bool SpendChip()
    {
        if (currentChips <= 0) return false;
        currentChips--;
        return true;
    }

    /// <summary>
    /// Refund one chip (called on bet removal).
    /// </summary>
    public void RefundChip()
    {
        currentChips++;
    }

    /// <summary>
    /// Award score and check for level completion.
    /// </summary>
    public void AwardScore(int scoreGain)
    {
        currentScore += scoreGain;
        if (currentScore >= currentScoreGoal)
            NextLevel();
    }

    /// <summary>
    /// Advance to next level, increase goal, persist if new high.
    /// </summary>
    private void NextLevel()
    {
        currentLevel++;
        currentScoreGoal += baseScoreGoal;

        if (currentLevel > highestLevelReached)
        {
            highestLevelReached = currentLevel;
            PlayerPrefs.SetInt("HighLevel", highestLevelReached);
        }
    }

    /// <summary>
    /// Reset all game state back to starting values.
    /// </summary>
    public void ResetGame()
    {
        currentChips     = startingChips;
        currentScore     = 0;
        currentLevel     = startingLevel;
        currentScoreGoal = baseScoreGoal;
    }
}

// GameManager.cs
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Starting Values")]
    public int startingChips = 10;
    public int chipValue = 1;  // score per chip unit

    [Header("Runtime State (read-only)")]
    public int currentChips;
    public int currentScore;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        currentChips = startingChips;
        currentScore = 0;
    }

    public bool SpendChip()
    {
        if (currentChips <= 0) return false;
        currentChips--;
        return true;
    }

    public void AwardScore(int scoreGain)
    {
        currentScore += scoreGain;
    }
}

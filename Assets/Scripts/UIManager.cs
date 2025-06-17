using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI chipsText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI betsText;
    public TextMeshProUGUI resultText;

    [Header("Level UI")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI goalText;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    [Tooltip("Large title text (e.g. 200pt)")]
    public TextMeshProUGUI gameOverMessage;
    [Tooltip("Details text (e.g. 50pt)")]
    public TextMeshProUGUI gameOverDetails;
    public Button resetButton;

    void Start()
    {
        resetButton.onClick.AddListener(OnResetClicked);

        gameOverPanel.SetActive(false);
        UpdateUI(GameManager.Instance.currentChips, GameManager.Instance.currentScore);
        UpdateLevelUI(
            GameManager.Instance.currentLevel,
            GameManager.Instance.currentScoreGoal
        );
        ClearBetsDisplay();
    }

    public void UpdateUI(int chips, int score)
    {
        chipsText.text = $"Chips: {chips}";
        scoreText.text = $"Score: {score}";
    }

    public void UpdateBetUI(List<string> betLabels, List<int> amounts, List<int> multipliers)
{
    if (betLabels == null || betLabels.Count == 0)
    {
        betsText.text = "No bets placed";
        return;
    }

    var sb = new StringBuilder();
    sb.AppendLine("Active Bets:");
    for (int i = 0; i < betLabels.Count; i++)
        sb.AppendLine($"{betLabels[i]} x{amounts[i]} @×{multipliers[i]}");
    betsText.text = sb.ToString();
}

    public void DisplayResult(string text)
    {
        resultText.text = text;
    }

    public void ClearBetsDisplay()
    {
        betsText.text = "No bets placed";
    }

    public void UpdateLevelUI(int level, int goal)
    {
        levelText.text = $"Level: {level}";
        goalText.text = $"Goal: {goal}";
    }

    /// <summary>
    /// Show the Game Over panel with a big title and smaller details.
    /// </summary>
    public void ShowGameOver(int finalScore, int level, int record)
    {
        gameOverPanel.SetActive(true);

        gameOverMessage.text = "Game Over";

        var sb = new StringBuilder();
        sb.AppendLine($"Score: {finalScore}");
        sb.AppendLine($"Level: {level}");
        sb.AppendLine($"Best: {record}");
        if (level > record)
            sb.AppendLine("NEW RECORD!");
        gameOverDetails.text = sb.ToString();
    }

    private void OnResetClicked()
    {
        GameManager.Instance.ResetGame();
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }
}
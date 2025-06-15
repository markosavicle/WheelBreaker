// UIManager.cs
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI chipsText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI betsText;
    public TextMeshProUGUI resultText;

    void Start()
    {
        UpdateUI( GameManager.Instance.currentChips, GameManager.Instance.currentScore );
        ClearBetsDisplay();
    }

    public void UpdateUI(int chips, int score)
    {
        chipsText.text = $"Chips: {chips}";
        scoreText.text = $"Score: {score}";
    }

    public void UpdateBetUI(List<BetZone.BetType> types, List<int> amounts, List<int> multipliers)
    {
        if (types.Count == 0)
        {
            betsText.text = "No bets placed";
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("Active Bets:");
        for (int i = 0; i < types.Count; i++)
            sb.AppendLine($"{types[i]} x{amounts[i]} @payout×{multipliers[i]}");
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
}

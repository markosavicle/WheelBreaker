// UIManager.cs
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI chipsText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI betsText;

    private GameManager gm;

    void Start()
    {
        gm = GameManager.Instance;
        UpdateUI();
        UpdateBetUI(new List<BetZone.BetType>(), new List<int>());
    }

    public void UpdateUI()
    {
        chipsText.text = $"Chips: {gm.currentChips}";
        scoreText.text = $"Score: {gm.currentScore}";
    }

    public void UpdateBetUI(List<BetZone.BetType> betTypes, List<int> multipliers)
    {
        if (betTypes == null || betTypes.Count == 0)
        {
            betsText.text = "No bets placed";
            return;
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Active Bets:");
        for (int i = 0; i < betTypes.Count; i++)
        {
            sb.AppendLine($"{betTypes[i]} x{multipliers[i]}");
        }
        betsText.text = sb.ToString();
    }
}

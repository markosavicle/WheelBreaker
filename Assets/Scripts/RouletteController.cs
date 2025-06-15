// RouletteController.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RouletteController : MonoBehaviour
{
    private class Bet
    {
        public BetZone.BetType type;
        public int payoutMultiplier;
        public int amount; // chips placed

        public Bet(BetZone.BetType t, int p) { type = t; payoutMultiplier = p; amount = 0; }
    }

    [Header("Refs")]
    public Button spinButton;
    public TextMeshProUGUI spinButtonLabel;
    public TextMeshProUGUI resultText;
    public WheelSpinner wheel;
    public UIManager ui;

    private List<Bet> bets = new List<Bet>();
    private System.Random rng = new System.Random();

    void Start()
    {
        spinButton.onClick.AddListener(OnSpinPressed);
        ui.UpdateUI(GameManager.Instance.currentChips, GameManager.Instance.currentScore);
        ui.ClearBetsDisplay();
    }

    public void PlaceBet(BetZone.BetType type, int multiplier)
    {
        if (!GameManager.Instance.SpendChip())
        {
            ui.DisplayResult("No chips left!");
            return;
        }

        // find or create the Bet entry
        var bet = bets.Find(b => b.type == type && b.payoutMultiplier == multiplier);
        if (bet == null)
        {
            bet = new Bet(type, multiplier);
            bets.Add(bet);
        }
        bet.amount++;

        // refresh UI
        ui.UpdateUI(GameManager.Instance.currentChips, GameManager.Instance.currentScore);
        ui.UpdateBetUI(
            bets.ConvertAll(b => b.type),
            bets.ConvertAll(b => b.amount),
            bets.ConvertAll(b => b.payoutMultiplier)
        );
    }

    void OnSpinPressed()
    {
        if (bets.Count == 0)
        {
            ui.DisplayResult("Place a bet first!");
            return;
        }

        spinButton.interactable = false;
        resultText.text = "Spinning…";

        int result = rng.Next(0, 37);
        string color = GetColor(result);
        string oddEven = result == 0 ? "Zero" : (result % 2 == 0 ? "Even" : "Odd");

        wheel.SpinToSlot(result);
        StartCoroutine(DelayedShowResult(result, color, oddEven));
    }

    IEnumerator DelayedShowResult(int number, string color, string oddEven)
    {
        yield return new WaitForSeconds(wheel.spinDuration);
        ShowResult(number, color, oddEven);
    }

    void ShowResult(int number, string color, string oddEven)
    {
        int totalScoreGain = 0;

        foreach (var bet in bets)
        {
            if (EvaluateBet(bet.type, number, color))
            {
                int gain = bet.amount * bet.payoutMultiplier * GameManager.Instance.chipValue;
                totalScoreGain += gain;
            }
        }

        if (totalScoreGain > 0)
            GameManager.Instance.AwardScore(totalScoreGain);

        string header = $"{number} {color} {oddEven}".Trim();
        string body = totalScoreGain > 0
            ? $"You won {totalScoreGain} score!"
            : "No winning bets.";

        ui.DisplayResult($"{header}\n{body}");

        // clear bets
        bets.Clear();
        ui.UpdateUI(GameManager.Instance.currentChips, GameManager.Instance.currentScore);
        ui.ClearBetsDisplay();
        spinButton.interactable = true;
    }

    private bool EvaluateBet(BetZone.BetType type, int number, string color)
    {
        int val = (int)type;
        if (val >= 0 && val <= 36) return val == number;
        switch (type)
        {
            case BetZone.BetType.Bet_Red:   return color == "Red";
            case BetZone.BetType.Bet_Black: return color == "Black";
            case BetZone.BetType.Bet_Even:  return number != 0 && number % 2 == 0;
            case BetZone.BetType.Bet_Odd:   return number % 2 == 1;
            case BetZone.BetType.Bet_1st12: return number >= 1 && number <= 12;
            case BetZone.BetType.Bet_2st12: return number >= 13 && number <= 24;
            case BetZone.BetType.Bet_3st12: return number >= 25 && number <= 36;
            case BetZone.BetType.Bet_1_34:  return (number - 1) % 3 == 0;
            case BetZone.BetType.Bet_2_35:  return (number - 2) % 3 == 0;
            case BetZone.BetType.Bet_3_36:  return (number - 3) % 3 == 0;
            case BetZone.BetType.Bet_1_18:  return number >= 1 && number <= 18;
            case BetZone.BetType.Bet_19_36: return number >= 19 && number <= 36;
            default: return false;
        }
    }

    private string GetColor(int number)
    {
        if (number == 0) return "Green";
        int[] reds = {1,3,5,7,9,12,14,16,18,19,21,23,25,27,30,32,34,36};
        return System.Array.IndexOf(reds, number) >= 0 ? "Red" : "Black";
    }
}

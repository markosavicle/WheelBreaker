using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;  // for SequenceEqual or Contains

public class RouletteController : MonoBehaviour
{
    private class Bet
    {
        public int[] numbers;
        public int payoutMultiplier;
        public int amount;
        public Bet(int[] nums, int mult) { numbers = nums; payoutMultiplier = mult; amount = 0; }
    }

    [Header("Refs")]
    public Button spinButton;
    public TextMeshProUGUI spinButtonLabel;
    public WheelSpinner wheel;
    public UIManager ui;

    private List<Bet> bets = new List<Bet>();
    private System.Random rng = new System.Random();

    void Start()
    {
        spinButton.onClick.AddListener(OnSpinPressed);
        RefreshUI();
    }

    /// <summary>Place one chip on a set of pockets. Returns false if no chips left.</summary>
    public bool PlaceBet(int[] numbers, int multiplier)
    {
        if (!GameManager.Instance.SpendChip())
        {
            ui.DisplayResult("No chips left!");
            return false;
        }

        var bet = bets.FirstOrDefault(b => b.numbers.SequenceEqual(numbers) && b.payoutMultiplier == multiplier);
        if (bet == null)
        {
            bet = new Bet(numbers, multiplier);
            bets.Add(bet);
        }
        bet.amount++;
        RefreshUI();
        return true;
    }

    /// <summary>Remove one chip from a set of pockets and refund it.</summary>
    public void RemoveBet(int[] numbers)
    {
        var bet = bets.FirstOrDefault(b => b.numbers.SequenceEqual(numbers));
        if (bet == null || bet.amount == 0) return;

        bet.amount--;
        GameManager.Instance.RefundChip();

        if (bet.amount == 0)
            bets.Remove(bet);

        RefreshUI();
    }

    private void OnSpinPressed()
    {
        if (bets.Count == 0)
        {
            ui.DisplayResult("Place a bet first!");
            return;
        }

        spinButton.interactable = false;
        ui.DisplayResult("Spinning…");

        int result    = rng.Next(0, 37);
        string color  = GetColor(result);
        string oddEven = (result == 0 ? "Zero" : (result % 2 == 0 ? "Even" : "Odd"));

        wheel.SpinToSlot(result);
        StartCoroutine(ShowResultDelayed(result, color, oddEven));
    }

    private IEnumerator ShowResultDelayed(int number, string color, string oddEven)
    {
        yield return new WaitForSeconds(wheel.spinDuration);
        ShowResult(number, color, oddEven);
    }

    private void ShowResult(int number, string color, string oddEven)
    {
        int totalScoreGain = 0;

        // Animate and calculate payouts
        foreach (var bet in bets)
        {
            bool won = bet.numbers.Contains(number);
            var zone = FindMatchingZone(bet.numbers);
            if (won)
            {
                int extra = bet.amount * (bet.payoutMultiplier - 1);
                zone.CollectWin(extra);

                int gain = bet.amount * bet.payoutMultiplier * GameManager.Instance.chipValue;
                totalScoreGain += gain;
                GameManager.Instance.AwardScore(gain);
            }
            else
            {
                zone.CollectLoss();
            }
        }

        ui.DisplayResult($"{number} {color} {oddEven}\n" +
            (totalScoreGain > 0 ? $"You won {totalScoreGain} score!" : "No winning bets."));

        bets.Clear();
        spinButton.interactable = true;
        RefreshUI();

        if (GameManager.Instance.currentChips <= 0)
            ui.ShowGameOver(
                GameManager.Instance.currentScore,
                GameManager.Instance.currentLevel,
                GameManager.Instance.highestLevelReached
            );
    }

    private void RefreshUI()
    {
        ui.UpdateUI(
            GameManager.Instance.currentChips,
            GameManager.Instance.currentScore
        );
        ui.UpdateLevelUI(
            GameManager.Instance.currentLevel,
            GameManager.Instance.currentScoreGoal
        );

        // Convert each bet’s numbers to a label like "1,2,3,4"
        var labels = bets.Select(b => string.Join(",", b.numbers)).ToList();
        var amounts = bets.Select(b => b.amount).ToList();
        var mults   = bets.Select(b => b.payoutMultiplier).ToList();

        ui.UpdateBetUI(labels, amounts, mults);
    }

    private BetZone FindMatchingZone(int[] numbers)
    {
        var allZones = Object.FindObjectsByType<BetZone>(
            FindObjectsInactive.Include, FindObjectsSortMode.None
        );
        return allZones.First(z => z.coveredNumbers.SequenceEqual(numbers));
    }

    private string GetColor(int number)
    {
        if (number == 0) return "Green";
        int[] reds = {1,3,5,7,9,12,14,16,18,19,21,23,25,27,30,32,34,36};
        return (System.Array.IndexOf(reds, number) >= 0) ? "Red" : "Black";
    }
}

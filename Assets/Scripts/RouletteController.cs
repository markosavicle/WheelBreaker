// RouletteController.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RouletteController : MonoBehaviour
{
    private struct Bet
    {
        public BetZone.BetType type;
        public int multiplier;
    }
    private List<Bet> activeBets = new List<Bet>();

    [Header("Refs")]
    public Button spinButton;
    public TextMeshProUGUI spinButtonLabel;
    public TextMeshProUGUI resultText;
    public WheelSpinner wheel;

    private GameManager gm;
    private UIManager ui;

    private int landedNumber;
    private string landedColor;

    void Start()
    {
        gm = GameManager.Instance;
        ui = FindFirstObjectByType<UIManager>();

        resultText.text = "Place your bet!";
        spinButtonLabel.text = "Bet & Spin";
        spinButton.onClick.AddListener(OnSpinPressed);

        ui.UpdateUI();
        // initialize bets display empty
        ui.UpdateBetUI(new List<BetZone.BetType>(), new List<int>());
    }

    /// <summary>
    /// Called by BetZone when a bet button is clicked.
    /// </summary>
    public void PlaceBet(BetZone.BetType type, int multiplier)
    {
        activeBets.Add(new Bet { type = type, multiplier = multiplier });
        // update with parallel lists
        var types = new List<BetZone.BetType>();
        var mults = new List<int>();
        foreach (var b in activeBets)
        {
            types.Add(b.type);
            mults.Add(b.multiplier);
        }
        ui.UpdateBetUI(types, mults);
    }

    void OnSpinPressed()
    {
        if (activeBets.Count == 0)
        {
            resultText.text = "Place a bet first!";
            return;
        }

        if (!gm.SpendChip())
        {
            resultText.text = "No chips left!";
            return;
        }

        ui.UpdateUI();
        spinButton.interactable = false;
        resultText.text = "Spinning…";

        landedNumber = Random.Range(0, 37);
        landedColor = (landedNumber == 0) ? "Green"
                       : (landedNumber % 2 == 0) ? "Black"
                       : "Red";

        wheel.SpinToSlot(landedNumber);
        Invoke(nameof(ShowResult), wheel.spinDuration);
    }

    void ShowResult()
    {
        int totalScoreGain = 0;
        bool anyWin = false;

        foreach (var bet in activeBets)
        {
            if (EvaluateBet(bet.type, landedNumber, landedColor))
            {
                anyWin = true;
                int gain = bet.multiplier * gm.chipValue;
                totalScoreGain += gain;
                gm.AwardScore(bet.multiplier);
            }
        }

        resultText.text = anyWin
            ? $"You won {totalScoreGain} score!"
            : "No winning bets.";

        // clear bets and update UI
        activeBets.Clear();
        ui.UpdateUI();
        ui.UpdateBetUI(new List<BetZone.BetType>(), new List<int>());

        spinButtonLabel.text = "Bet & Spin";
        spinButton.interactable = true;
    }

    private bool EvaluateBet(BetZone.BetType type, int number, string color)
    {
        if (number < 0 || number > 36) return false;

        int enumValue = (int)type;
        if (enumValue >= 0 && enumValue <= 36)
            return enumValue == number;

        switch (type)
        {
            // colors
            case BetZone.BetType.Bet_Red: return color == "Red";
            case BetZone.BetType.Bet_Black: return color == "Black";

            // even/odd
            case BetZone.BetType.Bet_Even: return number != 0 && number % 2 == 0;
            case BetZone.BetType.Bet_Odd: return number % 2 == 1;

            // dozens
            case BetZone.BetType.Bet_1st12: return number >= 1 && number <= 12;
            case BetZone.BetType.Bet_2st12: return number >= 13 && number <= 24;
            case BetZone.BetType.Bet_3st12: return number >= 25 && number <= 36;

            // columns
            case BetZone.BetType.Bet_1_34: return (number - 1) % 3 == 0;
            case BetZone.BetType.Bet_2_35: return (number - 2) % 3 == 0;
            case BetZone.BetType.Bet_3_36: return (number - 3) % 3 == 0;

            // high/low
            case BetZone.BetType.Bet_1_18: return number >= 1 && number <= 18;
            case BetZone.BetType.Bet_19_36: return number >= 19 && number <= 36;

            default:
                return false;
        }
    }
}

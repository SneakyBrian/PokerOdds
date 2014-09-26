using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HoldemHand;


namespace PokerOdds.HoldemOdds
{
    public class HoldemOddsCalculator
    {
        public async Task<TexasHoldemOdds> Calculate(TexasHoldemOdds odds, Action<TexasHoldemOdds> saveState)
        {
            var outcomes = new List<PokerOutcome>();

            for (var index = 0; index < Enum.GetNames(typeof(Hand.HandTypes)).Length; index++)
            {
                outcomes.Add(new PokerOutcome
                {
                    HandType = ((Hand.HandTypes)index).ToString(),
                    WinChance = 0
                });
            }

            odds.Outcomes = outcomes;

            if (saveState != null)
            {
                saveState(odds);
            }

            return await Task<TexasHoldemOdds>.Run(() =>
            {
                ulong playerMask = Hand.ParseHand(odds.Pocket);     // Player Pocket Cards
                ulong partialBoard = Hand.ParseHand(odds.Board);    // Partial Board

                // Calculate values for each hand type
                double[] playerWins = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
                double[] opponentWins = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };

                // Count of total hands examined.
                long count = 0;

                // Iterate through all possible opponent hands
                foreach (ulong opponentMask in Hand.Hands(0UL, partialBoard | playerMask, 2).AsParallel())
                {
                    // Iterate through all possible boards
                    foreach (ulong boardMask in Hand.Hands(partialBoard, opponentMask | playerMask, 5).AsParallel())
                    {
                        // Create a hand value for each player
                        uint playerHandValue = Hand.Evaluate(boardMask | playerMask, 7);
                        uint opponentHandValue = Hand.Evaluate(boardMask | opponentMask, 7);

                        // Calculate Winners
                        if (playerHandValue > opponentHandValue)
                        {
                            // Player Win
                            playerWins[Hand.HandType(playerHandValue)] += 1.0;
                        }
                        else if (playerHandValue < opponentHandValue)
                        {
                            // Opponent Win
                            opponentWins[Hand.HandType(opponentHandValue)] += 1.0;
                        }
                        else if (playerHandValue == opponentHandValue)
                        {
                            // Give half credit for ties.
                            playerWins[Hand.HandType(playerHandValue)] += 0.5;
                            opponentWins[Hand.HandType(opponentHandValue)] += 0.5;
                        }

                        count++;                       
                    }

                    for (var index = 0; index < playerWins.Length; index++)
                    {
                        outcomes[index].WinChance = playerWins[index] / ((double)count);
                    }

                    if (saveState != null)
                    {
                        saveState(odds);
                    }
                }

                odds.OverallWinSplitPercentage = odds.Outcomes.Sum(o => o.WinChance);
                odds.Completed = true;

                if (saveState != null)
                {
                    saveState(odds);
                }

                return odds;
            });
        }

        public static string SortCards(string cards)
        {
            var cardList = cards.Trim().ToLowerInvariant().Split(' ').ToList();

            cardList.Sort();

            return cardList.Aggregate(string.Empty, (a, b) => string.Format("{0} {1}", a.Trim(), b.Trim())).Trim();
        }
    }
}
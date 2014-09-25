using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web;
using HoldemHand;
using PokerOdds.Web.OWIN.Cache;
using PokerOdds.Web.OWIN.Interfaces;

namespace PokerOdds.Web.OWIN.Model
{
    public class HoldemOddsCalculator : IHasCache
    {
        public async Task<TexasHoldemOdds> Calculate(TexasHoldemOdds odds)
        {
            var outcomes = new List<PokerOutcome>();

            for (var index = 0; index < 9; index++)
            {
                outcomes.Add(new PokerOutcome
                {
                    HandType = (Hand.HandTypes)index,
                    HandTypeName = ((Hand.HandTypes)index).ToString(),
                    WinPercentage = 0
                });
            }

            odds.Outcomes = outcomes;

            Cache.Set(odds.GetCacheKey(), odds, CacheItemPolicy);

            return await Task<TexasHoldemOdds>.Run(() =>
            {
                ulong playerMask = Hand.ParseHand(odds.Pocket); // Player Pocket Cards
                ulong partialBoard = Hand.ParseHand(odds.Board);   // Partial Board

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

                        for (var index = 0; index < playerWins.Length; index++)
                        {
                            outcomes[index].WinPercentage = playerWins[index] / ((double)count) * 100.0;
                        }

                        Cache.Set(odds.GetCacheKey(), odds, CacheItemPolicy);
                    }
                }

                odds.OverallWinSplitPercentage = odds.Outcomes.Sum(o => o.WinPercentage);
                odds.Completed = true;

                Cache.Set(odds.GetCacheKey(), odds, CacheItemPolicy);

                return odds;
            });
        }

        public ObjectCache Cache { get; set; }

        public CacheItemPolicy CacheItemPolicy { get; set; }
    }
}
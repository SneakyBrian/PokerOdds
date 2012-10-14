using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HoldemHand;

namespace PokerOdds.Mvc.Web.Models.TexasHoldem
{
    public class TexasHoldemOdds
    {
        public string Pocket { get; set; }
        public string Board { get; set; }

        public PokerOutcome[] Outcomes { get; set; }

        public double OverallWinSplitPercentage { get; set; }
    }

    public class PokerOutcome
    {
        public Hand.HandTypes HandType { get; set; }
        public string HandTypeName { get; set; }
        public double WinPercentage { get; set; }
    }
}
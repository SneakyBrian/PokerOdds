using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using HoldemHand;

namespace PokerOdds.HoldemOdds
{
    [Serializable]
    public class TexasHoldemOdds
    {
        public string Pocket { get; set; }
        public string Board { get; set; }

        public IEnumerable<PokerOutcome> Outcomes { get; set; }

        public double OverallWinSplitChance { get; set; }

        public bool Completed { get; set; }

        public string GetCacheKey()
        {
            return string.Format("{0}-{1}", Pocket, Board);
        }

        public string GetETag()
        {
            var currentWinSplitChance = this.Outcomes.Sum(o => o.WinChance);

            using (var hashAlgo = SHA1Managed.Create())
            {
                return String.Concat("\"", Convert.ToBase64String(hashAlgo.ComputeHash(Encoding.UTF8.GetBytes(string.Format("{0}-{1}-{2}-{3}", Pocket, Board, currentWinSplitChance, Completed)))), "\"");
            }
        }
    }

    [Serializable]
    public class PokerOutcome
    {
        public string HandType { get; set; }
        public double WinChance { get; set; }
    }
}
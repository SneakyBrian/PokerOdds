using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ionic.Zip;
using Newtonsoft.Json;
using PokerOdds.HoldemOdds;

namespace PokerOdds.CachePrimer
{
    class Program
    {
        //generate the deck of cards
        private static readonly string[] _suits = new[] { "c", "d", "h", "s" };
        private static readonly string[] _values = new[] { "a", "2", "3", "4", "5", "6", "7", "8", "9", "t", "j", "q", "k" };

        private static string _outputPath;
        
        static void Main(string[] args)
        {
            _outputPath = args[0];

            Directory.CreateDirectory(_outputPath);

            var deck = GenerateDeck();
            var calculator = new HoldemOddsCalculator();

            var tasks = new List<Task>();

            for (int i = 0; i < deck.Count; i++)
            {
                for (int j = 0; j < deck.Count; j++)
                {
                    //must be different cards
                    if (i != j)
                    {
                        var odds = new TexasHoldemOdds { Pocket = HoldemOddsCalculator.SortCards(string.Format("{0} {1}", deck[i], deck[j])), Board = String.Empty };

                        Console.WriteLine("Generating odds for pocket {0}", odds.Pocket);

                        var calculateTask = calculator.Calculate(odds, o => Console.WriteLine("Updated odds for pocket {0}", o.Pocket));

                        calculateTask.ContinueWith(t => SaveOdds(t.Result));

                        tasks.Add(calculateTask);

                        //if we've got enough tasks to keep the processors busy
                        if (tasks.Count >= Environment.ProcessorCount)
                        {
                            //wait for any of them to finish
                            Task.WaitAny(tasks.ToArray(), Timeout.Infinite);
                            
                            //remove any completed tasks
                            int k = 0;
                            while ( k < tasks.Count)
                            {
                                if (tasks[k].IsCompleted)
                                {
                                    tasks.RemoveAt(k);
                                }
                                else
                                {
                                    k++;
                                }
                            }
                        }
                    }
                }
            }

            //Task.WaitAll(tasks.ToArray(), Timeout.Infinite);

            var zipPath = Path.Combine(_outputPath, "PrimeCache.zip");

            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            using (var outputArchive = new ZipFile(zipPath))
            {
                //outputArchive.AddProgress += (s, e) => Console.WriteLine("Adding entry {0}", e.CurrentEntry.FileName);
                outputArchive.SaveProgress += (s, e) => Console.WriteLine("Saving entry {0} of {1}", e.EntriesSaved, e.EntriesTotal);

                outputArchive.AddFiles(Directory.GetFiles(_outputPath, "*.json"));

                outputArchive.Save();
            }

            Console.WriteLine("Job Done");
        }

        private static void SaveOdds(TexasHoldemOdds odds)
        {
            Console.WriteLine("Saving generated odds for pocket {0}", odds.Pocket);

            var json = JsonConvert.SerializeObject(odds);

            File.WriteAllText(Path.Combine(_outputPath, string.Format("{0}.json", odds.GetCacheKey())), json);
        }

        private static List<string> GenerateDeck()
        {
            var deck = new List<string>();

            for (int i = 0; i < _suits.Length; i++)
            {
                for (int j = 0; j < _values.Length; j++)
                {
                    deck.Add(_values[j] + _suits[i]);
                }
            }

            return deck;
        }
    }
}

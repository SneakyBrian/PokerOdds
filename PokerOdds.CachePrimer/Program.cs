using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            Console.WriteLine(Assembly.GetExecutingAssembly().GetName().FullName);
            Console.WriteLine();

            if (args.Length < 1)
            {
                Console.WriteLine("Please specifiy output folder path");
                return;
            }

            _outputPath = args[0];

            Directory.CreateDirectory(_outputPath);

            bool overwrite = false;

            if (args.Length > 1)
            {
                if (args[1].Equals("/overwrite", StringComparison.InvariantCultureIgnoreCase))
                {
                    overwrite = true;
                }
            }

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

                        //if the file exists, and we are not overwriting, skip these cards
                        if (!overwrite && File.Exists(Path.Combine(_outputPath, string.Format("{0}.json", odds.GetCacheKey()))))
                        {
                            Console.WriteLine("Skipping odds for pocket {0}", odds.Pocket);
                            continue;
                        }

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

            if (tasks.Count > 0)
            {
                //wait for the remainder to complete
                Task.WaitAll(tasks.ToArray(), Timeout.Infinite);
            }

            var outPath = Path.Combine(_outputPath, "PrimeCache.json");

            if (File.Exists(outPath))
            {
                File.Delete(outPath);
            }

            using (var outputArchive = new StreamWriter(outPath))
            {
                bool firstItem = true;

                Directory.GetFiles(_outputPath, "*.json")
                    .Where(filePath => !filePath.Equals(outPath, StringComparison.InvariantCultureIgnoreCase))
                    .ToList().ForEach(filePath =>
                    {

                        if (firstItem)
                        {
                            outputArchive.Write("[");
                            firstItem = false;
                        }
                        else
                        {
                            outputArchive.WriteLine(",");
                        }

                        using (var inputStream = new StreamReader(filePath))
                        {
                            outputArchive.Write(inputStream.ReadToEnd());
                        }

                    });

                outputArchive.WriteLine("]");
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

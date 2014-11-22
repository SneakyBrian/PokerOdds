# Poker Odds API #

Poker Odds API is an example API built using ASP.NET OWIN. Given a Texas Holdem Poker hand (and optionally the community cards) it will calculate the odds of winning.

## How It Works ##

Poker Odds uses the excellent [PokerHandEval code written by Keith Rule](http://www.codeproject.com/Articles/12279/Fast-Texas-Holdem-Hand-Evaluation-and-Analysis) (based in turn on the [poker-eval library](http://pokersource.sourceforge.net/)). Users of the API submit information on their hand, and get back information on the possible outcomes of the game.

Cards are submitted as a sequence of two character codes separated by spaces. For example, to specify that your pocket cards are the two of diamonds and the queen of clubs, you would pass "2d qc" as the pocket parameter. If you wanted to pass 7 Hearts, Ace of Spades and 10 Hearts as the board, you would specify this as "7h as th". Note that the character "t" indicates 10.

## Live Demo ##

A live demo of the API is hosted at [http://pokerodds.azurewebsites.net/](http://pokerodds.azurewebsites.net/)

## Example ##

Requesting the following URL:

[http://pokerodds.azurewebsites.net/?pocket=2d%20qc&board=7h%20as%20th](http://pokerodds.azurewebsites.net/?pocket=2d%20qc&board=7h%20as%20th)

Results in the response:

	{
		"Pocket" : "2d qc",
		"Board" : "7h as th",
		"Outcomes" : [{
				"HandType" : "HighCard",
				"WinChance" : 0.070544587970861425
			}, {
				"HandType" : "Pair",
				"WinChance" : 0.18659517426273459
			}, {
				"HandType" : "TwoPair",
				"WinChance" : 0.0509451079156173
			}, {
				"HandType" : "Trips",
				"WinChance" : 0.0087375091396539109
			}, {
				"HandType" : "Straight",
				"WinChance" : 0.013022991307173612
			}, {
				"HandType" : "Flush",
				"WinChance" : 0.0
			}, {
				"HandType" : "FullHouse",
				"WinChance" : 0.0
			}, {
				"HandType" : "FourOfAKind",
				"WinChance" : 0.0
			}, {
				"HandType" : "StraightFlush",
				"WinChance" : 0.0
			}
		],
		"OverallWinSplitChance" : 0.0,
		"Completed" : false
	}

## Technology Used ##

- ASP.NET OWIN

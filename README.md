# Poker Odds API #

Poker Odds API is an example API built using ASP.NET MVC Web API. Given a Texas Holdem Poker hand (and optionally the community cards) it will calculate the odds of winning.

## How It Works ##

Poker Odds uses the excellent [PokerHandEval code written by Keith Rule](http://www.codeproject.com/Articles/12279/Fast-Texas-Holdem-Hand-Evaluation-and-Analysis) (based in turn on the [poker-eval library](http://pokersource.sourceforge.net/)). Users of the API submit information on their hand, and get back information on the possible outcomes of the game.

Cards are submitted as a sequence of two character codes separated by spaces. For example, to specify that your pocket cards are the two of diamonds and the queen of clubs, you would pass "2d qc" as the pocket parameter. If you wanted to pass 7 Hearts, Ace of Spades and 10 Hearts as the board, you would specify this as "7h as th". Note that the characer "t" indicates 10.

## Live Demo ##

A live demo is hosted at [http://pokerodds.azurewebsites.net/](http://pokerodds.azurewebsites.net/)

## Example ##

Requesting the following url (in Firefox or Chrome, which specify XML by default):

[http://pokerodds.azurewebsites.net/api/TexasHoldem/?pocket=2d%20qc&board=7h%20as%20th](http://pokerodds.azurewebsites.net//api/TexasHoldem/?pocket=2d%20qc&board=7h%20as%20th)

Results in the response:

`
<TexasHoldemOdds xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://schemas.datacontract.org/2004/07/PokerOdds.Mvc.Web.Models.TexasHoldem">
	<Board>7h as th</Board>
	<CalculationTimeMS>1678</CalculationTimeMS>
	<Completed>true</Completed>
	<Outcomes>
		<PokerOutcome>
			<HandType>HighCard</HandType>
			<HandTypeName>HighCard</HandTypeName>
			<WinPercentage>8.459619319933843</WinPercentage>
		</PokerOutcome>
		<PokerOutcome>
			<HandType>Pair</HandType>
			<HandTypeName>Pair</HandTypeName>
			<WinPercentage>20.051439464020408</WinPercentage>
		</PokerOutcome>
		<PokerOutcome>
			<HandType>TwoPair</HandType>
			<HandTypeName>TwoPair</HandTypeName>
			<WinPercentage>5.14609555312608</WinPercentage>
		</PokerOutcome>
		<PokerOutcome>
			<HandType>Trips</HandType>
			<HandTypeName>Trips</HandTypeName>
			<WinPercentage>0.90100823218307036</WinPercentage>
		</PokerOutcome>
		<PokerOutcome>
			<HandType>Straight</HandType>
			<HandTypeName>Straight</HandTypeName>
			<WinPercentage>1.326353264373616</WinPercentage>
		</PokerOutcome>
		<PokerOutcome>
			<HandType>Flush</HandType>
			<HandTypeName>Flush</HandTypeName>
			<WinPercentage>0</WinPercentage>
		</PokerOutcome>
		<PokerOutcome>
			<HandType>FullHouse</HandType>
			<HandTypeName>FullHouse</HandTypeName>
			<WinPercentage>0</WinPercentage>
		</PokerOutcome>
		<PokerOutcome>
			<HandType>FourOfAKind</HandType>
			<HandTypeName>FourOfAKind</HandTypeName>
			<WinPercentage>0</WinPercentage>
		</PokerOutcome>
		<PokerOutcome>
			<HandType>StraightFlush</HandType>
			<HandTypeName>StraightFlush</HandTypeName>
			<WinPercentage>0</WinPercentage>
		</PokerOutcome>
	</Outcomes>
	<OverallWinSplitPercentage>35.884515833637018</OverallWinSplitPercentage>
	<Pocket>2d qc</Pocket>
</TexasHoldemOdds>
`

Or the same url in IE results in a JSON formatted response:

`
{
	"Pocket" : "2d qc",
	"Board" : "7h as th",
	"Outcomes" : [{
			"HandType" : 0,
			"HandTypeName" : "HighCard",
			"WinPercentage" : 8.459619319933843
		}, {
			"HandType" : 1,
			"HandTypeName" : "Pair",
			"WinPercentage" : 20.051439464020408
		}, {
			"HandType" : 2,
			"HandTypeName" : "TwoPair",
			"WinPercentage" : 5.14609555312608
		}, {
			"HandType" : 3,
			"HandTypeName" : "Trips",
			"WinPercentage" : 0.90100823218307036
		}, {
			"HandType" : 4,
			"HandTypeName" : "Straight",
			"WinPercentage" : 1.326353264373616
		}, {
			"HandType" : 5,
			"HandTypeName" : "Flush",
			"WinPercentage" : 0.0
		}, {
			"HandType" : 6,
			"HandTypeName" : "FullHouse",
			"WinPercentage" : 0.0
		}, {
			"HandType" : 7,
			"HandTypeName" : "FourOfAKind",
			"WinPercentage" : 0.0
		}, {
			"HandType" : 8,
			"HandTypeName" : "StraightFlush",
			"WinPercentage" : 0.0
		}
	],
	"OverallWinSplitPercentage" : 35.884515833637018,
	"Completed" : true,
	"CalculationTimeMS" : 1816
}
`

## Technology Used ##

- ASP.NET MVC 4 Web API
- Knockout.js
- jQuery

namespace AdventOfCode22;

internal static class AdventOfCode22
{
    private static List<int> ParseCards(string hand)
    {
        return hand.Split('\n').Skip(1).Select(int.Parse).ToList();
    }

    private static T Pop<T>(this IList<T> l, int index)
    {
        var result = l[index];
        l.RemoveAt(index);
        return result;
    }
    
    private static List<int> PlayCombat(List<int> p1, List<int> p2)
    {
        while (p1.Count > 0 && p2.Count > 0)
        {
            var card1 = p1.Pop(0);
            var card2 = p2.Pop(0);

            if (card1 > card2)
            {
                p1.Add(card1);
                p1.Add(card2);
            }
            else
            {
                p2.Add(card2);
                p2.Add(card1);
            }
        }

        return (p1.Count > 0) ? p1 : p2;
    }

    private static (int, List<int>) PlayRecursiveCombat(List<int> p1, List<int> p2, ISet<(string, string)> hands,
        ref Dictionary<int, HashSet<(string, string)>> winCache)
    {
        while (p1.Count > 0 && p2.Count > 0)
        {
            var currHand = (string.Join(',', p1), string.Join(',', p2));
            
            // Check for cached game outcome.
            if (winCache[1].Contains(currHand))
            {
                return (1, p1);
            }
            else if (winCache[2].Contains(currHand))
            {
                return (2, p2);
            }

            // Check for instant round win due to repeat of a previous state.
            if (hands.Contains(currHand))
            {
                return (1, p1);
            }
            else
            {
                hands.Add(currHand);
            }

            // Determine winner of round via draw.
            var card1 = p1.Pop(0);
            var card2 = p2.Pop(0);
            int winner;
            if (p1.Count >= card1 && p2.Count >= card2)
            {
                (winner, _) = PlayRecursiveCombat(p1.Take(card1).ToList(), p2.Take(card2).ToList(), new HashSet<(string, string)>(), ref winCache);
            }
            else
            {
                winner = card1 > card2 ? 1 : 2;
            }

            // Place cards in end of winner's hand.
            if (winner == 1)
            {
                p1.Add(card1);
                p1.Add(card2);
            }
            else
            {
                p2.Add(card2);
                p2.Add(card1);
            }
        }

        var (thisWinner, thisWinningHand) = (p1.Count > 0) ? (1, p1) : (2, p2);
        winCache[thisWinner].UnionWith(hands);
        hands.Clear();
        
        return (thisWinner, thisWinningHand);
    }

    private static ulong ScoreHand(IReadOnlyList<int> hand)
    {
        var n = hand.Count;
        var score = 0ul;
        for (var i = 0; i < n; i++)
        {
            score += (ulong)(n - i) * (ulong)hand[i];
        }

        return score;
    }
    
    public static async Task Main()
    {
        var data = (await File.ReadAllTextAsync("input.txt")).Split("\n\n");
        var p1 = ParseCards(data[0]);
        var p2 = ParseCards(data[1]);

        var winnerA = PlayCombat(p1.ToList(), p2.ToList());
        var resultA = ScoreHand(winnerA);
        Console.WriteLine($"A: {resultA}");

        var wins = new Dictionary<int, HashSet<(string, string)>>()
        {
            [1] = new(),
            [2] = new()
        };
        var (_, winnerB) = PlayRecursiveCombat(p1, p2, new HashSet<(string, string)>(), ref wins);
        var resultB = ScoreHand(winnerB);
        Console.WriteLine($"B: {resultB}");
    }
}
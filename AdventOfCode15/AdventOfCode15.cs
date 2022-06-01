namespace AdventOfCode15;

internal static class AdventOfCode15
{
    public static async Task Main()
    {
        var data = (await File.ReadAllTextAsync("input.txt"))
            .Split(",")
            .Select(int.Parse)
            .ToArray();
        
        var spoken = data
            .Select((number, index) => (number, index))
            .ToDictionary(val => val.number, val => (val.index, val.index));

        var prev = data[^1];
        var turn = data.Length;
        var loopCount = 2020;

        elfNumbers:
        while (turn < loopCount)
        {
            var (l, ll) = spoken[prev];
            var curr = l - ll;
            spoken[curr] = (turn, spoken.TryGetValue(curr, out var v) ? v.Item1 : turn);
            prev = curr;
            turn++;
        }

        switch (loopCount)
        {
            case 2020:
                Console.WriteLine($"A: {prev}");
                loopCount = 30_000_000;
                goto elfNumbers;
            default:
                Console.WriteLine($"B: {prev}");
                break;
        }
    }
}
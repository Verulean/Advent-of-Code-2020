namespace AdventOfCode01;

internal static class AdventOfCode01
{
    public static async Task Main()
    {
        var expenses = (from entry in await File.ReadAllLinesAsync("input.txt") select int.Parse(entry)).ToHashSet();

        // Part A, two numbers that sum to 2020
        var result = 0;
        foreach (var a in expenses)
        {
            var b = 2020 - a;
            if (expenses.Contains(b))
            {
                result = a * b;
            }
        }
        Console.WriteLine($"A: {result}");

        // Part B, three numbers that sum to 2020
        result = 0;
        var found = false;
        foreach (var a in expenses)
        {
            foreach (var b in expenses)
            {
                var c = 2020 - a - b;
                if (a == b || b == c || a == c || !expenses.Contains(c)) continue;
                result = a * b * c;
                found = true;
                break;
            }
            if (found)
            {
                break;
            }
        }
        Console.WriteLine($"B: {result}");
    }
}

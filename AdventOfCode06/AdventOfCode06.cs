namespace AdventOfCode06;

internal static class AdventOfCode06
{
    private static int GroupScore(string group, int part = 1)
    {
        return part switch
        {
            1 => group.Split('\n')
                .Select(x => x.ToHashSet())
                .Aggregate(new HashSet<char>(), (acc, next) => new HashSet<char>(acc.Union(next)))
                .Count,
            2 => group.Split('\n')
                .Select(x => x.ToHashSet())
                .Aggregate(new HashSet<char>("abcdefghijklmnopqrstuvwxyz"),
                    (acc, next) => new HashSet<char>(acc.Intersect(next)))
                .Count,
            _ => throw new ArgumentException()
        };
    }
    
    public static async Task Main()
    {
        var groups = (await File.ReadAllTextAsync("input.txt")).Split("\n\n");
        var resultA = groups.Sum(x => GroupScore(x));
        var resultB = groups.Sum(x => GroupScore(x, 2));
        Console.WriteLine($"A: {resultA}\nB: {resultB}");
    }
}
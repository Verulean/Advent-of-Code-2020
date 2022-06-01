namespace AdventOfCode10;

internal static class AdventOfCode10
{
    private static int MergePermutations(int[] diffs)
    {
        var visited = new HashSet<string>();
        var q = new Queue<List<int>>();
        q.Enqueue(diffs.ToList());

        while (q.Count > 0)
        {
            var arr = q.Dequeue();
            var cArr = string.Join("", arr);
            if (visited.Contains(cArr))
            {
                continue;
            }

            visited.Add(cArr);
            for (var i = 0; i < arr.Count - 1; i++)
            {
                var merged = new List<int>(arr);
                merged[i] += merged[i + 1];
                if (merged[i] > 3)
                {
                    continue;
                }
                merged.RemoveAt(i + 1);
                q.Enqueue(merged);
            }
        }
        return visited.Count;
    }
    
    public static async Task Main()
    {
        var data = (await File.ReadAllLinesAsync("input.txt")).Select(int.Parse).ToArray();
        var jolts = data.Concat(new[] { 0, data.Max() + 3 }).ToArray();
        Array.Sort(jolts);
        var diffs = jolts.Zip(jolts.Skip(1)).Select(x => x.Second - x.First).ToArray();

        // A: product of number of pairs w/ difference 1 & difference 3
        var jolt1 = 0;
        var jolt3 = 0;
        foreach (var diff in diffs)
        {
            switch (diff)
            {
                case 1:
                    jolt1++;
                    break;
                case 3:
                    jolt3++;
                    break;
            }
        }
        Console.WriteLine($"A: {jolt1 * jolt3}");

        // B: total number of valid ways to chain adapters
        var resultB = string.Join("", diffs)
            .Split('3', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.ToCharArray().Select(x => x - '0').ToArray())
            .Aggregate(1ul, (acc, mergeSpan) => acc * (ulong) MergePermutations(mergeSpan));
        Console.WriteLine($"B: {resultB}");
    }
}
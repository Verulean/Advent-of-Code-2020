using System.Text.RegularExpressions;

namespace AdventOfCode07;

using BagSet = HashSet<Tuple<int, string>>;
using RuleDict = Dictionary<string, HashSet<Tuple<int, string>>>;

internal static class AdventOfCode07
{
    private static Tuple<string, BagSet> ParseLine(string line)
    {
        var elements = line.Split(new[] {"contain", "bag"}, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var source = elements.First().Trim();
        var dests = new BagSet();
        foreach (var elem in elements)
        {
            var number = Regex.Match(elem, @"\d+");
            if (!number.Success)
            {
                continue;
            }

            var destBag = elem.Substring(number.Index + number.Length).Trim();
            dests.Add(new Tuple<int, string>(int.Parse(number.ToString()), destBag));
        }

        return new Tuple<string, BagSet>(source, dests);
    }

    private static RuleDict ParseBagRules(string[] lines)
    {
        var d = new RuleDict();
        foreach (var line in lines)
        {
            var (src, dst) = ParseLine(line);
            if (d.ContainsKey(src))
            {
                d[src].UnionWith(dst);
            }
            else
            {
                d[src] = dst;
            }
        }

        return d;
    }

    private static Dictionary<string, HashSet<string>> InvertBagRules(RuleDict d)
    {
        var invDict = new  Dictionary<string, HashSet<string>>();
        foreach (var (src, dstBags) in d)
        {
            foreach (var (count, bag) in dstBags)
            {
                if (invDict.ContainsKey(bag))
                {
                    invDict[bag].Add(src);
                }
                else
                {
                    invDict[bag] = new HashSet<string>() { src };
                }
            }
        }

        return invDict;
    }

    public static async Task Main()
    {
        var data = await File.ReadAllLinesAsync("input.txt");
        var containRules = ParseBagRules(data);
        var containedRules = InvertBagRules(containRules);

        // A: Number of bags that contain shiny gold
        var validBags = new HashSet<string>();
        var checkedBags = new HashSet<string>();
        var qA = new Queue<string>();
        qA.Enqueue("shiny gold");
        while (qA.Count > 0)
        {
            var bag = qA.Dequeue();
            if (checkedBags.Contains(bag) || !containedRules.ContainsKey(bag))
            {
                continue;
            }
            else
            {
                checkedBags.Add(bag);
            }

            foreach (var contBag in containedRules[bag])
            {
                validBags.Add(contBag);
                qA.Enqueue(contBag);
            }
        }

        Console.WriteLine($"A: {validBags.Count}");

        // B: Total number of bags inside a shiny gold
        var totalBags = 0ul;
        var qB = new Queue<Tuple<ulong, string>>();
        qB.Enqueue(Tuple.Create(1ul, "shiny gold"));
        while (qB.Count > 0)
        {
            var (count, bag) = qB.Dequeue();
            totalBags += count;
            foreach (var (containedCount, containedBag) in containRules[bag])
            {
                qB.Enqueue(Tuple.Create(count * (ulong)containedCount, containedBag));
            }
        }

        totalBags--;
        Console.WriteLine($"B: {totalBags}");
    }
}
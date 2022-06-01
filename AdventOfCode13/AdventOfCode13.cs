using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AdventOfCode13;

internal static class AdventOfCode13
{
    public static int EarliestDeparture(int start, int bus)
    {
        var r = start % bus;
        return (start / bus) * bus + (r == 0 ? 0 : bus);
    }
    
    public static async Task Main()
    {
        var data = await File.ReadAllLinesAsync("input.txt");
        var startTime = int.Parse(data[0]);
        var buses = data[1].Split(",").Where(x => x != "x").Select(int.Parse).ToArray();

        int? bestBus = null;
        int? bestWait = null;
        foreach (var bus in buses)
        {
            var depart = EarliestDeparture(startTime, bus) - startTime;
            if (bestWait is null || depart < bestWait)
            {
                bestWait = depart;
                bestBus = bus;
            }
        }
        
        Console.WriteLine($"A: {bestBus * bestWait}");

        var busesB = new List<int[]>();
        var offset = 0;
        foreach (var elem in data[1].Split(","))
        {
            if (elem != "x")
            {
                busesB.Add(new[] {int.Parse(elem), offset});
            }
            offset++;
        }

        busesB = busesB.OrderByDescending(x => x[0]).ToList();

        var syncTime = 0ul;
        var increment = 1ul;
        while (busesB.Any(x => (syncTime + (ulong)x[1]) % (ulong)x[0] != 0))
        {
            for (var i = 0; i < busesB.Count; i++)
            {
                var id = busesB[i][0];
                var o = busesB[i][1];
                var r = (syncTime + (ulong)o) % (ulong)id;
                if (r == 0) continue;
                while ((syncTime + (ulong)o) % (ulong)id != 0)
                {
                    syncTime += increment;
                }
                increment *= (ulong) id;
                busesB.RemoveAt(i);
            }
        }
        
        Console.WriteLine($"B: {syncTime}");
    }
}
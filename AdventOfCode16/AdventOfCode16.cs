using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AdventOfCode16;

internal static class AdventOfCode16
{
    private readonly struct Interval
    {
        public Interval(string range)
        {
            var nums = range.Split("-");
            _low = int.Parse(nums[0]);
            _high = int.Parse(nums[1]);
        }

        public bool IsValid(int n)
        {
            return _low <= n && n <= _high;
        }

        private readonly int _low;
        private readonly int _high;
    }
    
    private class TicketValidator
    {
        public TicketValidator(string[] fields)
        {
            _fields = new Dictionary<string, Interval[]>();
            foreach (var field in fields)
            {
                var elements = field.Split(":");
                var name = elements[0];
                _fields[name] = elements[1]
                    .Split(" or ")
                    .Select(x => new Interval(x))
                    .ToArray();
            }
        }
        
        private Dictionary<string, Interval[]> _fields;

        public List<string> PossibleFields(int value)
        {
            var result = new List<string>();
            foreach (var (field, intervals) in _fields)
            {
                if (intervals.Any(i => i.IsValid(value)))
                {
                    result.Add(field);
                }
            }

            return result;
        }

        public IEnumerable<string> Fields => _fields.Keys;
    }

    private static int InvalidScore(TicketValidator validator, int[] ticket)
    {
        return ticket.Where(value => validator.PossibleFields(value).Count == 0).Sum();
    }

    private static (TicketValidator, int[], int[][]) ParseInput(string input)
    {
        var blocks = input
            .Split("\n\n")
            .Select(x => x.Split('\n'))
            .ToArray();
        var validator = new TicketValidator(blocks[0]);
        var yourTicket = blocks[1][1]
            .Split(",")
            .Select(int.Parse)
            .ToArray();
        var nearbyTickets = blocks[2][1..]
            .Select(line => line
                .Split(",")
                .Select(int.Parse)
                .ToArray()
            )
            .ToArray();

        return (validator, yourTicket, nearbyTickets);
    }
    
    public static async Task Main()
    {
        var data = (await File.ReadAllTextAsync("input.txt")).ReplaceLineEndings("\n");
        var (validator, yourTicket, nearbyTickets) = ParseInput(data);
        var resultA = InvalidScore(validator, yourTicket) + nearbyTickets.Sum(x => InvalidScore(validator, x));
        Console.WriteLine($"A: {resultA}");

        nearbyTickets = nearbyTickets
            .Where(x => x.All(entry => validator.PossibleFields(entry).Count > 0))
            .ToArray();
        var possibilities = new HashSet<string>[yourTicket.Length];
        for (var i = 0; i < possibilities.Length; i++)
        {
            possibilities[i] = validator.Fields.ToHashSet();
        }

        foreach (var ticket in nearbyTickets)
        {
            foreach (var (entry, i) in ticket.Select((e, i) => (e, i)))
            {
                possibilities[i].IntersectWith(validator.PossibleFields(entry).ToHashSet());
            }
        }

        var done = new HashSet<string>();
        while (possibilities.Any(p => p.Count > 1))
        {
            for (var i = 0; i < possibilities.Length; i++)
            {
                var p = possibilities[i];

                if (p.Count != 1 || p.IsSubsetOf(done)) continue;
                done.UnionWith(p);
                
                for (var j = 0; j < possibilities.Length; j++)
                {
                    if (j == i) continue;
                    possibilities[j].ExceptWith(p);
                }
            }
        }

        var fieldIds = possibilities.Select(x => x.Single()).ToArray();
        var resultB = 1ul;
        foreach (var (value, field) in yourTicket.Zip(fieldIds))
        {
            if (!field.StartsWith("departure")) continue;
            resultB *= (ulong)value;
        }
        
        Console.WriteLine($"B: {resultB}");
    }
}
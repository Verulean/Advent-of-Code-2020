using System.Numerics;

namespace AdventOfCode14;

internal static class AdventOfCode14
{
    private static (ulong, ulong, ulong) ParseMask(string mask)
    {
        var andMask = 0ul;
        var orMask = 0ul;
        var free = 0ul;
        foreach (var digit in mask)
        {
            andMask <<= 1;
            orMask <<= 1;
            free <<= 1;
            switch (digit)
            {
                case 'X':
                    andMask++;
                    free++;
                    break;
                case '1':
                    andMask++;
                    orMask++;
                    break;
            }
        }

        return (andMask, orMask, free);
    }

    private static IEnumerable<(ulong, ulong)> QuantumMask(ulong free)
    {
        var indices = new List<int>();
        var cardinality = 1ul << BitOperations.PopCount(free);
        var i = 0;
        while (free > 0ul)
        {
            var r = (int)(free % 2);
            free >>= 1;
            if (r != 0)
            {
                indices.Add(i);
            }

            i++;
        }
        
        for (var n = 0ul; n < cardinality; n++)
        {
            var andMask = ulong.MaxValue;
            var orMask = 0ul;

            var nn = n;
            foreach (var index in indices)
            {
                if (nn % 2 != 0)
                {
                    orMask |= 1ul << index;
                }
                else
                {
                    andMask &= ~(1ul << index);
                }

                nn >>= 1;
            }
            
            yield return (andMask, orMask);
        }
    }
    
    private class MaskedData
    {
        private Dictionary<int, ulong> _map = new();
        private ulong _andMask;
        private ulong _orMask;

        public void ExecuteLine(string line)
        {
            var elements = line.Split(" = ");
            if (elements[0] == "mask")
            {
                (_andMask, _orMask, _) = ParseMask(elements[1]);
            }
            else
            {
                var addr = int.Parse(elements[0][4..].TrimEnd(']'));
                var val = ulong.Parse(elements[1]);
                _map[addr] = val & _andMask | _orMask;
            }
        }

        public ulong Sum()
        {
            return _map.Values.Aggregate(0ul, (current, num) => current + num);
        }
    }
    
    private class MaskedAddress
    {
        private Dictionary<ulong, ulong> _map = new();
        private ulong _baseMask;
        private ulong _xs;

        public void ExecuteLine(string line)
        {
            var elements = line.Split(" = ");
            if (elements[0] == "mask")
            {
                (_, _baseMask, _xs) = ParseMask(elements[1]);
            }
            else
            {
                var addr = ulong.Parse(elements[0][4..].TrimEnd(']')) | _baseMask;
                var val = ulong.Parse(elements[1]);
                foreach (var (andMask, orMask) in QuantumMask(_xs))
                {
                    _map[addr & andMask | orMask] = val;
                }
            }
        }

        public ulong Sum()
        {
            return _map.Values.Aggregate(0ul, (current, num) => current + num);
        }
    }
    
    public static async Task Main()
    {
        var data = await File.ReadAllLinesAsync("input.txt");
        var memA = new MaskedData();
        var memB = new MaskedAddress();
        foreach (var line in data)
        {
            memA.ExecuteLine(line);
            memB.ExecuteLine(line);
        }
        
        Console.WriteLine($"A: {memA.Sum()}\nB: {memB.Sum()}");
    }
}
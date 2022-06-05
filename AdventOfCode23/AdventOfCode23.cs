using System.Globalization;

namespace AdventOfCode23;

internal static class AdventOfCode23
{
    private class CyclicArray
    {
        public CyclicArray(int[] arr)
        {
            _curr = arr[0];
            _length = arr.Length;
            _arr = new int[_length];
            for (var i = 0; i < _length; i++)
            {
                _arr[arr[i] - 1] = arr[(i + 1) % _length];
            }
        }

        private readonly int[] _arr;
        private readonly int _length;
        private int _curr;

        private int this[int curr]
        {
            get => _arr[curr - 1];
            set => _arr[curr - 1] = value;
        }

        public void Step()
        {
            // Extract 3 cups after current cup.
            var pick1 = this[_curr];
            var pick2 = this[pick1];
            var pick3 = this[pick2];

            // Find label of destination cup.
            var destLabel = _curr;
            do
            {
                destLabel = (destLabel <= 1) ? _arr.Length : destLabel - 1;
            } while (destLabel == pick1 || destLabel == pick2 || destLabel == pick3);

            // Shift 3 picked up cups to after the destination cup.
            this[_curr] = this[pick3];
            this[pick3] = this[destLabel];
            this[destLabel] = pick1;

            _curr = this[_curr];
        }
        
        public IEnumerable<int> AfterOne(int count = 0)
        {
            if (count <= 0) count = _length - 1;

            var curr = this[1];
            for (var i = 0; i < count; i++)
            {
                yield return curr;
                curr = this[curr];
            }
        }
    }
    
    public static async Task Main()
    {
        var data = await File.ReadAllTextAsync("input.txt");
        var cupLabels = data.Trim().Select(CharUnicodeInfo.GetDecimalDigitValue).ToArray();
        
        // A, 100 iterations of initial list
        var cups = new CyclicArray(cupLabels.ToArray());
        for (var step = 0; step < 100; step++)
        {
            cups.Step();
        }

        Console.WriteLine($"A: {string.Join("", cups.AfterOne())}");
        
        // B, 10_000_000 iterations of list extended to length 1_000_000
        var maxLabel = cupLabels.Max();
        var cupsB = new CyclicArray(
            cupLabels.Concat(Enumerable.Range(maxLabel + 1, 1_000_000 - maxLabel)).ToArray()
        );
        for (var step = 0; step < 10_000_000; step++)
        {
            cupsB.Step();
        }

        var resultB = cupsB.AfterOne(2).Aggregate(1ul, (acc, curr) => acc * (ulong)curr);
        Console.WriteLine($"B: {resultB}");
    }
}
namespace AdventOfCode02;

internal static class AdventOfCode02
{
    private class Password
    {
        public Password(string line)
        {
            var elements = line.Trim().Split(new char[] {' ', '-'}, StringSplitOptions.RemoveEmptyEntries);
            _minCount = Convert.ToInt32(elements[0]);
            _maxCount = Convert.ToInt32(elements[1]);
            _letter = elements[2][0];
            _entry = elements[3];
        }

        private readonly int _minCount;
        private readonly int _maxCount;
        private readonly char _letter;
        private readonly string _entry;

        public bool IsValid(int part = 1)
        {
            switch (part)
            {
                case 1:
                    var count = this._entry.Count(c => (c == this._letter));
                    return this._minCount <= count && count <= this._maxCount;
                case 2:
                    var c1 = this._entry[this._minCount - 1];
                    var c2 = this._entry[this._maxCount - 1];
                    return (c1 == this._letter) ^ (c2 == this._letter);
                default:
                    throw new ArgumentException("Part must be `1` or `2`");
            }
            
        }
    }

    public static async Task Main()
    {
        var passwords = (from entry in await File.ReadAllLinesAsync("input.txt") select new Password(entry)).ToList();

        // Count valid passwords
        var resultA = 0;
        var resultB = 0;
        foreach (var pw in passwords)
        {
            if (pw.IsValid())
            {
                resultA++;
            }
            if (pw.IsValid(2))
            {
                resultB++;
            }
        }
        Console.WriteLine($"A: {resultA}\nB: {resultB}");
    }
}

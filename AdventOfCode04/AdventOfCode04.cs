using System.Text.RegularExpressions;

namespace AdventOfCode04;

internal static class AdventOfCode04
{
    private class Passport
    {
        public Passport(string data)
        {
            _entries = data
                .Split(new[] {' ', '\n'}, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Split(':')).ToDictionary(x => x[0], x => x[1]);
        }

        private readonly Dictionary<string, string> _entries;
        private static readonly HashSet<string> Required = new() {"byr", "iyr", "eyr", "hgt", "hcl", "ecl", "pid"};
        private static readonly HashSet<string> EyeColors = new() {"amb", "blu", "brn", "gry", "grn", "hzl", "oth"};

        public bool IsValid(int part = 1)
        {
            if (!Required.All(_entries.ContainsKey))
            {
                return false;
            }

            return part != 2 || (
                (Regex.IsMatch(_entries["byr"], @"^\d{4}$") && int.Parse(_entries["byr"]) is >= 1920 and <= 2002) 
                && (Regex.IsMatch(_entries["iyr"], @"^\d{4}$") && int.Parse(_entries["iyr"]) is >= 2010 and <= 2020)
                && (Regex.IsMatch(_entries["eyr"], @"^\d{4}$") && int.Parse(_entries["eyr"]) is >= 2020 and <= 2030)
                && (Regex.IsMatch(_entries["hgt"], @"^\d+(cm|in)$") 
                    && ((_entries["hgt"].EndsWith("cm") && int.Parse(_entries["hgt"].Replace("cm", "")) is >= 150 and <= 193) 
                        || _entries["hgt"].EndsWith("in") && int.Parse(_entries["hgt"].Replace("in", "")) is >= 59 and <= 76)
                )
                && (Regex.IsMatch(_entries["hcl"], @"^#[0-9a-f]{6}$"))
                && (EyeColors.Contains(_entries["ecl"]))
                && (Regex.IsMatch(_entries["pid"], @"^\d{9}$"))
            );
        }
    }
    
    public static async Task Main()
    {
        var data = (await File.ReadAllTextAsync("input.txt")).Split("\n\n");
        var validCountA = 0;
        var validCountB = 0;
        foreach (var text in data)
        {
            var p = new Passport(text);
            if (p.IsValid())
            {
                validCountA++;
            }

            if (p.IsValid(2))
            {
                validCountB++;
            }
        }
        Console.WriteLine($"A: {validCountA}\nB: {validCountB}");
    }
}

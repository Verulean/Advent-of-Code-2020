namespace AdventOfCode19;

internal static class AdventOfCode19
{
    private static IEnumerable<string> StringCombinations(List<List<string>> possibilities)
    {
        switch (possibilities.Count)
        {
            case 0:
                yield break;
            case 1:
            {
                foreach (var elem in possibilities.Single())
                {
                    yield return elem;
                }

                yield break;
            }
            default:
            {
                foreach (var tail in StringCombinations(possibilities.GetRange(1, possibilities.Count - 1)))
                {
                    foreach (var elem in possibilities.First())
                    {
                        yield return elem + tail;
                    }
                }

                yield break;
            }
        }
    }
    
    private class Rule
    {
        public Rule(string line)
        {
            line = line.Trim();
            var elements = line.Split(": ");
            
            Id = int.Parse(elements[0]);
            Known = new HashSet<string>();
            _unknown = new List<List<int>>();
            
            if (elements[1].Contains('"'))
            {
                Known.Add(elements[1][^2].ToString());
            }
            else
            {
                foreach (var possibility in elements[1].Split(" | "))
                {
                    var ids = possibility
                        .Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToList();
                    _unknown.Add(ids);
                }
            }
        }

        public readonly int Id;
        public readonly HashSet<string> Known;
        private readonly List<List<int>> _unknown;

        public bool IsReduced => _unknown.Count == 0;

        private bool IsDetermined(IReadOnlyDictionary<int, HashSet<string>> knownIds)
        {
            return _unknown.All(elem => elem.All(knownIds.ContainsKey));
        }

        public bool TrySimplify(IReadOnlyDictionary<int, HashSet<string>> known)
        {
            if (!IsDetermined(known)) return false;
            
            foreach (var possibility in _unknown)
            {
                var substituted = possibility.Select(id => known[id].ToList()).ToList();
                foreach (var outcome in StringCombinations(substituted))
                {
                    Known.Add(outcome);
                }
            }

            _unknown.Clear();
            return true;
        }
    }

    private static Dictionary<int, HashSet<string>> GenerateDeterminedRules(Dictionary<int, Rule> rules,
        IEnumerable<int> requiredIds)
    {
        var known = new Dictionary<int, HashSet<string>>();
        
        var required = requiredIds as int[] ?? requiredIds.ToArray();
        while (required.Any(id => !known.ContainsKey(id)))
        {
            // Move fully determined rules to the known Dictionary.
            foreach (var rule in rules.Values.Where(rule => rule.IsReduced))
            {
                known[rule.Id] = rule.Known;
                rules.Remove(rule.Id);
            }
            
            // Fully simplify any outstanding rules, if possible.
            foreach (var rule in rules.Values)
            {
                rule.TrySimplify(known);
            }
        }

        rules.Clear();
        return known;
    }
    
    private class ValidatorB
    {
        public ValidatorB(IReadOnlyDictionary<int, HashSet<string>> known)
        {
            _rule31 = known[31];
            _rule42 = known[42];

            _length31 = _rule31.First().Length;
            _length42 = _rule42.First().Length;
        }

        private readonly HashSet<string> _rule31;
        private readonly int _length31;
        
        private readonly HashSet<string> _rule42;
        private readonly int _length42;
        
        public bool IsValid(string message)
        {
            var totalLength = message.Length;

            for (int remainder, repeats8 = 1; (remainder = totalLength - repeats8 * _length42) > 0; repeats8++)
            {
                if ((remainder % (_length42 + _length31)) != 0) continue;
                var repeats11 = remainder / (_length42 + _length31);

                // Rule 8 matches an arbitrary nonzero number of repetitions of Rule 42.
                var rule8Valid = Enumerable.Range(0, repeats8)
                    .All(n8 => 
                        _rule42.Contains(message.Substring(n8 * _length42, _length42))
                        );
                // Rule 11 matches an arbitrary nonzero number of repetitions of Rule 42
                // followed by the same number of Rule 31.
                var rule11Valid = Enumerable.Range(0, repeats11)
                    .All(n11 => 
                        _rule42.Contains(message.Substring((repeats8 + n11) * _length42, _length42))
                        && _rule31.Contains(message.Substring((repeats8 + repeats11) * _length42 + n11 * _length31, _length31))
                        );
                
                if (rule8Valid && rule11Valid)
                {
                    return true;
                }
            }

            return false;
        }
    }
    
    public static async Task Main()
    {
        var data = (await File.ReadAllTextAsync("input.txt")).Split("\n\n");
        var ruleText = data[0];
        var messages = data[1]
            .Split('\n')
            .Select(line => line.Trim())
            .ToArray();
        
        // Part A, original ruleset
        var rulesA = ruleText
            .Split('\n')
            .Select(line => new Rule(line))
            .ToDictionary(x => x.Id, x => x);
        var rule0A = GenerateDeterminedRules(rulesA, new[] { 0 })[0];
        var resultA = messages.Count(message => rule0A.Contains(message));
        Console.WriteLine($"A: {resultA}");
        
        // Part B, recursive ruleset
        var rulesB = ruleText
            .Replace("8: 42", "8: 42 | 42 8")
            .Replace("11: 42 31", "11: 42 31 | 42 11 31")
            .Split('\n')
            .Select(line => new Rule(line))
            .ToDictionary(x => x.Id, x => x);
        var knownB = GenerateDeterminedRules(rulesB, new[] { 31, 42 });
        var validatorB = new ValidatorB(knownB);
        var resultB = messages.Count(message => validatorB.IsValid(message));
        Console.WriteLine($"B: {resultB}");
    }
}

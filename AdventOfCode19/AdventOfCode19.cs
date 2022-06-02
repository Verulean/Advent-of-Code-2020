using System.Collections;

namespace AdventOfCode19;

internal static class AdventOfCode19
{
    private static IEnumerable<string> StringPermutations(List<List<object>> possibilities)
    {
        switch (possibilities.Count)
        {
            case 0:
                yield break;
            case 1:
            {
                foreach (var elem in possibilities.Single())
                {
                    yield return (string)elem;
                }

                yield break;
            }
            default:
            {
                foreach (var tail in StringPermutations(possibilities.GetRange(1, possibilities.Count - 1)))
                {
                    foreach (var elem in possibilities.First())
                    {
                        yield return (string)elem + tail;
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
            _unknown = new List<List<object>>();
            
            if (elements[1].Contains('"'))
            {
                Known.Add(elements[1][^2].ToString());
            }
            else
            {
                foreach (var possibility in elements[1].Split(" | "))
                {
                    _unknown.Add(possibility
                        .Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => (object)int.Parse(x))
                        .ToList()
                    );
                }
            }
        }
        
        public readonly int Id;
        public readonly HashSet<string> Known;
        private readonly List<List<object>> _unknown;

        public bool IsReduced => _unknown.Count == 0;

        public bool IsKnowable(ICollection<int> knownIds)
        {
            foreach (var elem in _unknown.SelectMany(x => x))
            {
                switch (elem)
                {
                    case int id:
                        if (!knownIds.Contains(id))
                        {
                            return false;
                        }

                        break;
                }
            }

            return true;
        }

        public void Simplify(IReadOnlyDictionary<int, HashSet<string>> known)
        {
            foreach (var possibility in _unknown)
            {
                for (var j = 0; j < possibility.Count; j++)
                {
                    switch (possibility[j])
                    {
                        case int otherId:
                            if (!known.TryGetValue(otherId, out var p)) continue;
                            possibility[j] = p.ToList();
                            break;
                    }
                }
            }

            foreach (var possibility in _unknown)
            {
                var goodLang = new List<List<object>>();
                foreach (var elem in possibility)
                {
                    if (elem is not IEnumerable enumerable) continue;
                    List<object> l = enumerable.Cast<object?>().ToList()!;
                    goodLang.Add(l);
                }
                foreach (var outcome in StringPermutations(goodLang))
                {
                    Known.Add(outcome);
                }
            }

            _unknown.Clear();
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
            
            // Fully simplify any outstanding rules, wherever possible.
            foreach (var rule in rules.Values.Where(rule => rule.IsKnowable(known.Keys)))
            {
                rule.Simplify(known);
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
        Console.Write($"B: {resultB}");
    }
}

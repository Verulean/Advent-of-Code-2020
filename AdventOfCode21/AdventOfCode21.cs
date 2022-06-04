namespace AdventOfCode21;

internal static class AdventOfCode21
{
    public static (HashSet<string>, HashSet<string>) ParseLine(string line)
    {
        var blocks = line.Trim().TrimEnd(')').Split(" (contains ");
        var ingredients = blocks[0].Split(' ').Select(x => x.Trim()).ToHashSet();
        var allergens = blocks[1].Split(", ").Select(x => x.Trim()).ToHashSet();
        return (ingredients, allergens);
    }
    
    public static async Task Main()
    {
        var data = (await File.ReadAllLinesAsync("input.txt")).Select(ParseLine).ToArray();

        // Determine possible ingredients containing each allergen.
        var allergenMap = new Dictionary<string, HashSet<string>>();
        foreach (var (ingredients, allergens) in data)
        {
            foreach (var allergen in allergens)
            {
                if (allergenMap.TryGetValue(allergen, out var possibilities))
                {
                    possibilities.IntersectWith(ingredients);
                }
                else
                {
                    allergenMap.Add(allergen, ingredients.ToHashSet());
                }
            }
        }

        // Cross-reference to identify the exact ingredient for each allergen.
        var allergenKey = new Dictionary<string, string>();
        while (allergenMap.Count > 0)
        {
            foreach (var (allergen, possibilities) in allergenMap)
            {
                if (possibilities.Count != 1) continue;
                allergenKey[allergen] = possibilities.Single();
                foreach (var (otherAllergen, otherPossibilities) in allergenMap)
                {
                    if (allergen == otherAllergen) continue;
                    otherPossibilities.ExceptWith(possibilities);
                }

                allergenMap.Remove(allergen);
            }
        }

        var potentialAllergens = allergenKey.Values.ToHashSet();

        var resultA = data
            .Sum(((HashSet<string> ingredients, HashSet<string> allergens) line) => 
                line.ingredients.Except(potentialAllergens).Count());
        Console.WriteLine($"A: {resultA}");
        
        var resultB = string.Join(
            ',',
            allergenKey.OrderBy(x => x.Key).Select(x => x.Value)
            );
        Console.WriteLine($"B: {resultB}");
    }
}
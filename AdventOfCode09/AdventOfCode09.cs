namespace AdventOfCode09;

internal static class AdventOfCode09
{
    private static bool IsValid(ref ulong[] numbers, int index)
    {
        for (var i = index - 25; i < index; i++)
        {
            for (var j = i + 1; j < index; j++)
            {
                if (numbers[i] + numbers[j] == numbers[index])
                {
                    return true;
                }
            }
        }

        return false;
    }
    
    public static async Task Main()
    {
        var numbers = (await File.ReadAllLinesAsync("input.txt")).Select(ulong.Parse).ToArray();
        
        // A: first invalid number
        ulong resultA = 0;
        for (var i = 25; i < numbers.Length; i++)
        {
            if (IsValid(ref numbers, i)) continue;
            resultA = numbers[i];
            break;
        }
        Console.WriteLine($"A: {resultA}");
        
        // B: encryption weakness (sum of extreme values in contiguous range that sums to number from A)
        for (var i = 0; i < numbers.Length - 1; i++)
        {
            var sum = numbers[i];
            var j = i + 1;
            while (sum < resultA)
            {
                sum += numbers[j];
                j++;
            }

            if (sum != resultA) continue;
            var weakness = numbers[i..j].Min() + numbers[i..j].Max();
            Console.WriteLine($"B: {weakness}");
            break;
        }
    }
}
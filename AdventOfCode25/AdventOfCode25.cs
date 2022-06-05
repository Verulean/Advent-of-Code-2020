namespace AdventOfCode25;

internal static class AdventOfCode25
{
    private static ulong Transform(ulong subject, int loopSize)
    {
        var r = 1ul;
        for (var i = 0; i < loopSize; i++)
        {
            r *= subject;
            r %= 2020_1227ul;
        }

        return r;
    }

    private static int Decrypt(ulong publicKey, ulong subject = 7)
    {
        var r = 1ul;
        var loopSize = 0;
        while (r != publicKey)
        {
            r *= subject;
            r %= 2020_1227ul;
            loopSize++;
        }

        return loopSize;
    }
    
    public static async Task Main()
    {
        var data = (await File.ReadAllLinesAsync("input.txt")).Select(ulong.Parse).ToArray();
        var resultA = Transform(data[1], Decrypt(data[0]));
        Console.WriteLine($"A: {resultA}");
    }
}

namespace AdventOfCode05;

internal static class AdventOfCode05
{
    private static int BinaryStringToInt(string s, char zero, char one)
    {
        return Convert.ToInt32(s.Replace(zero, '0').Replace(one, '1'), 2);
    }

    private static int SeatId(string seat)
    {
        var row = BinaryStringToInt(seat[..7], 'F', 'B');
        var col = BinaryStringToInt(seat[7..], 'L', 'R');
        return row * 8 + col;
    }
    
    public static async Task Main()
    {
        var seatIds = (await File.ReadAllLinesAsync("input.txt")).Select(SeatId).OrderBy(x => x).ToArray();
        var resultA = seatIds.Last();
        var resultB = Enumerable.Range(seatIds.First(), seatIds.Last() - seatIds.First()).Except(seatIds).Single();
        Console.WriteLine($"A: {resultA}\nB: {resultB}");
    }
}
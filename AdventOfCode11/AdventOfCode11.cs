namespace AdventOfCode11;
using Position = Tuple<int, int>;

internal static class AdventOfCode11
{
    private static readonly HashSet<char> Chairs = new() { 'L', '#' };

    private static readonly Position[] Directions = new[]
    {
        new Position(-1, -1),
        new Position(-1, 0),
        new Position(-1, 1),
        new Position(0, -1),
        new Position(0, 1),
        new Position(1, -1),
        new Position(1, 0),
        new Position(1, 1)
    };

    private static char[][] CopyGrid(char[][] grid)
    {
        return grid.Select(x => x.ToArray()).ToArray();
    }

    private static bool IsInBounds(int i, int j, int m, int n)
    {
        return (i >= 0 && i < m) && (j >= 0 && j < n);
    }

    private static int OccupiedSeats(int i, int j, char[][] grid, int m, int n, int part = 1)
    {
        var occ = 0;
        foreach (var (di, dj) in Directions)
        {
            var ii = i + di;
            var jj = j + dj;
            while (part == 2 && IsInBounds(ii + di, jj + dj, m, n) && !Chairs.Contains(grid[ii][jj]))
            {
                ii += di;
                jj += dj;
            }

            if (!IsInBounds(ii, jj, m, n))
            {
                continue;
            }

            if (grid[ii][jj] == '#')
            {
                occ++;
            }
        }

        return occ;
    }

    private static char[][] NextGrid(char[][] grid, int part = 1)
    {
        var m = grid.Length;
        var n = grid[0].Length;
        var newGrid = CopyGrid(grid);

        for (var i = 0; i < m; i++)
        {
            for (var j = 0; j < n; j++)
            {
                var curr = grid[i][j];
                if (curr == '.')
                {
                    continue;
                } 
                var occ = OccupiedSeats(i, j, grid, m, n, part);
                newGrid[i][j] = curr switch
                {
                    'L' when occ == 0 => '#',
                    '#' when occ >= (part == 1 ? 4 : 5) => 'L',
                    _ => newGrid[i][j]
                };
            }
        }

        return newGrid;
    }

    private static bool IsIdentical(IEnumerable<char[]> a, IEnumerable<char[]> b)
    {
        return a.Zip(b).All(x => x.First.SequenceEqual(x.Second));
    }

    private static int EquilibriumCount(ref char[][] grid, int part = 1)
    {
        var curr = CopyGrid(grid);
        var next = NextGrid(curr);
        while (!IsIdentical(next, curr))
        {
            curr = next;
            next = NextGrid(curr, part);
        }
        return next.Sum(x => x.Sum(y => y == '#' ? 1 : 0));
    }
    
    public static async Task Main()
    {
        var grid = (await File.ReadAllLinesAsync("input.txt")).Select(x => x.ToCharArray()).ToArray();
        Console.WriteLine($"A: {EquilibriumCount(ref grid)}");
        Console.WriteLine($"B: {EquilibriumCount(ref grid, 2)}");
    }
}
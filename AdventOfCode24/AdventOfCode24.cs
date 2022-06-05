namespace AdventOfCode24;

internal static class AdventOfCode24
{
    private static readonly string[] Directions = { "nw", "ne", "sw", "se", "w", "e" };

    private static (int, int) GetOffset(string direction)
    {
        return direction switch
        {
            "nw" => (-1, 1),
            "ne" => (1, 1),
            "sw" => (-1, -1),
            "se" => (1, -1),
            "w" => (-2, 0),
            "e" => (2, 0),
            _ => throw new ArgumentException($"Invalid neighbor direction '{direction}'.")
        };
    }
    
    private static (int, int) ParseLine(string line)
    {
        var x = 0;
        var y = 0;
        
        var i = 0;
        var length = line.Length;
        while (i < length)
        {
            foreach (var dir in Directions)
            {
                if (!line[i..].StartsWith(dir)) continue;
                var (dx, dy) = GetOffset(dir);
                x += dx;
                y += dy;
                i += dir.Length;
            }
        }

        return (x, y);
    }

    private static HashSet<(int, int)> Step(IReadOnlySet<(int, int)> curr)
    {
        var next = new HashSet<(int, int)>();
        var visited = new HashSet<(int, int)>();
        var q = new Queue<(int, int)>();
        foreach (var pos in curr)
        {
            q.Enqueue(pos);
        }
        
        while (q.Count > 0)
        {
            var pos = q.Dequeue();
            if (visited.Contains(pos)) continue;
            visited.Add(pos);

            // Count number of black neighboring tiles, and add them to the queue.
            var (x, y) = pos;
            var blackNeighbors = 0;
            foreach (var dir in Directions)
            {
                var (dx, dy) = GetOffset(dir);
                var neighborPos = (x + dx, y + dy);
                
                if (curr.Contains(neighborPos))
                {
                    blackNeighbors++;
                }

                if (curr.Contains(pos))
                {
                    q.Enqueue(neighborPos);
                }
            }

            // Set current tile's state to black if rule satisfied.
            if (
                (curr.Contains(pos) && blackNeighbors is 1 or 2)
                || (!curr.Contains(pos) && blackNeighbors == 2)
                )
            {
                next.Add(pos);
            }
        }
        
        return next;
    }

    public static async Task Main()
    {
        var data = await File.ReadAllLinesAsync("input.txt");
        var blackTiles = new HashSet<(int, int)>();
        foreach (var line in data)
        {
            var pos = ParseLine(line);
            if (blackTiles.Contains(pos))
            {
                blackTiles.Remove(pos);
            }
            else
            {
                blackTiles.Add(pos);
            }
        }
        
        Console.WriteLine($"A: {blackTiles.Count}");

        for (var day = 0; day < 100; day++)
        {
            blackTiles = Step(blackTiles);
        }
        Console.WriteLine($"B: {blackTiles.Count}");
    }
}
using System.Collections;
using System.Net;
using System.Xml.Xsl;

namespace AdventOfCode20;

internal static class AdventOfCode20
{
    private static bool[] OrientMatches(bool[] matches, (bool flip, int rotations) orientation)
    {
        var rotations = (orientation.rotations % 4 + 4) % 4;
        var tempMatches = matches.ToArray();
        // top, bottom, left, right
        if (orientation.flip)
        {
            (tempMatches[0], tempMatches[1]) = (tempMatches[1], tempMatches[0]);
        }

        var newMatches = new bool[matches.Length];
        switch (rotations)
        {
            case 0:
                newMatches = tempMatches;
                break;
            case 3:
                newMatches[0] = tempMatches[2];
                newMatches[1] = tempMatches[3];
                newMatches[2] = tempMatches[1];
                newMatches[3] = tempMatches[0];
                break;
            case 2:
                newMatches[0] = tempMatches[1];
                newMatches[1] = tempMatches[0];
                newMatches[2] = tempMatches[3];
                newMatches[3] = tempMatches[2];
                break;
            case 1:
                newMatches[0] = tempMatches[3];
                newMatches[1] = tempMatches[2];
                newMatches[2] = tempMatches[0];
                newMatches[3] = tempMatches[1];
                break;
        }

        return newMatches;
    }
    
    private class CharGrid
    {
        public static readonly List<(bool, int)> Orientations = new()
        {
            (false, 0), (false, 1), (false, 2), (false, 3),
            (true, 0), (true, 1), (true, 2), (true, 3)
        };
        
        private enum Axis
        {
            X = 0,
            Y = 1
        }
        
        public CharGrid(char[,] tile, int n)
        {
            _startGrid = tile;
            Grid = _startGrid;
            N = n;
        }

        public static char[,] Rotate(char[,] grid, int n, int rotations)
        {
            rotations = (rotations % 4 + 4) % 4;
            
            var newGrid = new char[n, n];
            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < n; j++)
                {
                    newGrid[i, j] = grid[i, j];
                }
            }
            switch (rotations)
            {
                case 0:
                    return newGrid;
                case 1:
                {
                    for (var i = 0; i < n; i++)
                    {
                        for (var j = 0; j < n; j++)
                        {
                            newGrid[i, j] = grid[j, n - i - 1];
                        }
                    }

                    return newGrid;
                }
                default:
                {
                    for (var r = 0; r < rotations; r++)
                    {
                        newGrid = Rotate(newGrid, n, 1);
                    }

                    return newGrid;
                }
            }
        }

        public void SetOrientation(int rotations = 0, bool flip = false)
        {
            char[,] temp;
            if (flip)
            {
                temp = new char[N, N];
                for (var i = 0; i < N; i++)
                {
                    for (var j = 0; j < N; j++)
                    {
                        temp[i, j] = _startGrid[N - 1 - i, j];
                    }
                }
            }
            else
            {
                temp = _startGrid;
            }
            
            Grid = Rotate(temp, N, rotations);
            Orientation = (flip, rotations);
        }

        private char[] GetEdge(Axis fixedAxis, int fixedIndex)
        {
            var edge = new char[N];
            switch (fixedAxis)
            {
                case Axis.X:
                {
                    for (var i = 0; i < N; i++)
                    {
                        edge[i] = Grid[i, fixedIndex];
                    }

                    break;
                }
                case Axis.Y:
                {
                    for (var j = 0; j < N; j++)
                    {
                        edge[j] = Grid[fixedIndex, j];
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(fixedAxis), fixedAxis, "Axis index out of bounds for 2D CharGrid.");
            }

            return edge;
        }
        
        private char[,] _startGrid;
        public char[,] Grid;
        public int N { get; }
        public (bool flip, int rot) Orientation = (false, 0);

        public char[] Left => GetEdge(Axis.X, 0);
        public char[] Right => GetEdge(Axis.X, N - 1);
        public char[] Top => GetEdge(Axis.Y, 0);
        public char[] Bottom => GetEdge(Axis.Y, N - 1);
        public char[][] Edges => new[] { Top, Bottom, Left, Right };
    }
    
    private class ImageTile
    {
        public enum TileType
        {
            Corner = 2,
            Edge = 3,
            Center = 4
        }

        public ImageTile(string input)
        {
            var lines = input.Split('\n');
            var n = lines.Length - 1;

            Id = int.Parse(lines[0].TrimEnd(':').Split(' ')[^1]);
            var grid = new char[n, n];
            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < n; j++)
                {
                    grid[i, j] = lines[i + 1][j];
                }
            }

            Tile = new CharGrid(grid, n);
        }

        public readonly int Id;
        public CharGrid Tile;
        private bool[] _free = new[] { true, true, true, true };
        public bool[] Free => OrientMatches(_free, Tile.Orientation);
        public TileType Location = TileType.Center;

        public bool[] PossibleMatches(ImageTile other)
        {
            var hits = new bool[] { false, false, false, false };
            foreach (var (edge, i) in Tile.Edges.Select((e, i) => (e, i)))
            {
                foreach (var otherEdge in other.Tile.Edges)
                {
                    if (!edge.SequenceEqual(otherEdge) && !edge.SequenceEqual(otherEdge.Reverse())) continue;
                    hits[i] = true;
                    break;
                }
            }

            return hits;
        }

        public IEnumerable<int> Candidates(IEnumerable<ImageTile> others)
        {
            return from tile in others where PossibleMatches(tile).Any() select tile.Id;
        }

        public void SetType(TileType t, bool[] matchable)
        {
            _free = matchable.ToArray();
            Location = t;
        }
    }

    private class MonsterValidator
    {
        public MonsterValidator(string pattern)
        {
            var lines = pattern.Trim().Split('\n');
            _m = lines.Length;
            _n = lines[0].Length;
            _pattern = new char[_m, _n];
            Size = 0;
            for (var i = 0; i < _m; i++)
            {
                for (var j = 0; j < _n; j++)
                {
                    var c = lines[i][j];
                    if (c == '#') Size++;
                    _pattern[i, j] = c;
                }
            }
        }

        private readonly char[,] _pattern;
        private readonly int _m;
        private readonly int _n;
        public int Size;

        public bool IsMonster(char[,] grid, int n, int i, int j)
        {
            for (var di = 0; di < _m; di++)
            {
                var ii = i + di;
                if (ii >= n) return false;
                for (var dj = 0; dj < _n; dj++)
                {
                    var jj = j + dj;
                    if (jj >= n || (_pattern[di, dj] == '#' && grid[ii, jj] != '#')) return false;
                }
            }

            return true;
        }
    }
    
    public static async Task Main()
    {
        var data = (await File.ReadAllTextAsync("input.txt"))
            .Split("\n\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToArray();
        var tiles = data.Select(tile => new ImageTile(tile)).ToDictionary(x => x.Id, x => x);
        
        // For each tile, determine edges that cannot possibly match anything (must be on outer boundary).
        var matches = new Dictionary<int, bool[]>();
        foreach (var (id, value) in tiles)
        {
            foreach (var tile2 in tiles)
            {
                if (id == tile2.Key) continue;
                var hits = value.PossibleMatches(tile2.Value);
                if (!matches.ContainsKey(id))
                {
                    matches[id] = hits;
                }
                else
                {
                    for (var k = 0; k < matches[id].Length; k++)
                    {
                        matches[id][k] |= hits[k];
                    }
                }
            }
        }

        // Use the information from above to find the edge/corner tiles.
        var corners = new List<ImageTile>();
        var edges = new List<ImageTile>();
        var centers = new List<ImageTile>();
        var borderIds = corners.Select(x => x.Id).Union(edges.Select(x => x.Id)).ToHashSet();
        foreach (var (id, hits) in matches)
        {
            switch (hits.Count(x => x))
            {
                case 2:
                    tiles[id].SetType(ImageTile.TileType.Corner, matches[id]);
                    edges.Add(tiles[id]);
                    corners.Add(tiles[id]);
                    break;
                case 3:
                    tiles[id].SetType(ImageTile.TileType.Edge, matches[id]);
                    edges.Add(tiles[id]);
                    break;
                default:
                    centers.Add(tiles[id]);
                    break;
            }
        }

        // Part A, product of corner tile IDs
        var resultA = corners.Aggregate(1ul, (acc, curr) => acc * (ulong)curr.Id);
        Console.WriteLine($"A: {resultA}");




        var test = new char[3, 3] { {'1', '2', '3'},{'4', '5', '6'},{'7','8','9'}};

        void PrintArr(char[,] grid, int n)
        {
            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < n; j++)
                {
                    Console.Write($"{grid[i, j]} ");
                }
                Console.Write('\n');
            }
            Console.Write('\n');
        }
        PrintArr(test, 3);
        PrintArr(CharGrid.Rotate(test, 3, 3), 3);
        Console.WriteLine(edges.Count);




        // Begin building process.
        var sideLength = edges.Count / 4 + 1;
        var imageGrid = new ImageTile[sideLength, sideLength];
        var topEdgeQueue = new Queue<(HashSet<int>, List<(int, bool, int)>)>();
        
        // Start queue with a corner tile at the top left corner.
        var tileNW = corners[0]; // checked by hand
        foreach (var (f, r) in CharGrid.Orientations)
        {
            tileNW.Tile.SetOrientation(r, f);
            var free = tileNW.Free;
            if (free[0] || free[2]) continue;
            topEdgeQueue.Enqueue((new HashSet<int>() { tileNW.Id },
                new List<(int, bool, int)>() { (tileNW.Id, f, r) }));
            break;
        }
        
        Console.WriteLine($"Starting queue size: {topEdgeQueue.Count}\nSide length: {sideLength}");
        
        // Fill top edge.
        var tops = new List<List<int>>();
        while (topEdgeQueue.Count > 0)
        {
            var (usedIds, topEdge) = topEdgeQueue.Dequeue();
            var length = topEdge.Count;
            
            // Extract finished border
            if (usedIds.SetEquals(borderIds))// || length >= sideLength - 1)
            {
                tops.Add(topEdge.Select(((int id, bool flip, int rot) tile) => tile.id).ToList());
                continue;
            }
            
            var (prevId, prevF, prevR) = topEdge.Last();
            var prevTile = tiles[prevId];
            prevTile.Tile.SetOrientation(prevR, prevF);

            // 0,   1,      2,    3
            // top, bottom, left, right
            var (borderSides, sourceSide, destSide) = (Math.DivRem(length, sideLength - 1)) switch
            {
                (var n, 0) => n switch
                {
                    0 => (new[] { 0, 2 }, 3, 2),
                    1 => (new[] { 0, 3 }, 1, 0),
                    2 => (new[] { 1, 3 }, 2, 3),
                    3 => (new[] { 1, 2 }, 0, 1),
                    _ => throw new ArgumentException("Unexpected border length encountered.")
                },
                (var n, _) => n switch
                {
                    0 => (new[] { 0 }, 3, 2),
                    1 => (new[] { 3 }, 1, 0),
                    2 => (new[] { 1 }, 2, 3),
                    3 => (new[] { 2 }, 0, 1),
                    _ => throw new ArgumentException("Unexpected border length encountered.")
                }
            };

            foreach (var tile in edges.Where(tile => !usedIds.Contains(tile.Id)))
            {
                foreach (var (f, r) in CharGrid.Orientations)
                {
                    tile.Tile.SetOrientation(r, f);
                    var free = tile.Free;

                    if (borderSides.Any(i => free[i])) continue;
                    if (!(tile.Tile.Edges[destSide].SequenceEqual(prevTile.Tile.Edges[sourceSide])
                          || tile.Tile.Edges[destSide].SequenceEqual(prevTile.Tile.Edges[sourceSide].Reverse())))
                        continue;

                    // var newTopEdge = new List<(int, bool, int)>(topEdge) { (tile.Id, f, r) };
                    var newTopEdge = new List<(int, bool, int)>();
                    foreach (var (id, flip, rot) in topEdge)
                    {
                        newTopEdge.Add((id, flip, rot));
                    }
                    newTopEdge.Add((tile.Id, f, r));

                    // var newUsed = new HashSet<int>(usedIds) { tile.Id };
                    var newUsed = new HashSet<int>() { tile.Id };
                    foreach (var id in usedIds)
                    {
                        newUsed.Add(id);
                    }
                    
                    topEdgeQueue.Enqueue((newUsed, newTopEdge));
                }
            }
        }
        Console.WriteLine($"Valid borders: {tops.Count}");
        // Console.WriteLine(string.Join(", ", tops.First()));
        
        // for (var i = 1; i < sideLength - 1; i++)
        // {
        //     // Top
        //     var foundTop = false;
        //     foreach (var tile in edges)
        //     {
        //         foreach (var (f, r) in CharGrid.Orientations)
        //         {
        //             tile.Tile.SetOrientation(r, f);
        //             var free = tile.Free;
        //             if (free[0]) continue;
        //             if (!tile.Tile.Left.SequenceEqual(imageGrid[i - 1, 0].Tile.Right)) continue;
        //             Console.WriteLine($"Set [{i}, 0] to tile {tile.Id}");
        //             imageGrid[i, 0] = tile;
        //             edges.Remove(tile);
        //             foundTop = true;
        //             break;
        //         }
        //
        //         if (foundTop) break;
        //     }
        //     
        //     // Left
        //     var foundLeft = false;
        //     foreach (var tile in edges)
        //     {
        //         foreach (var (f, r) in CharGrid.Orientations)
        //         {
        //             tile.Tile.SetOrientation(r, f);
        //             var free = tile.Free;
        //             if (free[2]) continue;
        //             if (!tile.Tile.Top.SequenceEqual(imageGrid[0, i - 1].Tile.Bottom)) continue;
        //             Console.WriteLine($"Set [0, {i}] to tile {tile.Id}");
        //             imageGrid[0, i] = tile;
        //             edges.Remove(tile);
        //             foundLeft = true;
        //             break;
        //         }
        //
        //         if (foundLeft) break;
        //     }
        // }

    }
}
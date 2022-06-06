using Numcs;

namespace AdventOfCode20;

internal static class AdventOfCode20
{
    private static readonly List<(bool, int)> Orientations = new()
    {
        (false, 0), (false, 1), (false, 2), (false, 3),
        (true, 0), (true, 1), (true, 2), (true, 3)
    };

    private enum Sides
    {
        Top = 0,
        Right = 1,
        Bottom = 2,
        Left = 3
    }
    
    private class CharGrid : Array2D<char>
    {
        public CharGrid(char[,] tile) : base(tile)
        {
            Grid = new Array2D<char>(tile);
        }

        public Array2D<char> Grid;

        public void SetOrientation(bool flip, int rotations)
        {
            Grid = flip ? FlipUD().Rot90(rotations) : Rot90(rotations);
        }
        
        private Array1D<char> GetEdge(int fixedAxis, int fixedIndex)
        {
            return fixedAxis switch
            {
                0 => Grid[fixedIndex, ..],
                1 => Grid[.., fixedIndex],
                _ => throw new ArgumentException($"Invalid axis for 2D array '{fixedAxis}'.")
            };
        }
        
        public Array1D<char> Left => GetEdge(1, 0);
        public Array1D<char> Right => GetEdge(1, _n - 1);
        public Array1D<char> Top => GetEdge(0, 0);
        public Array1D<char> Bottom => GetEdge(0, _m - 1);
        public Array1D<char>[] Edges => new[] { Top, Right, Bottom, Left };
    }
    
    private class ImageTile : CharGrid
    {
        public ImageTile(int id, char[,] tile) : base(tile)
        {
            Id = id;
        }

        public ImageTile((int id, char[,] tile) parse) : this(parse.id, parse.tile) {}

        public static (int, char[,]) ParseText(string input)
        {
            var lines = input.Split('\n');
            var n = lines.Length - 1;
        
            var id = int.Parse(lines[0].TrimEnd(':').Split(' ')[^1]);
            var grid = new char[n, n];
            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < n; j++)
                {
                    grid[i, j] = lines[i + 1][j];
                }
            }

            return (id, grid);
        }

        public readonly int Id;

        public bool[] PossibleMatches(ImageTile other)
        {
            var hits = new bool[] { false, false, false, false };
            foreach (var (edge, i) in Edges.Select((e, i) => (e, i)))
            {
                if (other.Edges.Any(otherEdge => edge.ArrayEquals(otherEdge) || edge.ArrayEquals(otherEdge.Reverse())))
                {
                    hits[i] = true;
                }
            }

            return hits;
        }

        public IEnumerable<int> Candidates(IEnumerable<ImageTile> others)
        {
            return from tile in others where PossibleMatches(tile).Any() select tile.Id;
        }
    }

    private class MonsterValidator
    {
        public MonsterValidator(string pattern, char body = '#')
        {
            var lines = pattern.Split('\n');
            _m = lines.Length;
            _n = lines[0].Length;

            _pattern = new HashSet<(int, int)>();
            Size = 0;
            for (var i = 0; i < _m; i++)
            {
                for (var j = 0; j < _n; j++)
                {
                    var c = lines[i][j];
                    if (c == body)
                    {
                        Size++;
                        _pattern.Add((i, j));
                    };
                }
            }
        }
        
        private readonly HashSet<(int, int)> _pattern;
        private readonly int _m;
        private readonly int _n;
        public int Size;

        public IEnumerable<(int, int)> Indices()
        {
            return from pos in _pattern select pos;
        }
        
        public bool IsMonster(char[,] grid, int i, int j)
        {
            var n = grid.GetLength(0);
            for (var di = 0; di < _m; di++)
            {
                var ii = i + di;
                if (ii >= n) return false;
                for (var dj = 0; dj < _n; dj++)
                {
                    var jj = j + dj;
                    if (jj >= n || (_pattern.Contains((di, dj)) && grid[ii, jj] == '.')) return false;
                }
            }

            return true;
        }
    }
    
    public static async Task Main()
    {
        var data = (await File.ReadAllTextAsync("input.txt"))
            .Split("\n\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var tiles = data
            .Select(tile => new ImageTile(ImageTile.ParseText(tile)))
            .ToDictionary(x => x.Id, x => x);
        
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

        var borderEdges = new HashSet<string>();
        foreach (var (id, hits) in matches)
        {
            var e = tiles[id].Edges;
            for (var i = 0; i < e.Length; i++)
            {
                if (hits[i]) continue;
                var edge = string.Join("", e[i]);
                borderEdges.Add(edge);
                borderEdges.Add(edge.Reverse());
            }
        }
        
        // Use the information from above to find the edge/corner tiles.
        var cornerIds = new List<int>();
        var borderIds = new List<int>();
        var centerIds = new List<int>();
        foreach (var (id, hits) in matches)
        {
            switch (hits.Count(x => x))
            {
                case 2:
                    cornerIds.Add(id);
                    borderIds.Add(id);
                    break;
                case 3:
                    borderIds.Add(id);
                    break;
                default:
                    centerIds.Add(id);
                    break;
            }
        }

        // Part A, product of corner tile IDs
        var resultA = cornerIds.Aggregate(1ul, (acc, curr) => acc * (ulong)curr);
        Console.WriteLine($"A: {resultA}");
        
        var sideLength = borderIds.Count / 4 + 1;
        var borderQueue = new Queue<(HashSet<int>, List<(int, bool, int)>)>();

        (HashSet<int>, int, int) BorderConstraints(int i)
        {
            var outerEdges = new HashSet<int>();
            int prevEdge;
            int currEdge;
            
            switch (Math.DivRem(i, sideLength - 1))
            {
                case (0, 0):
                    outerEdges.Add((int)Sides.Left);
                    outerEdges.Add((int)Sides.Top);
                    prevEdge = (int)Sides.Top;
                    currEdge = (int)Sides.Bottom;
                    break;
                case (1, 0):
                    outerEdges.Add((int)Sides.Top);
                    outerEdges.Add((int)Sides.Right);
                    prevEdge = (int)Sides.Right;
                    currEdge = (int)Sides.Left;
                    break;
                case (2, 0):
                    outerEdges.Add((int)Sides.Right);
                    outerEdges.Add((int)Sides.Bottom);
                    prevEdge = (int)Sides.Bottom;
                    currEdge = (int)Sides.Top;
                    break;
                case (3, 0):
                    outerEdges.Add((int)Sides.Bottom);
                    outerEdges.Add((int)Sides.Left);
                    prevEdge = (int)Sides.Left;
                    currEdge = (int)Sides.Right;
                    break;
                case (0, _):
                    outerEdges.Add((int)Sides.Top);
                    prevEdge = (int)Sides.Right;
                    currEdge = (int)Sides.Left;
                    break;
                case (1, _):
                    outerEdges.Add((int)Sides.Right);
                    prevEdge = (int)Sides.Bottom;
                    currEdge = (int)Sides.Top;
                    break;
                case (2, _):
                    outerEdges.Add((int)Sides.Bottom);
                    prevEdge = (int)Sides.Left;
                    currEdge = (int)Sides.Right;
                    break;
                case (3, _):
                    outerEdges.Add((int)Sides.Left);
                    prevEdge = (int)Sides.Top;
                    currEdge = (int)Sides.Bottom;
                    break;
                default:
                    throw new ArgumentException("Boundary tile index exceeded total border length.");
            }
            
            return (outerEdges, prevEdge, currEdge);
        }

        IEnumerable<(bool, int)> ValidBorderOrientations(IReadOnlyCollection<int> outerEdges, ImageTile tile)
        {
            foreach (var (f, r) in Orientations)
            {
                tile.SetOrientation(f, r);
                var e = tile.Edges;
                if (outerEdges.All(i => borderEdges.Contains(string.Join("", e[i])))) yield return (f, r);
            }
        }

        var (borderStartEdges, _, _) = BorderConstraints(0);
        var (borderStartF, borderStartR) = ValidBorderOrientations(borderStartEdges, tiles[cornerIds[0]]).First();
        borderQueue.Enqueue(
            (
                new HashSet<int> { cornerIds[0] },
                new List<(int, bool, int)> { (cornerIds[0], borderStartF, borderStartR) }
            )
        );

        List<(int, bool, int)> border = null;
        while (borderQueue.Count > 0)
        {
            var (usedIds, currBorder) = borderQueue.Dequeue();
            var length = currBorder.Count;

            if (usedIds.SetEquals(borderIds))
            {
                border = currBorder;
                break;
            }

            var (prevId, prevF, prevR) = currBorder.Last();
            var prevTile = tiles[prevId];
            prevTile.SetOrientation(prevF, prevR);
            var prevEdges = prevTile.Edges;

            var (currWalls, prevEdgeIndex, currEdgeIndex) = BorderConstraints(length);

            foreach (var id in borderIds.Where(x => !usedIds.Contains(x)))
            {
                var tile = tiles[id];
                foreach (var (f, r) in ValidBorderOrientations(currWalls, tile))
                {
                    tile.SetOrientation(f, r);
                    var currEdges = tile.Edges;
                    if (!prevEdges[prevEdgeIndex].ArrayEquals(currEdges[currEdgeIndex])) continue;

                    var newUsedIds = new HashSet<int>(usedIds) { tile.Id };
                    var newBorderSequence = currBorder.ToList();
                    newBorderSequence.Add((tile.Id, f, r));
                    
                    borderQueue.Enqueue((newUsedIds, newBorderSequence));
                }
            }
        }

        var tileArrangement = new Dictionary<(int, int), (int, bool, int)>();
        foreach (var (x, i) in border.Select((x, i) => (x, i)))
        {
            var (q, r) = Math.DivRem(i, sideLength - 1);
            var pos = q switch
            {
                0 => (0, r),
                1 => (r, sideLength - 1),
                2 => (sideLength - 1, sideLength - 1 - r),
                3 => (sideLength - 1 - r, 0),
                _ => throw new ArgumentException("More border tiles provided than possible.")
            };
            tileArrangement[pos] = x;
        }
        
        // Reconstruct center of image.
        bool IsValidPlacement(int i, int j, ImageTile tile, Dictionary<(int, int), (int, bool, int)> placed)
        {
            if (placed.TryGetValue((i - 1, j), out (int id, bool flip, int rot) up))
            {
                tiles[up.id].SetOrientation(up.flip, up.rot);
                if (!tiles[up.id].Bottom.ArrayEquals(tile.Top)) return false;
            }
            if (placed.TryGetValue((i, j + 1), out (int id, bool flip, int rot) right))
            {
                tiles[right.id].SetOrientation(right.flip, right.rot);
                if (!tiles[right.id].Left.ArrayEquals(tile.Right)) return false;
            }
            if (placed.TryGetValue((i + 1, j), out (int id, bool flip, int rot) down))
            {
                tiles[down.id].SetOrientation(down.flip, down.rot);
                if (!tiles[down.id].Top.ArrayEquals(tile.Bottom)) return false;
            }
            if (placed.TryGetValue((i, j - 1), out (int id, bool flip, int rot) left))
            {
                tiles[left.id].SetOrientation(left.flip, left.rot);
                if (!tiles[left.id].Right.ArrayEquals(tile.Left)) return false;
            }

            return true;
        }

        (int, int) NextOpening(int i, int j)
        {
            var ii = i;
            var jj = j + 1;
            if (jj > sideLength - 2)
            {
                jj = 1;
                ii++;
            }

            return (ii, jj);
        }

        Dictionary<(int, int), (int, bool, int)> CopyArrangementWith(Dictionary<(int, int), (int, bool, int)> curr,
            (int, int) newKey, (int, bool, int) newValue)
        {
            var next = curr.ToDictionary(entry => entry.Key, entry => entry.Value);
            next[newKey] = newValue;
            return next;
        }
        
        var centerQueue = new Queue<((int, int), HashSet<int>, Dictionary<(int, int), (int, bool, int)>)>();
        foreach (var centerId in centerIds)
        {
            var tile = tiles[centerId];
            foreach (var (f, r) in Orientations)
            {
                tile.SetOrientation(f, r);
                if (!IsValidPlacement(1, 1, tile, tileArrangement)) continue;
                var possibleUsedIds = new HashSet<int> { centerId };
                var possibleArrangement = CopyArrangementWith(tileArrangement, (1, 1), (centerId, f, r));

                centerQueue.Enqueue((NextOpening(1, 1), possibleUsedIds, possibleArrangement));
            }
        }

        while (centerQueue.Count > 0)
        {
            var (currPos, currUsed, currArrangement) = centerQueue.Dequeue();
            var (i, j) = currPos;

            if (currUsed.SetEquals(centerIds))
            {
                tileArrangement = currArrangement;
                continue;
            }
            
            foreach (var centerId in centerIds.Where(id => !currUsed.Contains(id)))
            {
                var currTile = tiles[centerId];
                foreach (var (f, r) in Orientations)
                {
                    currTile.SetOrientation(f, r);
                    if (!IsValidPlacement(i, j, currTile, currArrangement)) continue;
                    var possibleUsedIds = new HashSet<int>(currUsed) { centerId };
                    var possibleArrangement = CopyArrangementWith(currArrangement, currPos, (centerId, f, r));

                    centerQueue.Enqueue((NextOpening(i, j), possibleUsedIds, possibleArrangement));
                }
            }
        }

        // Stitch tiles into a single image.
        var tileSize = tiles.Values.First().Shape.Item1 - 2;
        var imageSize = sideLength * tileSize;
        var imgArr = new char[imageSize, imageSize];

        for (var i = 0; i < sideLength; i++)
        {
            for (var j = 0; j < sideLength; j++)
            {
                var (currId, currF, currR) = tileArrangement[(i, j)];
                var currTile = tiles[currId];
                currTile.SetOrientation(currF, currR);
                for (var ii = 0; ii < tileSize; ii++)
                {
                    for (var jj = 0; jj < tileSize; jj++)
                    {
                        imgArr[i * tileSize + ii, j * tileSize + jj] = currTile.Grid[ii + 1, jj + 1];
                    }
                }
            }
        }

        var image = new CharGrid(imgArr);
        
        // Create sea monster validator from given pattern.
        var validator = new MonsterValidator(await File.ReadAllTextAsync("monster.txt"));

        (int monsters, int roughness) AnalyzeImage(CharGrid image)
        {
            var (m, n) = image.Shape;
            var arr = image.Grid.ToArray();

            var monsters = 0;
            for (var i = 0; i < m; i++)
            {
                for (var j = 0; j < n; j++)
                {
                    if (!validator.IsMonster(arr, i, j)) continue;
                    foreach (var (di, dj) in validator.Indices())
                    {
                        arr[i + di, j + dj] = 'O';
                    }

                    monsters++;
                }
            }

            var roughness = 0;
            for (var i = 0; i < m; i++)
            {
                for (var j = 0; j < n; j++)
                {
                    if (arr[i, j] != '#') continue;
                    roughness++;
                }
            }

            return (monsters, roughness);
        }


        // Find orientation of image that has recognizable monsters.
        var maxMonsters = 0;
        var resultB = 0;

        foreach (var (f, r) in Orientations)
        {
            image.SetOrientation(f, r);
            var (currMonsters, currRoughness) = AnalyzeImage(image);
            if (currMonsters < maxMonsters || (currMonsters == maxMonsters && currRoughness >= resultB)) continue;
            
            maxMonsters = currMonsters;
            resultB = currRoughness;
        }

        Console.WriteLine($"B: {resultB}");
    }
}

internal static class StringUtil
{
    public static string Reverse(this string s)
    {
        var arr = s.ToCharArray();
        Array.Reverse(arr);
        return new string(arr);
    }
}

namespace AdventOfCode17;

/// <summary>
/// Class <class>AdventOfCode17</class> implements a solution to 2020 Advent of Code Day 17: Conway Cubes.
/// </summary>
internal static class AdventOfCode17
{
    /// <summary>
    /// Const <c>Cycles</c> is the total number of grid iterations to model.
    /// </summary>
    private const int Cycles = 6;

    /// <summary>
    /// Class <class>TesseractGrid</class> models an infinite grid of cells following the cellular automata rules
    /// defined in Day 17 of Advent of Code 2020.
    /// </summary>
    private class TesseractGrid
    {
        /// <summary>
        /// This constructor initializes the new <c>TesseractGrid</c> with an initial arrangement of active cells.
        /// </summary>
        /// <param name="start">The initial cell states.</param>
        /// <param name="enable4D">Flag indicating whether to propagate along the fourth spatial dimension (part 2).</param>
        public TesseractGrid(IReadOnlyList<char[]> start, bool enable4D = true)
        {
            var x = start.Count;
            var y = start[0].Length;
            
            // Only half of the grid in dimensions 3 and 4 is needed since the cells propagate symmetrically along those axes.
            _grid = new char[_m = x + 2 * Cycles, _n = y + 2 * Cycles, _o = Cycles + 1, _p = (enable4D ? Cycles + 1 : 1)];

            // Initialize every grid element as inactive.
            foreach (var (i, j, k, l) in Positions())
            {
                _grid[i, j, k, l] = '.';
            }

            // Copy the starting states into the grid.
            for (var i = 0; i < x; i++)
            {
                for (var j = 0; j < y; j++)
                {
                    _grid[i + Cycles, j + Cycles, 0, 0] = start[i][j];
                }
            }
        }
        
        private char[,,,] _grid;
        private readonly int _m;
        private readonly int _n;
        private readonly int _o;
        private readonly int _p;
        
        /// <summary>
        /// This method determines whether a given 4D index lies within bounds of the _grid array.
        /// </summary>
        /// <param name="i">The index for the first array dimension.</param>
        /// <param name="j">The index for the second array dimension.</param>
        /// <param name="k">The index for the third array dimension.</param>
        /// <param name="l">The index for the fourth array dimension.</param>
        /// <returns> True if <c>[i, j, k, l]</c> is a valid index for <c>_grid</c>, otherwise False. </returns>
        private bool IsInBounds(int i, int j, int k, int l)
        {
            return ((0 <= i && i < _m) && (0 <= j && j < _n) && (0 <= k && k < _o) && (0 <= l && l < _p));
        }

        /// <summary>
        /// This method iterates through the valid adjacent neighbors of a given index.
        /// </summary>
        /// <param name="i">The index for the first array dimension.</param>
        /// <param name="j">The index for the second array dimension.</param>
        /// <param name="k">The index for the third array dimension.</param>
        /// <param name="l">The index for the fourth array dimension.</param>
        /// <returns>
        /// A sequence of indices of cells at most 1 away in each spatial dimension that lie in <c>_grid</c>.
        /// Does not include the index passed in.
        /// </returns>
        private IEnumerable<(int, int, int, int)> Neighbors(int i, int j, int k, int l)
        {
            for (var ii = i - 1; ii < i + 2; ii++)
            {
                for (var jj = j - 1; jj < j + 2; jj++)
                {
                    for (var kk = k - 1; kk < k + 2; kk++)
                    {
                        for (var ll = l - 1; ll < l + 2; ll++)
                        {
                            if (!IsInBounds(ii, jj, kk, ll) || (ii == i && jj == j && kk == k && ll == l)) continue;
                            
                            // Yield the current index, respecting symmetry of the grid along dimensions 3 and 4.
                            var copies = (k == 0 && kk != 0 ? 2 : 1) * (l == 0 && ll != 0 ? 2 : 1);
                            for (var c = 0; c < copies; c++)
                            {
                                yield return (ii, jj, kk, ll);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method iterates through every valid index in <c>_grid</c>.
        /// </summary>
        /// <returns>
        /// A sequence of indices for every cell in <c>_grid</c>.
        /// </returns>
        private IEnumerable<(int, int, int, int)> Positions()
        {
            for (var i = 0; i < _m; i++)
            {
                for (var j = 0; j < _n; j++)
                {
                    for (var k = 0; k < _o; k++)
                    {
                        for (var l = 0; l < _p; l++)
                        {
                            yield return (i, j, k, l);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method determines the state of a given cell at the next iteration.
        /// </summary>
        /// <param name="i">The index for the first array dimension.</param>
        /// <param name="j">The index for the second array dimension.</param>
        /// <param name="k">The index for the third array dimension.</param>
        /// <param name="l">The index for the fourth array dimension.</param>
        /// <returns>
        /// <c>'#'</c> (active) if the cell is currently active and has exactly 2 or 3 active neighbors, or if the cell
        /// is currently inactive and has exactly 3 active neighbors. Otherwise, returns <c>'.'</c> (inactive).
        /// </returns>
        private char NextCubeState(int i, int j, int k, int l)
        {
            var activeNeighbors = Neighbors(i, j, k, l)
                .Count(pos => _grid[pos.Item1, pos.Item2, pos.Item3, pos.Item4] == '#');
            return _grid[i, j, k, l] switch
            {
                '#' => activeNeighbors is 2 or 3 ? '#' : '.',
                '.' => activeNeighbors == 3 ? '#' : '.',
                _ => '.'
            };
        }

        /// <summary>
        /// This method models a single iteration of the cellular automata game and modifies <c>_grid</c> accordingly.
        /// </summary>
        public void Step()
        {
            var nextGrid = new char[_m, _n, _o, _p];
            foreach (var (i, j, k, l) in Positions())
            {
                nextGrid[i, j, k, l] = NextCubeState(i, j, k, l);
            }
            _grid = nextGrid;
        }

        /// <summary>
        /// This method counts the total number of active cells (state <c>'#'</c>) in the grid.
        /// </summary>
        /// <returns>The number of active Conway cubes in the grid.</returns>
        public int ActiveCubes()
        {
            var count = 0;
            foreach (var (i, j, k, l) in Positions())
            {
                // Determine the multiplicity of the given index due to grid symmetry.
                var multiplicity = (k != 0 ? 2 : 1) * (l != 0 ? 2 : 1);

                if (_grid[i, j, k, l] == '#')
                {
                    count += multiplicity;
                }
            }

            return count;
        }
    }

    /// <summary>
    /// This method is the main entry point of the program.
    /// </summary>
    public static async Task Main()
    {
        var data = await File.ReadAllLinesAsync("input.txt");
        
        // Parse input into the starting state and initialize cube grids.
        var startGrid = data.Select(x => x.ToCharArray()).ToArray();
        var cubes = new TesseractGrid(startGrid, enable4D: false);
        var hypercubes = new TesseractGrid(startGrid);
        
        // Model both cellular automata to completion.
        for (var i = 0; i < Cycles; i++)
        {
            cubes.Step();
            hypercubes.Step();
        }
        
        // Output the final count of active cubes for both grids.
        Console.WriteLine($"A: {cubes.ActiveCubes()}\nB: {hypercubes.ActiveCubes()}");
    }
}
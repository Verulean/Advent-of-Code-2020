namespace AdventOfCode03;

internal static class AdventOfCode03 {
    private class Skier {
        public Skier(string trees, int[] slope) {
            _slope = slope;
            TreesHit = 0;

            var lines = trees.Split('\n');
            _height = lines.Length;
            _width = lines[0].Length;
            _trees = new char[_height][];
            foreach (var (line, i) in lines.Select((i, x) => (i, x))) {
                _trees[i] = line.ToCharArray();
            }
        }

        public Skier(string trees) : this(trees, new int[] {1, 3}) { }

        private readonly char[][] _trees;
        private char _treeChar = '#';
        private readonly int _height;
        private readonly int _width;
        private readonly int[] _slope;
        private readonly int[] _position = {0, 0};
        public int TreesHit;

        private bool IsAtBottom() {
            return this._position[0] >= this._height;
        }

        private void Move() {
            if (this._trees[this._position[0]][this._position[1]] == this._treeChar) {
                this.TreesHit++;
            }
            if (this.IsAtBottom()) {
                return;
            }
            this._position[0] += this._slope[0];
            this._position[1] = (this._position[1] + this._slope[1]) % this._width;
        }

        public void Ski() {
            while (!this.IsAtBottom()) {
                this.Move();
            }
        }
    }

    public static async Task Main() {
        var trees = await File.ReadAllTextAsync("input.txt");

        // Part A, count trees hit for slope 1,3
        Skier ski = new(trees);
        ski.Ski();
        Console.WriteLine($"A: {ski.TreesHit}");

        // Part B, product of various slopes
        var result = 1ul;
        int[][] slopes = {
            new int[] {1, 1}, 
            new int[] {1, 3}, 
            new int[] {1, 5}, 
            new int[] {1, 7}, 
            new int[] {2, 1}
        };

        foreach (var slope in slopes) {
            ski = new Skier(trees, slope);
            ski.Ski();
            result *= (ulong) ski.TreesHit;
        }
        Console.WriteLine($"B: {result}");
    }
}

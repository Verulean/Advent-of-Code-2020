using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AdventOfCode12;

internal static class AdventOfCode12
{
    private static readonly List<char> Clockwise = new() { 'N', 'E', 'S', 'W' };
    
    private static char RotateDirection(char facing, char direction, int degrees)
    {
        var i = Clockwise.FindIndex(x => x == facing);
        var n = (degrees / 90) * (direction == 'R' ? 1 : -1);
        var N = Clockwise.Count;
        return Clockwise[((i + n) % N + N) % N];
    }

    private static int[] RotateVector(int[] v, char direction, int degrees)
    {
        var n = (degrees / 90) * (direction == 'R' ? 1 : -1);
        n = ((n % 4) + 4) % 4;
        // Rotate clockwise n times
        var vNext = v.ToArray();
        for (var i = 0; i < n; i++)
        {
            var x = vNext[0];
            var y = vNext[1];
            vNext[0] = y;
            vNext[1] = -x;
        }

        return vNext;
    }
    
    private class Ferry
    {
        public Ferry()
        {
            _posA = new[] { 0, 0 };
            _posB = new[] { 0, 0 };
            _wayPos = new[] { 10, 1 };
            _facing = 'E';
        }

        private int[] _posA;
        private int[] _posB;
        private int[] _wayPos;
        private char _facing;
        
        private static void MoveVector(ref int[] position, char direction, int length)
        {
            switch (direction)
            {
                case 'N':
                    position[1] += length;
                    break;
                case 'S':
                    position[1] -= length;
                    break;
                case 'E':
                    position[0] += length;
                    break;
                case 'W':
                    position[0] -= length;
                    break;
            }
        }

        private void Move(char direction, int length)
        {
            switch (direction)
            {
                case 'F':
                    MoveVector(ref _posA, _facing, length);
                    _posB[0] += _wayPos[0] * length;
                    _posB[1] += _wayPos[1] * length;
                    break;
                default:
                    MoveVector(ref _posA, direction, length);
                    MoveVector(ref _wayPos, direction, length);
                    break;
            }
        }

        public void ExecuteAction(string action)
        {
            var op = action[0];
            var n = int.Parse(action[1..]);
            switch (op)
            {
                case 'N':
                case 'S':
                case 'E':
                case 'W':
                case 'F':
                    Move(op, n);
                    break;
                case 'L':
                case 'R':
                    _facing = RotateDirection(_facing, op, n);
                    _wayPos = RotateVector(_wayPos, op, n);
                    break;
            }
        }

        public int Distance(int part = 1)
        {
            return part switch
            {
                1 => Math.Abs(_posA[0]) + Math.Abs(_posA[1]),
                2 => Math.Abs(_posB[0]) + Math.Abs(_posB[1]),
                _ => throw new ArgumentException("part must be `1` or `2`")
            };
        }
    }
    
    public static async Task Main()
    {
        var actions = await File.ReadAllLinesAsync("input.txt");
        var ship = new Ferry();
        foreach (var action in actions)
        {
            ship.ExecuteAction(action);
        }
        Console.WriteLine($"A: {ship.Distance()}");
        Console.WriteLine($"B: {ship.Distance(2)}");
    }
}
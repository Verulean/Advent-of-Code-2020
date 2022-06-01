namespace AdventOfCode08;

using Instruction = Tuple<string, int>;

internal static class AdventOfCode08
{
    private static Instruction ParseLine(string line)
    {
        var elements = line.Trim().Split(' ');
        var op = elements[0];
        var num = int.Parse(elements[1]);

        return new Instruction(op, num);
    }

    private static Tuple<int, bool> RunProgram(Instruction[] program, int maxRepeats = 1)
    {
        var i = 0;
        var acc = 0;
        var counts = new Dictionary<int, int>();
        var loop = false;

        while (i < program.Length)
        {
            // Execute current line
            if (counts.ContainsKey(i))
            {
                counts[i]++;
                if (counts[i] > maxRepeats)
                {
                    loop = true;
                    break;
                }
            }
            else
            {
                counts[i] = 1;
            }

            var (op, num) = program[i];
            switch (op)
            {
                case "acc":
                    acc += num;
                    i++;
                    break;
                case "jmp":
                    i += num;
                    break;
                case "nop":
                    i++;
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        return new Tuple<int, bool>(acc, loop);
    }
    
    public static async Task Main()
    {
        var program = (await File.ReadAllLinesAsync("input.txt")).Select(ParseLine).ToArray();
        
        // A: value in accumulator before entering infinite loop
        var (resultA, _) = RunProgram(program);
        Console.WriteLine($"A: {resultA}");
        
        // B: find modified program that does not result in infinite loop
        Dictionary<string, string> swap = new()
        {
            ["nop"] = "jmp",
            ["jmp"] = "nop",
        };
        var resultB = 0;
        for (var i = 0; i < program.Length; i++)
        {
            var (op, num) = program[i];
            if (!swap.TryGetValue(op, out var newOp))
            {
                continue;
            }
            
            program[i] = new Instruction(newOp, num);
            
            // Check if this program terminates
            var (temp, loop) = RunProgram(program);
            if (!loop)
            {
                resultB = temp;
                break;
            }
            
            // Restore line to unmodified instruction
            program[i] = new Instruction(op, num);
        }
        Console.WriteLine($"B: {resultB}");
    }
}
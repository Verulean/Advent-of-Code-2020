using System.Data;

namespace AdventOfCode18;

using ExprValue = UInt64;

internal static class AdventOfCode18
{
    private static List<object> ParseLine(string line)
    {
        var elements = new List<object>();
        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            switch (c)
            {
                case ' ':
                    continue;
                case '+':
                case '*':
                    elements.Add(c);
                    continue;
                case '(':
                {
                    var depth = 1;
                    var j = i;
                    while (depth > 0)
                    {
                        j++;
                        switch (line[j])
                        {
                            case '(':
                                depth++;
                                break;
                            case ')':
                                depth--;
                                break;
                        }
                    }
                    elements.Add(ParseLine(line[(i+1)..(j)]));
                    i = j;
                    continue;
                }
                case ')':
                    throw new InvalidExpressionException("Unmatched parentheses in input expression.");
                default:
                    elements.Add((ExprValue)c - '0');
                    continue;
            }
        }

        return elements;
    }
    
    private static ExprValue EvaluateElement(object elem, bool addPrecedence)
    {
        return elem switch
        {
            ExprValue i => i,
            List<object> l => EvaluateExpression(l, addPrecedence),
            _ => throw new ArgumentException("Unrecognized sub-expression encountered.")
        };
    }

    private static ExprValue EvaluateExpression(List<object> expr, bool addPrecedence = false)
    {
        if (addPrecedence)
        {
            var changed = true;
            while (changed)
            {
                changed = false;
                for (var i = 1; i < expr.Count; i += 2)
                {
                    if ((char)expr[i] != '+') continue;
                    var start = i - 1;
                    while (i < expr.Count && (char)expr[i] == '+')
                    {
                        i += 2;
                    }

                    if (start == 0 && i >= expr.Count) break;
                    var group = new List<object>();
                    for (var j = start; j < i; j++)
                    {
                        group.Add(expr[start]);
                        expr.RemoveAt(start);
                    }
                        
                    expr.Insert(start, group);
                    changed = true;
                    break;
                }
            }
        }
        
        var acc = EvaluateElement(expr.First(), addPrecedence);
        if (expr.Count <= 1) return acc;
        
        for (var i = 1; i < expr.Count; i++)
        {
            var op = (char)expr[i];
            var operand = EvaluateElement(expr[++i], addPrecedence);
            switch (op)
            {
                case '*':
                    acc *= operand;
                    break;
                case '+':
                    acc += operand;
                    break;
            }
        }

        return acc;
    }
    
    public static async Task Main()
    {
        var data = await File.ReadAllLinesAsync("input.txt");
        var expressions = data.Select(ParseLine).ToArray();

        var resultA = expressions.Aggregate(0ul, (current, expr) => current + EvaluateExpression(expr));
        Console.WriteLine($"A: {resultA}");
        
        var resultB = expressions.Aggregate(0ul, (current, expr) => current + EvaluateExpression(expr, addPrecedence: true));
        Console.WriteLine($"B: {resultB}");
    }
}
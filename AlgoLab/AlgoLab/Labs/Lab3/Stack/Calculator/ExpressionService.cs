using System.Globalization;
using System.Text;

namespace AlgoLab.Labs.Lab3.Stack.Calculator;

public sealed record PostfixStep(string Token, string Action, string StackState);

public sealed record PostfixResult(double Value, List<PostfixStep> Steps);

public static class ExpressionService
{
    private static readonly HashSet<string> Binary = new() { "+", "-", "*", ":", "/", "^" };
    private static readonly HashSet<string> Unary = new() { "ln", "cos", "sin", "sqrt", "neg" };

    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static string Fmt(double v) => v.ToString("G10", Inv);

    // ───────────── Вычисление постфиксной записи ─────────────

    /// <summary>Вычисляет «7 8 + 3 2 + /». Токены разделены пробелами. O(n) по числу токенов.</summary>
    public static PostfixResult EvaluatePostfix(string expression, bool trace = true)
    {
        var steps = new List<PostfixStep>();
        var stack = new MyStack<double>();
        var tokens = expression.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0) throw new FormatException("Выражение пустое.");

        foreach (var raw in tokens)
        {
            var t = raw.ToLowerInvariant();
            string action;

            if (Binary.Contains(t))
            {
                if (stack.Count < 2)
                    throw new FormatException($"Недостаточно операндов для «{raw}».");
                var b = stack.Pop();
                var a = stack.Pop();
                double r;
                switch (t)
                {
                    case "+": r = a + b; break;
                    case "-": r = a - b; break;
                    case "*": r = a * b; break;
                    case "^": r = Math.Pow(a, b); break;
                    default:
                        if (b == 0) throw new DivideByZeroException("Деление на ноль.");
                        r = a / b; break;
                }
                stack.Push(r);
                action = $"{Fmt(a)} {raw} {Fmt(b)} = {Fmt(r)}";
            }
            else if (Unary.Contains(t))
            {
                if (stack.Count < 1)
                    throw new FormatException($"Недостаточно операндов для «{raw}».");
                var a = stack.Pop();
                double r;
                switch (t)
                {
                    case "ln":
                        if (a <= 0) throw new ArgumentException("ln определён только для x > 0.");
                        r = Math.Log(a); break;
                    case "sqrt":
                        if (a < 0) throw new ArgumentException("sqrt от отрицательного числа.");
                        r = Math.Sqrt(a); break;
                    case "sin": r = Math.Sin(a); break;
                    case "cos": r = Math.Cos(a); break;
                    default: r = -a; break;
                }
                stack.Push(r);
                action = $"{raw}({Fmt(a)}) = {Fmt(r)}";
            }
            else if (double.TryParse(raw, NumberStyles.Float, Inv, out var num))
            {
                stack.Push(num);
                action = "Push";
            }
            else throw new FormatException($"Неизвестный токен «{raw}».");

            if (trace) steps.Add(new(raw, action, StackToString(stack)));
        }

        if (stack.Count != 1)
            throw new FormatException("В выражении лишние операнды (в стеке осталось больше одного значения).");

        return new PostfixResult(stack.Pop(), steps);
    }

    private static string StackToString(MyStack<double> s)
    {
        var list = s.ToList();
        list.Reverse(); // слева — основание, справа — вершина
        return "[ " + string.Join("  ", list.Select(Fmt)) + " ]";
    }

    // ───────────── Инфикс → постфикс (алгоритм сортировочной станции) ─────────────

    private static int Priority(string op) => op switch
    {
        "+" or "-" => 1,
        "*" or "/" or ":" => 2,
        "neg" => 3,   // унарный минус слабее ^: -2^2 = -(2^2)
        "^" => 4,
        _ => 0
    };

    private static bool IsFunc(string t) => t is "ln" or "sin" or "cos" or "sqrt";

    /// <summary>«(2+3)*4» → «2 3 + 4 *». Поддержка + - * / : ^, унарного минуса, ln/sin/cos/sqrt, скобок.</summary>
    public static string InfixToPostfix(string infix)
    {
        var output = new List<string>();
        var ops = new MyStack<string>();
        bool expectOperand = true; // для различения унарного и бинарного минуса
        int i = 0;

        while (i < infix.Length)
        {
            char c = infix[i];
            if (char.IsWhiteSpace(c)) { i++; continue; }

            if (char.IsDigit(c) || c == '.')
            {
                int start = i;
                while (i < infix.Length && (char.IsDigit(infix[i]) || infix[i] == '.')) i++;
                var num = infix[start..i];
                if (!double.TryParse(num, NumberStyles.Float, Inv, out _))
                    throw new FormatException($"Некорректное число «{num}».");
                output.Add(num);
                expectOperand = false;
            }
            else if (char.IsLetter(c))
            {
                int start = i;
                while (i < infix.Length && char.IsLetter(infix[i])) i++;
                var name = infix[start..i].ToLowerInvariant();
                if (!IsFunc(name)) throw new FormatException($"Неизвестная функция «{infix[start..i]}».");
                ops.Push(name);
                expectOperand = true;
            }
            else if (c == '(')
            {
                ops.Push("(");
                expectOperand = true;
                i++;
            }
            else if (c == ')')
            {
                while (!ops.IsEmpty() && ops.Top() != "(") output.Add(ops.Pop());
                if (ops.IsEmpty()) throw new FormatException("Лишняя закрывающая скобка.");
                ops.Pop(); // «(»
                if (!ops.IsEmpty() && IsFunc(ops.Top())) output.Add(ops.Pop());
                expectOperand = false;
                i++;
            }
            else if (c is '+' or '-' or '*' or '/' or ':' or '^')
            {
                string op = c.ToString();
                if (expectOperand)
                {
                    if (c == '-') op = "neg";
                    else if (c == '+') { i++; continue; } // унарный плюс игнорируем
                    else throw new FormatException($"Неожиданный оператор «{c}» в позиции {i + 1}.");
                }

                bool rightAssoc = op is "^" or "neg";
                while (!ops.IsEmpty() && ops.Top() != "(" && !IsFunc(ops.Top())
                       && (Priority(ops.Top()) > Priority(op)
                           || (Priority(ops.Top()) == Priority(op) && !rightAssoc)))
                    output.Add(ops.Pop());

                ops.Push(op);
                expectOperand = true;
                i++;
            }
            else throw new FormatException($"Недопустимый символ «{c}» в позиции {i + 1}.");
        }

        if (expectOperand && (output.Count > 0 || !ops.IsEmpty()))
            throw new FormatException("Выражение заканчивается оператором.");

        while (!ops.IsEmpty())
        {
            var top = ops.Pop();
            if (top == "(") throw new FormatException("Не закрыта скобка.");
            output.Add(top);
        }

        if (output.Count == 0) throw new FormatException("Выражение пустое.");
        return string.Join(' ', output);
    }

    // ───────────── Генерация для оценки сложности ─────────────

    /// <summary>Случайное корректное постфиксное выражение: n операндов и n−1 бинарных операций.</summary>
    public static string GeneratePostfix(int operands, Random rnd)
    {
        var sb = new StringBuilder(operands * 4);
        string[] ops = { "+", "-", "*" };
        int depth = 0, placed = 0, opsLeft = operands - 1;
        while (placed < operands || opsLeft > 0)
        {
            bool canOp = depth >= 2 && opsLeft > 0;
            bool canNum = placed < operands;
            if (canNum && (!canOp || rnd.Next(2) == 0))
            {
                sb.Append(rnd.Next(1, 10)).Append(' ');
                placed++; depth++;
            }
            else
            {
                sb.Append(ops[rnd.Next(ops.Length)]).Append(' ');
                opsLeft--; depth--;
            }
        }
        return sb.ToString().TrimEnd();
    }
}

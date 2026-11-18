namespace AlgoLab.Labs.Lab3.Stack.Brackets;

public static class BracketValidator
{
    public sealed record BracketStep(int Pos, char Ch, string Action, string StackState);

    public sealed record BracketResult(bool Balanced, string Message, List<BracketStep> Steps);

    public static BracketResult CheckBrackets(string text)
    {
        const string open = "([{", close = ")]}";
        var stack = new MyStack<(char ch, int pos)>();
        var steps = new List<BracketStep>();

        string State() => "[ " + string.Join(" ", Enumerable.Reverse(stack.ToList()).Select(x => x.ch)) + " ]";

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            int oi = open.IndexOf(c), ci = close.IndexOf(c);
            if (oi >= 0)
            {
                stack.Push((c, i));
                steps.Add(new(i + 1, c, "Push", State()));
            }
            else if (ci >= 0)
            {
                if (stack.IsEmpty())
                {
                    steps.Add(new(i + 1, c, "Ошибка: нет открывающей", State()));
                    return new(false, $"Лишняя «{c}» в позиции {i + 1}: стек пуст.", steps);
                }
                var (top, tpos) = stack.Pop();
                if (open.IndexOf(top) != ci)
                {
                    steps.Add(new(i + 1, c, $"Ошибка: ожидалась закрывающая для «{top}»", State()));
                    return new(false, $"Позиция {i + 1}: «{c}» не соответствует «{top}» из позиции {tpos + 1}.", steps);
                }
                steps.Add(new(i + 1, c, $"Pop (пара «{top}»)", State()));
            }
        }

        if (!stack.IsEmpty())
        {
            var (top, tpos) = stack.Top();
            return new(false, $"Не закрыта «{top}» из позиции {tpos + 1}.", steps);
        }
        return new(true, "Скобки расставлены правильно.", steps);
    }
}

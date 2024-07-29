namespace Parser.ExpressionParser.Rules
{
    public interface IRule
    {
        ValueContainer Evaluate();

        string PrettyPrint();
    }
}
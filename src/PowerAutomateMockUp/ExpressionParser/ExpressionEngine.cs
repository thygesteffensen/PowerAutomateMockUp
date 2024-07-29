namespace Parser.ExpressionParser
{
    public interface IExpressionEngine
    {
        string Parse(string input);
        ValueContainer ParseToValueContainer(string input);
    }

    public class ExpressionEngine : IExpressionEngine
    {
        private readonly ExpressionGrammar _expressionGrammar;

        public ExpressionEngine(ExpressionGrammar grammar)
        {
            _expressionGrammar = grammar;
        }

        public string Parse(string input)
        {
            return _expressionGrammar.EvaluateToString(input);
        }

        public ValueContainer ParseToValueContainer(string input)
        {
            return _expressionGrammar.EvaluateToValueContainer(input);
        }
    }
}
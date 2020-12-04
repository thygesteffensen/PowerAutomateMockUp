namespace Parser.ExpressionParser
{
    public interface IExpressionEngine
    {
        string Parse(string input);
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
            return _expressionGrammar.Evaluate(input);
        }
    }
}
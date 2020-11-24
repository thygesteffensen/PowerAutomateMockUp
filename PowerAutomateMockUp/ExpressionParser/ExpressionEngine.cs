namespace Parser.ExpressionParser
{
    public class ExpressionEngine
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
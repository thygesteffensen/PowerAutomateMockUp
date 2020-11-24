namespace Parser.ExpressionParser.Rules
{
    public class StringLiteralRule : IRule
    {
        private readonly ValueContainer _content;

        public StringLiteralRule(ValueContainer content)
        {
            _content = content;
        }

        public ValueContainer Evaluate()
        {
            return _content;
        }

        public string PrettyPrint()
        {
            return $"'{_content}'";
        }
    }
}
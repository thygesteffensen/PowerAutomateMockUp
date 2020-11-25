namespace Parser.ExpressionParser.Rules
{
    public class ConstantRule : IRule
    {
        private readonly ValueContainer _content;

        public ConstantRule(ValueContainer content)
        {
            _content = content;
        }

        public ValueContainer Evaluate()
        {
            return _content;
        }

        public string PrettyPrint()
        {
            return _content.Type() == ValueContainer.ValueType.String? $"'{_content}'" : _content.ToString();
        }
    }
}
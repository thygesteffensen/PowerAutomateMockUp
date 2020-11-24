namespace Parser.ExpressionParser.Rules
{
    public class ArgumentRule : IRule
    {
        private readonly ValueContainer _argument;

        public ArgumentRule(ValueContainer argument)
        {
            _argument = argument;
        }

        public ValueContainer Evaluate()
        {
            return _argument;
        }

        public string PrettyPrint()
        {
            return _argument.ToString();
        }
    }
}
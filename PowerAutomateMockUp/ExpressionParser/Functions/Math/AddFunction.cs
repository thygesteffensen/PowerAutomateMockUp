using Parser.ExpressionParser.Functions.Base;
using Parser.ExpressionParser.Functions.CustomException;

namespace Parser.ExpressionParser.Functions.Math
{
    public class AddFunction : Function
    {
        public AddFunction() : base("add")
        {
        }

        public override ValueContainer ExecuteFunction(params ValueContainer[] parameters)
        {
            if (parameters.Length != 2)
            {
                throw new InvalidTemplateException("Paramters count does not match expected");
            }

            var first = parameters[0].GetValue<float>();
            var second = parameters[1].GetValue<float>();

            return new ValueContainer(first + second);
        }
    }
}
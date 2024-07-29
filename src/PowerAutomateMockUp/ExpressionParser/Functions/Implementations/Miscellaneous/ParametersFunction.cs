using System.Linq;
using Microsoft.Extensions.Options;
using Parser.ExpressionParser.Functions.Base;
using Parser.ExpressionParser.Functions.CustomException;

namespace Parser.ExpressionParser.Functions.Implementations.Miscellaneous
{
    public class ParametersFunction : Function
    {
        private readonly WorkflowParameters _flowParameters;

        public ParametersFunction(IOptions<WorkflowParameters> flowParameters) : base("parameters")
        {
            _flowParameters = flowParameters?.Value;
        }

        public override ValueContainer ExecuteFunction(params ValueContainer[] parameters)
        {
            var paramKey = parameters.FirstOrDefault();

            if (paramKey == null || paramKey.Type() != ValueContainer.ValueType.String)
            {
                throw new ArgumentError("The parameters functions is expected to have one argument of type string");
            }

            if (_flowParameters?.Parameters == null)
            {
                return paramKey;
            }

            if (_flowParameters.Parameters.ContainsKey(paramKey.GetValue<string>()))
            {
                return _flowParameters.Parameters[paramKey.GetValue<string>()];
            }

            throw InvalidTemplateException.BuildInvalidLanguageFunction("ActionName", "parameters");
        }
    }
}
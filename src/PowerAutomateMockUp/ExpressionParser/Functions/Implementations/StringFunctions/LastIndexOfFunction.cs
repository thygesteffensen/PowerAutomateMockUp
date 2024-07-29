using System;
using Parser.ExpressionParser.Functions.Base;
using Parser.ExpressionParser.Functions.CustomException;

namespace Parser.ExpressionParser.Functions.Implementations.StringFunctions
{
    public class LastIndexOfFunction : Function
    {
        public LastIndexOfFunction() : base("lastIndexOf")
        {
        }

        public override ValueContainer ExecuteFunction(params ValueContainer[] parameters)
        {
            if (parameters.Length < 2)
            {
                throw new ArgumentError("Too few arguments");
            }

            return new ValueContainer(AuxiliaryMethods.VcIsString(parameters[0])
                .LastIndexOf(AuxiliaryMethods.VcIsString(parameters[1]), StringComparison.OrdinalIgnoreCase));
        }
    }
}
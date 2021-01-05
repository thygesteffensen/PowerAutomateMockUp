using System;
using System.Collections.Generic;
using System.Linq;
using Parser.ExpressionParser.Functions.Base;
using Parser.ExpressionParser.Functions.CustomException;

namespace Parser.ExpressionParser.Functions.Implementations.StringFunctions
{
    public class LengthFunction : Function
    {
        public LengthFunction() : base("length")
        {
        }

        public override ValueContainer ExecuteFunction(params ValueContainer[] parameters)
        {
            if (parameters.Length != 1)
            {
                throw new ArgumentError(parameters.Length > 1 ? "Too many arguments" : "Too few arguments");
            }

            var item = parameters[0];

            return item.Type() switch
            {
                ValueContainer.ValueType.Array =>
                    new ValueContainer(parameters[0].GetValue<IEnumerable<ValueContainer>>().Count()),
                ValueContainer.ValueType.String => new ValueContainer(parameters[0].GetValue<string>().Length),
                _ => throw new Exception(
                    "The template language function 'length' expects its parameter to be an array or a string. " +
                    $"The provided value is of type '{item.Type()}'. " +
                    "Please see https://aka.ms/logicexpressions#length for usage details.")
            };
        }
    }
}
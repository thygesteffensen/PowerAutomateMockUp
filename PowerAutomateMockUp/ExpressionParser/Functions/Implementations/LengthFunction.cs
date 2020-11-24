using System;
using System.Collections.Generic;
using System.Linq;
using Parser.ExpressionParser.Functions.Base;
using Parser.ExpressionParser.Functions.CustomException;

namespace Parser.ExpressionParser.Functions.Implementations
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

            switch (item.Type())
            {
                case ValueContainer.ValueType.Array:
                    return new ValueContainer(parameters[0].GetValue<IEnumerable<ValueContainer>>().Count().ToString());
                case ValueContainer.ValueType.String:
                    return new ValueContainer(parameters[0].GetValue<string>().Length.ToString());
                default:
                    throw new Exception(
                        "The template language function 'length' expects its parameter to be an array or a string. " +
                        $"The provided value is of type '{item.Type()}'. " +
                        "Please see https://aka.ms/logicexpressions#length for usage details.");
            }
        }
    }
}
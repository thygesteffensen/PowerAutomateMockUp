﻿using Parser.ExpressionParser.Functions.Base;
using Parser.ExpressionParser.Functions.CustomException;

namespace Parser.ExpressionParser.Functions.Implementations.StringFunctions
{
    public class ToUpperFunction : Function
    {
        public ToUpperFunction() : base("toUpper")
        {
        }

        public override ValueContainer ExecuteFunction(params ValueContainer[] parameters)
        {
            if (parameters.Length != 1)
            {
                throw new ArgumentError(parameters.Length > 1 ? "Too many arguments" : "Too few arguments");
            }

            return new ValueContainer(AuxiliaryMethods.VcIsString(parameters[0]).ToUpper());
        }
    }
}
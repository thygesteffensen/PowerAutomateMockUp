namespace Parser.ExpressionParser.Functions.Implementations.StringFunctions
{
    public static class AuxiliaryMethods
    {
        public static string VcIsString(ValueContainer vc)
        {
            return vc.Type() == ValueContainer.ValueType.String
                ? vc.GetValue<string>()
                : throw new PowerAutomateMockUpException("ValueContainer must be of type string.");
        }
    }
}
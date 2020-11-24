using System;
using System.Runtime.Serialization;

namespace Parser.ExpressionParser.Functions.CustomException
{
    public class InvalidTemplateException : Exception
    {
        public InvalidTemplateException()
        {
        }

        protected InvalidTemplateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InvalidTemplateException(string message) : base(message)
        {
        }

        public InvalidTemplateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public static InvalidTemplateException BuildInvalidTemplateException(string actionName, string expression, string key)
        {
            var msg = "InvalidTemplate. Unable to process template language expressions in " +
                      $"action '{actionName}' inputs at line 'x' and column 'y': " +
                      $"'The template language expression '{expression}' " +
                      $"cannot be evaluated because property '{key}' cannot be selected. " +
                      "Array elements can only be selected using an integer index. " +
                      "Please see https://aka.ms/logicexpressions for usage details.'.";

            return new InvalidTemplateException(msg);
        }
    }
}
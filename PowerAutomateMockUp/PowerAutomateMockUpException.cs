using System;
using System.Runtime.Serialization;

namespace Parser
{
    public class PowerAutomateMockUpException : Exception
    {
        public PowerAutomateMockUpException()
        {
        }

        public PowerAutomateMockUpException(string message) : base(message)
        {
        }

        public PowerAutomateMockUpException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PowerAutomateMockUpException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
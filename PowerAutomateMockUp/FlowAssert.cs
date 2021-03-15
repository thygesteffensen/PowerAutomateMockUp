using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Parser.ExpressionParser;
using Parser.FlowParser;

namespace Parser
{
    public static class FlowAssert
    {
        public static void AssertActionWasTriggered(FlowReport flowReport, string actionName, string message = null)
        {
            if (!flowReport.ActionStates.ContainsKey(actionName))
            {
                throw new FlowAssertionException($"Action {actionName} was expected to have been triggered. {message}");
            }
        }

        public static void AssertFlowParameters(
            FlowReport flowReport,
            string actionName,
            params Expression<Func<ValueContainer, bool>>[] expressions)
        {
            // FInd if action have been triggered.
            AssertActionWasTriggered(flowReport, actionName);

            var actionVc = flowReport.ActionStates[actionName].ActionInput;
            if (actionVc == null)
            {
                throw new FlowAssertionException("Action was triggered without parameters");
            }

            var exceptions = new List<Exception>();
            foreach (var expression in expressions)
            {
                try
                {
                    if (expression.Compile().Invoke(actionVc))
                    {
                        exceptions.Add(new FlowAssertionException("One of the predicates didn't match..."));
                    }
                }
                catch (KeyNotFoundException keyNotFoundException)
                {
                    exceptions.Add(new FlowAssertionException($"Action {actionName} did not contain expected input",
                        keyNotFoundException));
                }
            }

            if (exceptions.Count <= 0) return;

            if (exceptions.Count > 1)
            {
                throw new AggregateException(exceptions);
            }

            throw exceptions.First();
        }
    }

    public class FlowAssertionException : Exception
    {
        public FlowAssertionException(string msg) : base(msg)
        {
            Console.WriteLine(msg);
        }

        public FlowAssertionException(string msg, Exception innerException) : base(msg, innerException)
        {
            Console.WriteLine(msg);
        }
    }
}
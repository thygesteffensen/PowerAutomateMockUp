using System;
using Microsoft.Extensions.DependencyInjection;
using Parser.ExpressionParser;
using Parser.ExpressionParser.Functions.Base;
using Parser.ExpressionParser.Functions.Implementations.CollectionFunctions;
using Parser.ExpressionParser.Functions.Implementations.ConversionFunctions;
using Parser.ExpressionParser.Functions.Implementations.LogicalComparisonFunctions;
using Parser.ExpressionParser.Functions.Implementations.StringFunctions;
using Parser.ExpressionParser.Functions.Storage;
using Parser.FlowParser;
using Parser.FlowParser.ActionExecutors;
using Parser.FlowParser.ActionExecutors.Implementations;
using Parser.FlowParser.ActionExecutors.Implementations.ControlActions;
using LengthFunction = Parser.ExpressionParser.Functions.Implementations.StringFunctions.LengthFunction;

namespace Parser
{
    public static class FlowRunnerDependencyExtension
    {
        public static void AddFlowRunner(this IServiceCollection services)
        {
            services.AddScoped<IFlowRunner, FlowRunner>();

            services.AddScoped<IActionExecutorFactory, ActionExecutorFactory>();
            services.AddScoped<IScopeDepthManager, ScopeDepthManager>();

            services.AddScoped<IState, State>();
            services.AddScoped<IVariableRetriever>(x => x.GetRequiredService<IState>());
            services.AddScoped<IOutputsRetriever>(x => x.GetRequiredService<IState>());
            services.AddScoped<ITriggerOutputsRetriever>(x => x.GetRequiredService<IState>());
            services.AddScoped<IItemsRetriever>(x => x.GetRequiredService<IState>());
            services.AddScoped<IExpressionEngine, ExpressionEngine>();
            services.AddScoped<ExpressionGrammar>();

            AddStringFunctions(services);
            AddCollectionFunction(services);
            AddConversionFunction(services);

            services.AddTransient<IFunction, VariablesFunction>();
            services.AddTransient<IFunction, OutputsFunction>();
            services.AddTransient<IFunction, TriggerOutputsFunctions>();
            services.AddTransient<IFunction, ItemsFunction>();

            services.AddTransient<IFunction, LengthFunction>();
            services.AddTransient<IFunction, GreaterFunction>();

            services.AddFlowActionByFlowType<IfActionExecutor>("If");
            services.AddFlowActionByFlowType<ScopeActionExecutor>("Scope");
            services.AddFlowActionByFlowType<TerminateActionExecutor>("Terminate");
            services.AddFlowActionByFlowType<ForEachActionExecutor>("Foreach");
            services.AddFlowActionByFlowType<DoUntilActionExecutor>("Until");
            services.AddFlowActionByFlowType<SwitchActionExecutor>("Switch");

            services.AddLogging();
        }

        private static void AddStringFunctions(IServiceCollection services)
        {
            services.AddTransient<IFunction, ConcatFunction>();
            services.AddTransient<IFunction, EndsWithFunction>();
            services.AddTransient<IFunction, FormatNumberFunction>();
            services.AddTransient<IFunction, GuidFunction>();
            services.AddTransient<IFunction, IndexOfFunction>();
            services.AddTransient<IFunction, LastIndexOfFunction>();
            services.AddTransient<IFunction, LengthFunction>();
            services.AddTransient<IFunction, ReplaceFunction>();
            services.AddTransient<IFunction, SplitFunction>();
            services.AddTransient<IFunction, StartsWithFunction>();
            services.AddTransient<IFunction, SubstringFunction>();
            services.AddTransient<IFunction, ToLowerFunction>();
            services.AddTransient<IFunction, ToUpperFunction>();
            services.AddTransient<IFunction, TrimFunction>();
        }

        private static void AddCollectionFunction(IServiceCollection services)
        {
            services.AddTransient<IFunction, ContainsFunction>();
            services.AddTransient<IFunction, EmptyFunction>();
            services.AddTransient<IFunction, FirstFunction>();
            services.AddTransient<IFunction, InterSectionFunction>();
            services.AddTransient<IFunction, JoinFunction>();
            services.AddTransient<IFunction, LastFunction>();
            services.AddTransient<IFunction, LengthFunction>();
            services.AddTransient<IFunction, SkipFunction>();
            services.AddTransient<IFunction, TakeFunction>();
            services.AddTransient<IFunction, UnionFunction>();
        }

        private static void AddConversionFunction(IServiceCollection services)
        {
            services.AddTransient<IFunction, ArrayFunction>();
            services.AddTransient<IFunction, Base64Function>();
            services.AddTransient<IFunction, Base64ToBinaryFunction>();
            services.AddTransient<IFunction, Base64ToStringFunction>();
            services.AddTransient<IFunction, BinaryFunction>();
            services.AddTransient<IFunction, BoolFunction>();
            services.AddTransient<IFunction, CreateArrayFunction>();
            services.AddTransient<IFunction, DataUriFunction>();
            services.AddTransient<IFunction, DataUriToBinaryFunction>();
        }

        public static void AddFlowActionByName<T>(this IServiceCollection services, string actionName)
            where T : ActionExecutorBase
        {
            services.AddTransient<T>();
            services.AddSingleton(new ActionExecutorRegistration {ActionName = actionName, Type = typeof(T)});
        }

        public static void AddFlowActionByFlowType<T>(this IServiceCollection services, string actionType)
            where T : ActionExecutorBase
        {
            services.AddTransient<T>();
            services.AddSingleton(new ActionExecutorRegistration {ActionType = actionType, Type = typeof(T)});
        }

        public static void AddFlowActionByApiIdAndOperationsName<T>(this IServiceCollection services, string apiId,
            string[] supportedOperationNames) where T : ActionExecutorBase
        {
            services.AddTransient<T>();
            services.AddSingleton(new ActionExecutorRegistration
            {
                ActionApiId = apiId,
                SupportedOperationNames = supportedOperationNames,
                Type = typeof(T)
            });
        }
    }
}
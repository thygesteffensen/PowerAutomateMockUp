using System;
using Microsoft.Extensions.DependencyInjection;
using Parser.ExpressionParser.Functions.Implementations;
using Parser.ExpressionParser.Functions.Implementations.StringFunctions;
using Parser.ExpressionParser.Functions.Storage;
using Parser.FlowParser;
using Parser.FlowParser.ActionExecutors;
using Parser.FlowParser.ActionExecutors.Implementations;

namespace Parser.ExpressionParser.Functions.Base
{
    public static class FlowRunnerDependencyExtension
    {
        // TODO: Her kan vi give en option med, så de kan sige hvad de gerne vil have med. 
        // public static void AddFlowRunner(this IServiceCollection services, FlowSettings flowSettings)
        public static void AddFlowRunner(this IServiceCollection services)
        {
            services.AddSingleton<FlowRunner>();

            services.AddSingleton<ActionExecutorFactory>();
            services.AddSingleton<IScopeDepthManager, ScopeDepthManager>();

            services.AddScoped<IState, State>();
            services.AddScoped<IVariableRetriever>(x => x.GetRequiredService<IState>());
            services.AddScoped<IOutputsRetriever>(x => x.GetRequiredService<IState>());
            services.AddScoped<ITriggerOutputsRetriever>(x => x.GetRequiredService<IState>());
            services.AddScoped<IExpressionEngine, ExpressionEngine>();
            services.AddScoped<ExpressionGrammar>();

            AddStringFunctions(services);

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

        public static void AddFlowActionByName<T>(this IServiceCollection services, string actionName)
            where T : ActionExecutorBase
        {
            services.AddTransient<T>();
            services.AddSingleton(new ActionExecutorRegistration {ActionName = actionName, Type = typeof(T)});
        }

        private static void AddFlowActionByFlowType<T>(this IServiceCollection services, string actionType)
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
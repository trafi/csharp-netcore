using System;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using OpenTracing.Contrib.NetCore.Internal;
using OpenTracing.Tag;

namespace OpenTracing.Contrib.NetCore.AspNetCore
{
    internal sealed class MvcEventProcessor
    {
        private const string ActionTagActionName = "action";
        private const string ActionTagControllerName = "controller";

        private const string ResultTagType = "result.type";

        private static readonly PropertyFetcher _beforeAction_ActionDescriptorFetcher = new PropertyFetcher("actionDescriptor");
        private static readonly PropertyFetcher _beforeActionResult_ResultFetcher = new PropertyFetcher("result");

        private readonly ITracer _tracer;
        private readonly ILogger _logger;
        private readonly MvcOptions _options;

        public MvcEventProcessor(ITracer tracer, ILogger logger, MvcOptions options)
        {
            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));;
        }

        public bool ProcessEvent(string eventName, object arg)
        {
            switch (eventName)
            {
                case "Microsoft.AspNetCore.Mvc.BeforeAction":
                    {
                        // NOTE: This event is the start of the action pipeline. The action has been selected, the route
                        //       has been selected but no filters have run and model binding hasn't occured.

                        var actionDescriptor = (ActionDescriptor)_beforeAction_ActionDescriptorFetcher.Fetch(arg);
                        var controllerActionDescriptor = actionDescriptor as ControllerActionDescriptor;

                        string operationName = _options.ActionOperationNameResolver(actionDescriptor);

                        IScope scope = _tracer.BuildSpan(operationName)
                            .WithTag(Tags.Component, _options.ActionComponentName)
                            .WithTag(ActionTagControllerName, controllerActionDescriptor?.ControllerTypeInfo.FullName)
                            .WithTag(ActionTagActionName, controllerActionDescriptor?.ActionName)
                            .StartActive();

                        _options.OnAction?.Invoke(scope.Span, actionDescriptor);
                    }
                    return true;

                case "Microsoft.AspNetCore.Mvc.AfterAction":
                    {
                        _tracer.ScopeManager.Active?.Dispose();
                    }
                    return true;

                case "Microsoft.AspNetCore.Mvc.BeforeActionResult":
                    {
                        // NOTE: This event is the start of the result pipeline. The action has been executed, but
                        //       we haven't yet determined which view (if any) will handle the request

                        object result = _beforeActionResult_ResultFetcher.Fetch(arg);
                        string resultType = result.GetType().Name;

                        string operationName = _options.ResultOperationNameResolver(result);

                        IScope scope = _tracer.BuildSpan(operationName)
                            .WithTag(Tags.Component, _options.ResultComponentName)
                            .WithTag(ResultTagType, resultType)
                            .StartActive();

                        _options.OnResult?.Invoke(scope.Span, result);
                    }
                    return true;

                case "Microsoft.AspNetCore.Mvc.AfterActionResult":
                    {
                        _tracer.ScopeManager.Active?.Dispose();
                    }
                    return true;

                default: return false;
            }
        }
    }
}

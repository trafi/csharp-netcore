using System;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace OpenTracing.Contrib.NetCore.AspNetCore
{
    public class MvcOptions
    {
        public const string DefaultActionComponent = "AspNetCore.MvcAction";
        public const string DefaultResultComponent = "AspNetCore.MvcResult";

        private string _actionComponentName = DefaultActionComponent;
        private string _resultComponentName = DefaultResultComponent;

        /// <summary>
        /// Allows changing the "component" tag of created action spans.
        /// </summary>
        public string ActionComponentName
        {
            get => _actionComponentName;
            set => _actionComponentName = value ?? throw new ArgumentNullException(nameof(ActionComponentName));
        }

        /// <summary>
        /// Allows changing the "component" tag of created result spans.
        /// </summary>
        public string ResultComponentName
        {
            get => _resultComponentName;
            set => _resultComponentName = value ?? throw new ArgumentNullException(nameof(ResultComponentName));
        }

        private static readonly Func<ActionDescriptor, string> DefaultActionOperationNameResolver = actionDescriptor =>
            actionDescriptor is ControllerActionDescriptor controllerActionDescriptor
                ? $"Action {controllerActionDescriptor.ControllerTypeInfo.FullName}/{controllerActionDescriptor.ActionName}"
                : $"Action {actionDescriptor.DisplayName}";

        private Func<ActionDescriptor, string> _actionOperationNameResolver;

        /// <summary>
        /// A delegate that returns the OpenTracing "operation name" for the given ActionDescriptor.
        /// </summary>
        public Func<ActionDescriptor, string> ActionOperationNameResolver
        {
            get => _actionOperationNameResolver ?? (_actionOperationNameResolver = DefaultActionOperationNameResolver);
            set => _actionOperationNameResolver = value ?? throw new ArgumentNullException(nameof(ActionOperationNameResolver));
        }

        private static readonly Func<object, string> DefaultResultOperationNameResolver = result =>
        {
            var resultType = result.GetType().Name;
            return $"Result {resultType}";
        };

        private Func<object, string> _resultOperationNameResolver;

        /// <summary>
        /// A delegate that returns the OpenTracing "operation name" for the given result.
        /// </summary>
        public Func<object, string> ResultOperationNameResolver
        {
            get => _resultOperationNameResolver ?? (_resultOperationNameResolver = DefaultResultOperationNameResolver);
            set => _resultOperationNameResolver = value ?? throw new ArgumentNullException(nameof(ResultOperationNameResolver));
        }

        /// <summary>
        /// Allows the modification of the created action span to e.g. add further tags.
        /// </summary>
        public Action<ISpan, ActionDescriptor> OnAction { get; set; }

        /// <summary>
        /// Allows the modification of the created result span to e.g. add further tags.
        /// </summary>
        public Action<ISpan, object> OnResult { get; set; }
    }
}

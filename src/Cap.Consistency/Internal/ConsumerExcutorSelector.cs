﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Cap.Consistency.Abstractions;
using Cap.Consistency.Attributes;
using Cap.Consistency.Consumer;
using Cap.Consistency.Infrastructure;
using Cap.Consistency.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Cap.Consistency.Internal
{
    public class ConsumerExcutorSelector : IConsumerExcutorSelector
    {
        private readonly IServiceProvider _serviceProvider;

        public ConsumerExcutorSelector(IServiceProvider serviceProvider) {
            _serviceProvider = serviceProvider;
        }

        public ConsumerExecutorDescriptor SelectBestCandidate(string key, IReadOnlyList<ConsumerExecutorDescriptor> executeDescriptor) {
            return executeDescriptor.FirstOrDefault(x => x.Topic.Name == key);
        }

        public IReadOnlyList<ConsumerExecutorDescriptor> SelectCandidates(TopicRouteContext context) {

            var consumerServices = context.ServiceProvider.GetServices<IConsumerService>();

            var executorDescriptorList = new List<ConsumerExecutorDescriptor>();
            foreach (var service in consumerServices) {
                var typeInfo = service.GetType().GetTypeInfo();
                if (!typeof(IConsumerService).GetTypeInfo().IsAssignableFrom(typeInfo)) {
                    continue;
                }

                foreach (var method in typeInfo.DeclaredMethods) {

                    var topicAttr = method.GetCustomAttribute<TopicAttribute>(true);
                    if (topicAttr == null) continue;

                    executorDescriptorList.Add(InitDescriptor(topicAttr, method, typeInfo));
                }
            }

            return executorDescriptorList;
        }
        private ConsumerExecutorDescriptor InitDescriptor(TopicAttribute attr,
            MethodInfo methodInfo, TypeInfo implType
            ) {
            var descriptor = new ConsumerExecutorDescriptor();

            descriptor.Topic = new TopicInfo(attr.Name);
            descriptor.MethodInfo = methodInfo;
            descriptor.ImplTypeInfo = implType;

            return descriptor;
        }
    }
}

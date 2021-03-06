﻿// Copyright 2007-2017 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the
// License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, either express or implied. See the License for the
// specific language governing permissions and limitations under the License.
namespace MassTransit.SimpleInjectorIntegration.ScopeProviders
{
    using Context;
    using GreenPipes;
    using Scoping;
    using Scoping.ConsumerContexts;
    using SimpleInjector;
    using SimpleInjector.Lifestyles;
    using Util;


    public class SimpleInjectorConsumerScopeProvider :
        IConsumerScopeProvider
    {
        readonly Container _container;

        public SimpleInjectorConsumerScopeProvider(Container container)
        {
            _container = container;
        }

        public void Probe(ProbeContext context)
        {
            context.Add("provider", "simpleInjector");
        }

        public IConsumerScopeContext GetScope(ConsumeContext context)
        {
            if (context.TryGetPayload<Scope>(out var existingScope))
            {
                existingScope.UpdateScope(context);

                return new ExistingConsumerScopeContext(context);
            }

            var scope = AsyncScopedLifestyle.BeginScope(_container);
            try
            {
                scope.UpdateScope(context);

                var proxy = new ConsumeContextProxyScope(context);

                proxy.UpdatePayload(scope);

                return new CreatedConsumerScopeContext<Scope>(scope, proxy);
            }
            catch
            {
                scope.Dispose();

                throw;
            }
        }

        public IConsumerScopeContext<TConsumer, T> GetScope<TConsumer, T>(ConsumeContext<T> context) where TConsumer : class where T : class
        {
            if (context.TryGetPayload<Scope>(out var existingScope))
            {
                existingScope.UpdateScope(context);

                var consumer = existingScope.Container.GetInstance<TConsumer>();
                if (consumer == null)
                    throw new ConsumerException($"Unable to resolve consumer type '{TypeMetadataCache<TConsumer>.ShortName}'.");

                ConsumerConsumeContext<TConsumer, T> consumerContext = context.PushConsumer(consumer);

                return new ExistingConsumerScopeContext<TConsumer, T>(consumerContext);
            }

            var scope = AsyncScopedLifestyle.BeginScope(_container);
            try
            {
                scope.UpdateScope(context);

                var consumer = scope.Container.GetInstance<TConsumer>();
                if (consumer == null)
                    throw new ConsumerException($"Unable to resolve consumer type '{TypeMetadataCache<TConsumer>.ShortName}'.");

                ConsumerConsumeContext<TConsumer, T> consumerContext = context.PushConsumerScope(consumer, scope);

                consumerContext.UpdatePayload(scope);

                return new CreatedConsumerScopeContext<Scope, TConsumer, T>(scope, consumerContext);
            }
            catch
            {
                scope.Dispose();

                throw;
            }
        }
    }
}

﻿using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Internal;
using Volo.Abp.Modularity;

namespace Volo.Abp
{
    public abstract class AbpApplicationBase : IAbpApplication
    {
        [NotNull]
        public Type StartupModuleType { get; }

        public IServiceProvider ServiceProvider { get; protected set; }

        [NotNull]
        public AbpModuleDescriptor[] Modules { get; }

        internal AbpApplicationBase(
            [NotNull] Type startupModuleType,
            [NotNull] IServiceCollection services,
            [CanBeNull] Action<AbpApplicationCreationOptions> optionsAction)
        {
            Check.NotNull(startupModuleType, nameof(startupModuleType));
            Check.NotNull(services, nameof(services));

            StartupModuleType = startupModuleType;

            var options = new AbpApplicationCreationOptions(services);
            optionsAction?.Invoke(options);

            services.AddSingleton<IAbpApplication>(_ => this);
            services.AddCoreAbpServices();

            Modules = LoadModules(services, options);
        }

        private AbpModuleDescriptor[] LoadModules(IServiceCollection services, AbpApplicationCreationOptions options)
        {
            return services
                .GetSingletonInstance<IModuleLoader>()
                .LoadModules(
                    services,
                    StartupModuleType,
                    options.PlugInSources
                );
        }

        public void Shutdown() //TODO: Why not disposable?
        {
            ServiceProvider
                .GetRequiredService<IModuleManager>()
                .ShutdownModules(new ApplicationShutdownContext());
        }
    }
}
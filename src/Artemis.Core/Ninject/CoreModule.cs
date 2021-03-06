﻿using System.IO;
using Artemis.Core.Services;
using Artemis.Storage;
using Artemis.Storage.Migrations.Interfaces;
using Artemis.Storage.Repositories.Interfaces;
using LiteDB;
using Ninject.Activation;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Ninject.Planning.Bindings.Resolvers;
using Serilog;

namespace Artemis.Core.Ninject
{
    /// <summary>
    ///     The main <see cref="NinjectModule" /> of the Artemis Core that binds all services
    /// </summary>
    public class CoreModule : NinjectModule
    {
        /// <inheritdoc />
        public override void Load()
        {
            if (Kernel == null)
                throw new ArtemisCoreException("Failed to bind Ninject Core module, kernel is null.");

            Kernel.Components.Remove<IMissingBindingResolver, SelfBindingResolver>();

            // Bind all services as singletons
            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .IncludingNonPublicTypes()
                    .SelectAllClasses()
                    .InheritedFrom<IArtemisService>()
                    .BindAllInterfaces()
                    .Configure(c => c.InSingletonScope());
            });

            // Bind all protected services as singletons
            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .IncludingNonPublicTypes()
                    .SelectAllClasses()
                    .InheritedFrom<IProtectedArtemisService>()
                    .BindAllInterfaces()
                    .Configure(c => c.When(HasAccessToProtectedService).InSingletonScope());
            });

            Kernel.Bind<LiteRepository>().ToMethod(t =>
            {
                try
                {
                    return new LiteRepository(Constants.ConnectionString);
                }
                catch (LiteException e)
                {
                    // I don't like this way of error reporting, now I need to use reflection if I want a meaningful error code
                    if (e.ErrorCode != LiteException.INVALID_DATABASE)
                        throw new ArtemisCoreException($"LiteDB threw error code {e.ErrorCode}. See inner exception for more details", e);

                    // If the DB is invalid it's probably LiteDB v4 (TODO: we'll have to do something better later)
                    File.Delete($"{Constants.DataFolder}\\database.db");
                    return new LiteRepository(Constants.ConnectionString);
                }
            }).InSingletonScope();

            Kernel.Bind<StorageMigrationService>().ToSelf().InSingletonScope();

            // Bind all migrations as singletons
            Kernel.Bind(x =>
            {
                x.FromAssemblyContaining<IStorageMigration>()
                    .IncludingNonPublicTypes()
                    .SelectAllClasses()
                    .InheritedFrom<IStorageMigration>()
                    .BindAllInterfaces()
                    .Configure(c => c.InSingletonScope());
            });

            // Bind all repositories as singletons
            Kernel.Bind(x =>
            {
                x.FromAssemblyContaining<IRepository>()
                    .IncludingNonPublicTypes()
                    .SelectAllClasses()
                    .InheritedFrom<IRepository>()
                    .BindAllInterfaces()
                    .Configure(c => c.InSingletonScope());
            });

            Kernel.Bind<PluginSettings>().ToProvider<PluginSettingsProvider>();
            Kernel.Bind<ILogger>().ToProvider<LoggerProvider>();
            Kernel.Bind<LoggerProvider>().ToSelf();
        }

        private bool HasAccessToProtectedService(IRequest r)
        {
            return r.ParentRequest != null && !r.ParentRequest.Service.Assembly.Location.StartsWith(Path.Combine(Constants.DataFolder, "plugins"));
        }
    }
}
using BaxterStore.Data.Exceptions;
using BaxterStore.Data.Implementation;
using BaxterStore.Data.Implementation.Users;
using BaxterStore.Data.Interfaces;
using BaxterStore.Data.Migrations;
using BaxterStore.Data.POCOs;
using BaxterStore.Data.POCOs.Users;
using BaxterStore.Service.Implementation.Users;
using BaxterStore.Service.Interfaces;
using BaxterStore.Service.POCOs;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BaxterStore.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddControllers();

            services.AddSingleton<ICacheHandler<UserDataEntity>, CacheHandler<UserDataEntity>>();
            services.AddSingleton<IMapper<User, UserDataEntity>, UserMapper>();
            services.AddSingleton<IUserService, UserService>();

            var databaseConfiguration = new DatabaseConfiguration();
            Configuration.GetSection("DatabaseConfiguration").Bind(databaseConfiguration);
            services.AddSingleton(databaseConfiguration);

            services.AddFluentMigrator(databaseConfiguration.ConnectionString);

            var userTableConfiguration = new TableConfiguration();
            Configuration.GetSection("TableConfiguration:Users").Bind(userTableConfiguration);
            services.AddSingleton<ICrudRepository<UserDataEntity>, UserRepository>(sp => new UserRepository(databaseConfiguration, userTableConfiguration, sp.GetRequiredService<ICacheHandler<UserDataEntity>>()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IMigrationRunner runner)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseMiddleware(typeof(DataRepositoryErrorHandlerMiddleware));

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            runner.MigrateUp();
        }

    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFluentMigrator(this IServiceCollection services, string connectionString)
        {
            var migrationAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t.IsClass && t.Namespace == "BaxterStore.Data.Migrations")
                .Select(x => x.Assembly).ToArray();

            services.AddFluentMigratorCore()
                .ConfigureRunner(runner => runner.AddSqlServer()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(migrationAssemblies).For.Migrations());

            return services;
        }
    }
}

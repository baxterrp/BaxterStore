using BaxterStore.Data.Implementation.Users;
using BaxterStore.Data.Interfaces;
using BaxterStore.Data.POCOs;
using BaxterStore.Data.POCOs.Users;
using BaxterStore.Service.Implementation.Users;
using BaxterStore.Service.Interfaces;
using BaxterStore.Service.POCOs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
            var databaseConfiguration = new DatabaseConfiguration();
            Configuration.GetSection("DatabaseConfiguration").Bind(databaseConfiguration);
            services.AddSingleton(databaseConfiguration);

            services.AddSingleton<IMapper<User, UserDataEntity>, UserMapper>();
            services.AddSingleton<IUserService, UserService>();

            var userTableConfiguration = new TableConfiguration();
            Configuration.GetSection("TableConfiguration:Users").Bind(userTableConfiguration);
            services.AddSingleton<ICrudRepository<UserDataEntity>, UserRepository>(sp => new UserRepository(databaseConfiguration, userTableConfiguration));

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

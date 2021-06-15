using System.Reflection;
using Common;
using Front.Controllers;
using MediatR;
using MediatR.Extensions.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using RabitQueueWrapper;
using XapoWrappers;

namespace Front
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
            services.AddControllers();
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "Front", Version = "v1"}); });

            services.Configure<RabbitOptions>(Configuration.GetSection(RabbitOptions.SectionName));
            services.AddScoped<IXapoQueue, RabbitQueueWrapper>();
            services.AddScoped<IJsonSerializer, NewtonSerializer>();

            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddFluentValidation(new[] {Assembly.GetExecutingAssembly()});
            services.AddTransient<ValidationExceptionMiddleware>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Front v1"));
            }
            app.UseMiddleware<ValidationExceptionMiddleware>();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();


            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
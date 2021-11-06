using System.Reflection;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using PdfFiller.API.Controllers;
using Swashbuckle.AspNetCore.SwaggerGen;
using ApiVersion = Microsoft.AspNetCore.Mvc.ApiVersion;

namespace PdfFiller.API
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
            services.AddApiVersioning(o =>
            {
                o.DefaultApiVersion = ApiVersion.Default;
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.ReportApiVersions = true;
            });
            services.AddControllers().AddFluentValidation(fv=>fv.RegisterValidatorsFromAssembly(typeof(Startup).GetTypeInfo().Assembly));
            services.AddAppServices(Configuration);
            services.AddMediatR(typeof(Startup).GetTypeInfo().Assembly);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "FillPdf API", Version = "v1" });
                c.OperationFilter<ProblemDetailsOperationFilter>();

            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FillPdf API v1"));
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
    public class ProblemDetailsOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            foreach (var operationResponse in operation.Responses)
            {
                if (operationResponse.Key.StartsWith("2"))
                {
                    operationResponse.Value.Content.Remove("application/json");
                }
                else
                {
                    operationResponse.Value.Content.Remove("application/pdf");
                }
            }
        }
    }

}

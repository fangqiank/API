using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Routing.Api.Data;
using Routing.Api.Services;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json.Serialization;

namespace Routing.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        
        public void ConfigureServices(IServiceCollection services)
        {
            //缓存
            services.AddResponseCaching();

            services.AddControllers(opt =>
                {
                    opt.ReturnHttpNotAcceptable = true; //请求类型和返回类型不一致时，返回406
                    //opt.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter()); //返回类型是xml
                    //opt.OutputFormatters.Insert(0,new XmlDataContractSerializerOutputFormatter()); //调整默认格式顺序

                    opt.CacheProfiles.Add("120sCacheProfile",new CacheProfile
                    {
                        Duration = 120
                    });
                }).AddNewtonsoftJson(opt =>
                {
                    opt.SerializerSettings.ContractResolver=new CamelCasePropertyNamesContractResolver();
                })
                .AddXmlDataContractSerializerFormatters() //asp.net core 3.0后的写法
                
                //自定义错误报告
                .ConfigureApiBehaviorOptions(opt =>
                {
                    opt.InvalidModelStateResponseFactory = context =>
                    {
                        var problemDetails = new ValidationProblemDetails(context.ModelState)
                        {
                            Type = "https://www.google.com",
                            Title = "有错误",
                            Status = StatusCodes.Status422UnprocessableEntity,
                            Detail = "请看详细信息",
                            Instance = context.HttpContext.Request.Path
                        };

                        problemDetails.Extensions.Add("traceId",context.HttpContext.TraceIdentifier);

                        return new UnprocessableEntityObjectResult(problemDetails)
                        {
                            ContentTypes = {"application/problem+json"}
                        };
                    };
                });

            //全局注册vendor-specific media type:application/vnd.company.hateoas+json
            services.Configure<MvcOptions>(config =>
            {
                var newtonSoftJsonOutputFormatter = config.OutputFormatters
                    .OfType<NewtonsoftJsonOutputFormatter>()?
                    .FirstOrDefault();

                if(newtonSoftJsonOutputFormatter!=null)
                    newtonSoftJsonOutputFormatter.SupportedMediaTypes.Add(
                        "application/vnd.company.hateoas+json");
            });

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies()); //Auto Mapper


            services.AddScoped<ICompanyRepository, CompanyRepository>();

            services.AddDbContext<RoutingDbContext>(opt =>
            {
                opt.UseSqlite("Data Source=routine.db");
            });

            services.AddTransient<IPropertyMappingService, PropertyMappingService>();

            services.AddTransient<IPropertyCheckService, PropertyCheckService>();
        }

        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(opt =>
                {
                    opt.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("Unexpected Error");
                    });
                });
            }

            app.UseResponseCaching();//缓存

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

            });
        }
    }
}

using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using Routing.Api.Data;
using Routing.Api.Services;
using System;
using System.Linq;

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
            services.AddHttpCacheHeaders(opt =>
            {
                opt.MaxAge = 60;
                opt.CacheLocation = CacheLocation.Private;
            }, validation =>
            {
                validation.MustRevalidate = true;
            });//Etag

            //缓存
            services.AddResponseCaching();

            //services.AddMvc();// Mvc, addcontrollers:no view,for api

            services.AddControllers(opt =>
                {
                    opt.ReturnHttpNotAcceptable = true; //请求类型和返回类型不一致时，返回406
                    //opt.InputFormatters.Add(new XmlDataContractSerializerInputFormatter());
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
                /* //fluent validation
                .AddFluentValidation(fv =>
                {
                    // 混用规则。
                    fv.RunDefaultMvcValidationAfterFluentValidationExecutes = true;
                    fv.RegisterValidatorsFromAssembly(Assembly.GetAssembly(this.GetType()));
                })*/
                .AddXmlDataContractSerializerFormatters() //asp.net core 3.0后的写法,输入和输出
                
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

                newtonSoftJsonOutputFormatter?.SupportedMediaTypes.Add(
                    "application/vnd.company.hateoas+json");
            });

            //services.AddSwaggerGen(x =>
            //{
            //    x.SwaggerDoc("v1", new OpenApiInfo
            //    {
            //        Title = "My Web Api",
            //        Version = "v1"
            //    });
            //    x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            //    {
            //        Description = "JWT Authorization",
            //        Name = "Authorization",
            //        In = ParameterLocation.Header,
            //        Type = SecuritySchemeType.ApiKey,
            //        Scheme = "Bearer"
            //    });
            //    x.AddSecurityRequirement(new OpenApiSecurityRequirement
            //    {
            //        {
            //            new OpenApiSecurityScheme
            //            {
            //                Reference = new OpenApiReference
            //                {
            //                    Type = ReferenceType.SecurityScheme,
            //                    Id = "Bearer"
            //                }
            //            },
            //            new string[]
            //            {

            //            }
            //        }

            //    });
            //});

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies()); //Auto Mapper,参数是程序集的数组


            services.AddScoped<ICompanyRepository, CompanyRepository>(); //addscoped 每一次http请求

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
                app.UseExceptionHandler(opt => //定义Exception
                {
                    opt.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("Unexpected Error");
                    });
                });
            }

            //app.UseResponseCaching();//缓存,微软没有实现验证模型

            app.UseHttpCacheHeaders();

            app.UseRouting();

            app.UseAuthorization();

            //app.UseSwagger();

            //app.UseSwaggerUI(opt =>
            //{
            //    opt.SwaggerEndpoint("/swagger/v1/swagger.json", "My Api v1");
            //});

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

            });
        }
    }
}

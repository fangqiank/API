using AutoMapper;
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

            //����
            services.AddResponseCaching();

            //services.AddMvc();// Mvc, addcontrollers:no view,for api

            services.AddControllers(opt =>
                {
                    opt.ReturnHttpNotAcceptable = true; //�������ͺͷ������Ͳ�һ��ʱ������406
                    //opt.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter()); //����������xml
                    //opt.OutputFormatters.Insert(0,new XmlDataContractSerializerOutputFormatter()); //����Ĭ�ϸ�ʽ˳��

                    opt.CacheProfiles.Add("120sCacheProfile",new CacheProfile
                    {
                        Duration = 120
                    });
                }).AddNewtonsoftJson(opt =>
                {
                    opt.SerializerSettings.ContractResolver=new CamelCasePropertyNamesContractResolver();
                })
                .AddXmlDataContractSerializerFormatters() //asp.net core 3.0���д��,��������
                
                //�Զ�����󱨸�
                .ConfigureApiBehaviorOptions(opt =>
                {
                    opt.InvalidModelStateResponseFactory = context =>
                    {
                        var problemDetails = new ValidationProblemDetails(context.ModelState)
                        {
                            Type = "https://www.google.com",
                            Title = "�д���",
                            Status = StatusCodes.Status422UnprocessableEntity,
                            Detail = "�뿴��ϸ��Ϣ",
                            Instance = context.HttpContext.Request.Path
                        };

                        problemDetails.Extensions.Add("traceId",context.HttpContext.TraceIdentifier);

                        return new UnprocessableEntityObjectResult(problemDetails)
                        {
                            ContentTypes = {"application/problem+json"}
                        };
                    };
                });

            //ȫ��ע��vendor-specific media type:application/vnd.company.hateoas+json
            services.Configure<MvcOptions>(config =>
            {
                var newtonSoftJsonOutputFormatter = config.OutputFormatters
                    .OfType<NewtonsoftJsonOutputFormatter>()?
                    .FirstOrDefault();

                if(newtonSoftJsonOutputFormatter!=null)
                    newtonSoftJsonOutputFormatter.SupportedMediaTypes.Add(
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

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies()); //Auto Mapper,�����ǳ��򼯵�����


            services.AddScoped<ICompanyRepository, CompanyRepository>(); //addscope ÿһ��http����

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

            //app.UseResponseCaching();//����,΢��û��ʵ����֤ģ��

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

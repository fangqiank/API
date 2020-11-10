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
using Microsoft.AspNetCore.Http;

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
            services.AddControllers(opt =>
            {
                opt.ReturnHttpNotAcceptable = true;  //�������ͺͷ������Ͳ�һ��ʱ������406
                //opt.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter()); //����������xml
                //opt.OutputFormatters.Insert(0,new XmlDataContractSerializerOutputFormatter()); //����Ĭ�ϸ�ʽ˳��
                
            }).AddXmlDataContractSerializerFormatters(); //asp.net core 3.0���д��

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies()); //Auto Mapper


            services.AddScoped<ICompanyRepository, CompanyRepository>();

            services.AddDbContext<RoutingDbContext>(opt =>
            {
                opt.UseSqlite("Data Source=routine.db");
            });
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

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

            });
        }
    }
}

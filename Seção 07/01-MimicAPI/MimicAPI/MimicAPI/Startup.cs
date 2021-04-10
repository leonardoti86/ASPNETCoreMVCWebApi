using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using MimicAPI.Database;
using MimicAPI.Helpers;
using MimicAPI.Helpers.Swagger;
using MimicAPI.v1.Repositories;
using MimicAPI.v1.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MimicAPI
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //injeta MVC
            services.AddMvc();

            //injeta EF usando o Sqlite
            services.AddDbContext<MimicContext>(opt =>
            {
                opt.UseSqlite("Data source=Database\\Mimic.db"); 
            });

            //injeta no escopo a relação dessa interface com classe
            services.AddScoped<IPalavraRepository,PalavraRepository>();

            //Configura AutoMapper
            var config = new MapperConfiguration(cfg => {
                cfg.AddProfile(new DTOMapperProfile());
            });
            IMapper mapper = config.CreateMapper();
            services.AddSingleton(mapper); // uma única instancia na aplicação utilizando esse "mapeador"

            services.AddApiVersioning(cfg =>
            {
                cfg.ReportApiVersions = true; //da a opção ao usuario de consumir versões compatíveis
                //cfg.ApiVersionReader = new HeaderApiVersionReader("api-version"); //nome da variavel que chega pelo request contendo a versão escolhida
                cfg.AssumeDefaultVersionWhenUnspecified = true; // se usuario não especificar versão usa-se a default(especificada abaixo)
                cfg.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0); // interessante qdo a versão vier no cabeçalho
            });

            //adicionando o Swagger
            services.AddSwaggerGen(cfg => {
                cfg.ResolveConflictingActions(apiDescription => apiDescription.First());
                cfg.SwaggerDoc("v2.0", new Swashbuckle.AspNetCore.Swagger.Info()
                {
                    Title = "MimicAPI - v2.0",
                    Version = "v2.0"
                });

                cfg.SwaggerDoc("v1.0", new Swashbuckle.AspNetCore.Swagger.Info() { 
                    Title = "MimicAPI - v1.0",
                    Version = "v1.0"
                });
                cfg.SwaggerDoc("v1.1", new Swashbuckle.AspNetCore.Swagger.Info()
                {
                    Title = "MimicAPI - v1.1",
                    Version = "v1.1"
                });

                var caminhoProjeto = PlatformServices.Default.Application.ApplicationBasePath;
                var nomeProjeto = $"{PlatformServices.Default.Application.ApplicationName}.xml";
                var CaminhoXMLComentario = Path.Combine(caminhoProjeto, nomeProjeto);

                cfg.IncludeXmlComments(CaminhoXMLComentario);

                cfg.DocInclusionPredicate((docName, apiDesc) =>
                {
                    var actionApiVersionModel = apiDesc.ActionDescriptor?.GetApiVersion();
                    // would mean this action is unversioned and should be included everywhere
                    if (actionApiVersionModel == null)
                    {
                        return true;
                    }
                    if (actionApiVersionModel.DeclaredApiVersions.Any())
                    {
                        return actionApiVersionModel.DeclaredApiVersions.Any(v => $"v{v.ToString()}" == docName);
                    }
                    return actionApiVersionModel.ImplementedApiVersions.Any(v => $"v{v.ToString()}" == docName);
                });

                cfg.OperationFilter<ApiVersionOperationFilter>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStatusCodePages();

            app.UseMvc();

            app.UseSwagger(); //vai criar um arquivo no /swagger/v1/swagger.json
            app.UseSwaggerUI(cfg =>
            {
                cfg.SwaggerEndpoint("/swagger/v2.0/swagger.json", "MimicAPI - V2.0");
                cfg.SwaggerEndpoint("/swagger/v1.1/swagger.json", "MimicAPI - V1.1");
                cfg.SwaggerEndpoint("/swagger/v1.0/swagger.json", "MimicAPI - V1.0");

                cfg.RoutePrefix = string.Empty;
            });


        }
    }
}

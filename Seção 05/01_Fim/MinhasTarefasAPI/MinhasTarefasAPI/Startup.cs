using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MinhasTarefasAPI.Database;
using MinhasTarefasAPI.v1.Models;
using MinhasTarefasAPI.v1.Repositories;
using MinhasTarefasAPI.v1.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.PlatformAbstractions;
using System.IO;
using MinhasTarefasAPI.v1.Helpers.Swagger;
using Swashbuckle.AspNetCore.Swagger;

namespace MinhasTarefasAPI
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
            //suprime.. retira a validação do modelstate que roda antes de entrar no metodo da API... coisa que o notation ApiController faz..
            services.Configure<ApiBehaviorOptions>(op =>
            {
                op.SuppressModelStateInvalidFilter = true;
            });

            //adiciona o banco de dados
            services.AddDbContext<MinhasTarefasContext>(op =>
            {
                op.UseSqlite("Data Source=Database\\MinhasTarefas.db");
            });

            //adiciona a injeção de dependência das classes
            //Repositories
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<ITarefaRepository, TarefaRepository>();
            services.AddScoped<ITokenRepository, TokenRepository>();

            // AddJsonOptions - ignora o loop de retorno da api onde uma lista do retorno chama algum objeto da propria api...  
            services.AddMvc(config =>
            {
                config.ReturnHttpNotAcceptable = true; //se tipo de retorno não aceitavel retorna 406
                config.InputFormatters.Add(new XmlSerializerInputFormatter(config)); //API permite receber XML como request
                config.OutputFormatters.Add(new XmlSerializerOutputFormatter()); //API permite devolver XML como response
            })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(opt =>
                    opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                );

            //Swagger - 1
            services.AddApiVersioning(cfg =>
            {
                cfg.ReportApiVersions = true; //da a opção ao usuario de consumir versões compatíveis
                //cfg.ApiVersionReader = new HeaderApiVersionReader("api-version"); //nome da variavel que chega pelo request contendo a versão escolhida
                cfg.AssumeDefaultVersionWhenUnspecified = true; // se usuario não especificar versão usa-se a default(especificada abaixo)
                cfg.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0); // interessante qdo a versão vier no cabeçalho
            });

            //adicionando o Swagger
            services.AddSwaggerGen(cfg => {

                //adiciona o header no swagger
                cfg.AddSecurityDefinition("Bearer", new ApiKeyScheme()
                {
                     In = "header",
                     Type = "apiKey",
                     Description = "Adicione o JWT para autenticar.",
                     Name = "Authorization"
                });

                var security = new Dictionary<string, IEnumerable<string>>()
                {
                    {"Bearer", new string[] {} }
                };
                cfg.AddSecurityRequirement(security);

                cfg.ResolveConflictingActions(apiDescription => apiDescription.First());
                cfg.SwaggerDoc("v1.0", new Swashbuckle.AspNetCore.Swagger.Info()
                {
                    Title = "MinhasTarefasAPI - v1.0",
                    Version = "v1.0"
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


            //adiciona o Identity
            //gera o redirect de erro 500 caso de algum erro interno..
            //AddDefaultIdentity chama 3 comandos: AddIdentity, AddDefaultUI(mostra as telas de login etc) e AddDefaultProviders  
            //services.AddDefaultIdentity<ApplicationUser>()
            //    .AddEntityFrameworkStores<MinhasTarefasContext>();

            //tira a tela default de erro 500 
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<MinhasTarefasContext>()
                .AddDefaultTokenProviders(); //permite que o Identity valide por token (JWT)

            //configurar forma de autenticação via token (JWT). o "AddAuthentication" é quem vai trabalhar no Notation [Authorize]
            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters() //aqui fica a critério da aplicação o que será validado
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("chave-api-jwt-minhas-tarefas"))
                };
            });

            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                                             .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme) //schema é o JWT
                                             .RequireAuthenticatedUser() //autenticar o usuario
                                             .Build()
                );
            });

            //ao inves de tentar redirecionar para uma tela inexistente, vai retornar 401
            services.ConfigureApplicationCookie(opt =>
            {
                opt.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseAuthentication(); //para usar o JWT na autenticação
            app.UseStatusCodePages();  //para qdo rodar a api no debug aparecer na tela o status code atual
            //app.UseAuthentication(); //pra usar o notation [Authorize]
            app.UseHttpsRedirection();
            app.UseMvc();

            app.UseSwagger(); //vai criar um arquivo no /swagger/v1/swagger.json
            app.UseSwaggerUI(cfg =>
            {
                cfg.SwaggerEndpoint("/swagger/v1.0/swagger.json", "MinhasTarefasAPI - V1.0");

                cfg.RoutePrefix = string.Empty;
            });
        }
    }
}

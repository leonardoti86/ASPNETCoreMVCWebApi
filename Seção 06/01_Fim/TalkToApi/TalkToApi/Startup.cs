using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalkToApi.Database;
using TalkToApi.Helpers;
using TalkToApi.Helpers.Constants;
using TalkToApi.Helpers.Swagger;
using TalkToApi.V1.Models;
using TalkToApi.V1.Repositories;
using TalkToApi.V1.Repositories.Contracts;

namespace TalkToApi
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
            //Configura AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new DTOMapperProfile());
            });
            IMapper mapper = config.CreateMapper();
            services.AddSingleton(mapper); // uma única instancia na aplicação utilizando esse "mapeador"

            //suprime.. retira a validação do modelstate que roda antes de entrar no metodo da API... coisa que o notation ApiController faz..
            services.Configure<ApiBehaviorOptions>(op =>
            {
                op.SuppressModelStateInvalidFilter = true;
            });

            //adiciona a injeção de dependência das classes
            //Repositories
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<ITokenRepository, TokenRepository>();
            services.AddScoped<IMensagemRepository, MensagemRepository>();

            services.AddDbContext<TalkToContext>(cfg =>
            {
                cfg.UseSqlite("Data Source=Database\\TalkTo.db");
            });

            /*
            trata as politicas de CORS
            o CORS verifica o Dominio, Subdominio, Protocolo e Porta da requisição se é da mesma origem. 
            se um desses for diferente da origem, o CORS irá barrar.
            */
            services.AddCors(cfg =>
            {
                cfg.AddDefaultPolicy(policy => {
                    policy
                        .WithOrigins("https://localhost:44390") //autoriza apenas requisições externas vindas dessa URL
                        .AllowAnyMethod()
                        .SetIsOriginAllowedToAllowWildcardSubdomains() //autoriza qualquer subdominio da origem
                        .AllowAnyHeader();
                    //.WithMethods("GET") //autoriza apenas para metodos GET da URL de cima
                    //.WithHeaders("Accept", "Authorization");
                });

                //habilitar todos os sites, sem restrição
                cfg.AddPolicy("AnyOrigin", policy => {
                    policy
                        .AllowAnyOrigin()
                        .WithMethods("GET")
                        .AllowAnyHeader();
                });
            });

            services.AddMvc(cfg =>
            {
                cfg.ReturnHttpNotAcceptable = true; //se tipo de retorno não aceitavel retorna 406
                cfg.InputFormatters.Add(new XmlSerializerInputFormatter(cfg)); //API permite receber XML como request
                cfg.OutputFormatters.Add(new XmlSerializerOutputFormatter()); //API permite devolver XML como response

                var jsonOutputFormatter = cfg.OutputFormatters.OfType<JsonOutputFormatter>().FirstOrDefault();
                if (jsonOutputFormatter != null)
                {
                    //adiciona dois media types no Accept do request: vnd.talkto.hateoas e json
                    jsonOutputFormatter.SupportedMediaTypes.Add(CustomMediaType.HATEOAS); // esse mediaType permite vc criar um tipo de retorno especifico da sua empresa.
                }

            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
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
            services.AddSwaggerGen(cfg =>
            {

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
                    Title = "TalkToAPI - v1.0",
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
            //    .AddEntityFrameworkStores<TalkToContext>();

            //tira a tela default de erro 500 
            services.AddIdentity<ApplicationUser, IdentityRole>(options => {
                //aqui tiramos as validações padrão de senha. não recomendado 
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 5;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false; // caracter especial
            })
                .AddEntityFrameworkStores<TalkToContext>()
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
            //app.UseCors("AnyOrigin"); não recomendado usar assim qdo se usa atributos EnableCors/DisableCors

            app.UseMvc();

            app.UseSwagger(); //vai criar um arquivo no /swagger/v1/swagger.json
            app.UseSwaggerUI(cfg =>
            {
                cfg.SwaggerEndpoint("/swagger/v1.0/swagger.json", "TalkToAPI - V1.0");

                cfg.RoutePrefix = string.Empty;
            });
        }
    }
}

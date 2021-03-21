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
using MinhasTarefasAPI.Database;
using MinhasTarefasAPI.Models;
using MinhasTarefasAPI.Repositories;
using MinhasTarefasAPI.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

            services.AddDbContext<MinhasTarefasContext>(op =>
            {
                op.UseSqlite("Data Source=Database\\MinhasTarefas.db");
            });

            //adiciona a injeção de dependência das classes
            //Repositories
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<ITarefaRepository, TarefaRepository>();


            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(opt =>
                    opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                ); // ignora o loop de retorno da api onde uma lista do retorno chama algum objeto da propria api...  

            //adiciona o Identity
            //gera o redirect de erro 500..
            //AddDefaultIdentity chama 3 comandos: AddIdentity, AddDefaultUI(mostra as telas de login etc) e AddDefaultProviders  
            //services.AddDefaultIdentity<ApplicationUser>()
            //    .AddEntityFrameworkStores<MinhasTarefasContext>();

            //tira a tela default de erro 500 
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<MinhasTarefasContext>();

            //ao inves de tentar redirecionar para uma tela inexistente, vai retornar 401
            services.ConfigureApplicationCookie(opt => {
                opt.Events.OnRedirectToLogin = context => {
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

            app.UseStatusCodePages();  //para qdo rodar a api no debug aparecer na tela o status code atual
            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}

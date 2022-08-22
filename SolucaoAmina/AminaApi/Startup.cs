using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AminaApi.Src.Contexto;
using AminaApi.Src.Repositorios;
using AminaApi.Src.Repositorios.Implementacoes;
using Microsoft.EntityFrameworkCore;
using AminaApi.Src.Servicos.Implementacoes;
using AminaApi.Src.Servicos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AminaApi
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
            // Configura��p Banco de Dados
            services.AddDbContext<AminaContextos>(opt => opt.UseSqlServer(Configuration["ConnectionStringsDev:DefaultConnection"]));

            // Repositorios
            services.AddScoped<IPostagem, PostagemRepositorio>();
            services.AddScoped<IGrupo, GrupoRepositorio>();
            services.AddScoped<IUsuario, UsuarioRepositorio>();

            //Configura��o dos controladores
            services.AddCors();
            services.AddControllers();

            // Configura��o de Servi�os
            services.AddScoped<IAutenticacao, AutenticacaoServicos>();

            // Configura��o do Token Autentica��o JWTBearer
            var chave = Encoding.ASCII.GetBytes(Configuration["Settings:Secret"]);
            services.AddAuthentication(a =>
            {
                a.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                a.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(b =>
            {
                b.RequireHttpsMetadata = false;
                b.SaveToken = true;
                b.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(chave),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            }
            );
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AminaContextos contexto)
        {
            if (env.IsDevelopment())
            {
                contexto.Database.EnsureCreated();
                app.UseDeveloperExceptionPage();
            }
            //Ambiente de produ��o

            //Rotas
            contexto.Database.EnsureCreated();

            app.UseRouting();

            app.UseCors(c => c 
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
            );
            //Autentica��o autorizada
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

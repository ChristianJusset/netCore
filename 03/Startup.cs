using _03.Contexts;
using _03.Entities;
using _03.Entities.Seguridad;
using _03.Helpers;
using _03.Models;
using _03.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Text;

namespace _03
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

            //opcion para habilitar en todos los metodos el cors.
            services.AddCors();

            //opcion para establecer en cada metodo, en middleware debe de ir solo app.UseCors();
            //services.AddCors(options =>
            //{
            //    options.AddPolicy(name: MyAllowSpecificOrigins,
            //        builder => builder.WithOrigins("http://www.apirequest.io").WithMethods("GET", "POST").AllowAnyHeader());
            //});

            //habilitamos la configuracion para el uso de encriptacion
            services.AddDataProtection();

            //Para utilizar el hash se utiliza el Cryptography, habilitamos desde el servicio que hemos creado
            services.AddScoped<HashService>();

            //Filtro a nivel de metodo.
            services.AddScoped<MiFiltroDeAccion>();

            //habilitamos configuracion para usar el cache.
            services.AddResponseCaching();

            

            //configuracion para usar el EF
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            
            //configuracion de los controller a nivel general.
            services.AddControllers(options => {
                options.Filters.Add(new MiFiltroDeExcepcion());
                // Si hubiese Inyección de dependencias en el filtro
                //options.Filters.Add(typeof(MiFiltroDeExcepcion)); 
            }).AddNewtonsoftJson(options => 
               options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

          
            //automapper
            services.AddAutoMapper(configuration => 
            {
                configuration.CreateMap<Autor, AutorDTO>();
                configuration.CreateMap<Libro, LibroDTO>();
                configuration.CreateMap<AutorCreacionDTO, Autor>().ReverseMap();
                configuration.CreateMap<AutorPathDTO, Autor>().ReverseMap();
            }, typeof(Startup));


            //configuracion para el sistema de usuario de seguridad
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            //activar configuracion de la autenticacion con Bearer por default
            /*
                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();
             */


            //activar configuracion de la autenticacion con Bearer con key unico para mayor seguridad.
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
            {
                     ValidateIssuer = false,
                     ValidateAudience = false,
                     ValidateLifetime = true,
                     ValidateIssuerSigningKey = true,
                     IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(Configuration["jwt:key"])),
                     ClockSkew = TimeSpan.Zero
            });

            //visualizacion del Swagger.
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "_03", Version = "v1" });
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "_03 v1"));
            }

            //redirigir peticiones HTTP to HTTPS 
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseResponseCaching();
            
            app.UseAuthentication();
            app.UseAuthorization();

            //validar
            //app.UseCors(MyAllowSpecificOrigins);
            //app.UseCors(builder => builder.WithOrigins("http://www.apirequest.io").WithMethods("GET", "POST").AllowAnyHeader());
            //app.UseCors(builder => builder.WithOrigins("http://www.apirequest.io").AllowAnyMethod());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

using LibraryAPI.DAL;
using LibraryAPI.Domain;
using LibraryAPI.LogicProcessors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            GlobalSettings.ConnectionString = Configuration["ConnectionString"];
            GlobalSettings.DbProviderFactory = MySqlConnectorFactory.Instance;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddDbContext<IdentityDataContext>(options => options.UseMySQL(Configuration["ConnectionString"]));

            //configure DataContexts
            ILibraryDataContext libraryDataContext = new LibraryDataContext();

            //configure LogicProcessors
            PermissionLogicProcessor permissionLogicProcessor = new PermissionLogicProcessor(libraryDataContext);
            LibraryLogicProcessor libraryLogicProcessor = new LibraryLogicProcessor(libraryDataContext, permissionLogicProcessor);
            BookLogicProcessor bookLogicProcessor = new BookLogicProcessor(libraryDataContext, permissionLogicProcessor);
            CollectionLogicProcessor collectionLogicProcessor = new CollectionLogicProcessor(libraryDataContext, permissionLogicProcessor, bookLogicProcessor);
            AuthorLogicProcessor authorLogicProcessor = new AuthorLogicProcessor(libraryDataContext);

            //register
            services.AddSingleton(typeof(ILibraryDataContext), libraryDataContext);
            services.AddSingleton(typeof(PermissionLogicProcessor), permissionLogicProcessor);
            services.AddSingleton(typeof(LibraryLogicProcessor), libraryLogicProcessor);
            services.AddSingleton(typeof(BookLogicProcessor), bookLogicProcessor);
            services.AddSingleton(typeof(CollectionLogicProcessor), collectionLogicProcessor);
            services.AddSingleton(typeof(AuthorLogicProcessor), authorLogicProcessor);

            services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<IdentityDataContext>().AddDefaultTokenProviders();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false; //TODO: enable me pls
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = Configuration["BaseURL"], //TODO: enable in prod
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT-SECRET-KEY"])),
                    ValidateAudience = false
                };
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "LibraryAPI", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Auth",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Bearer <token>"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme()
                        {
                            Reference = new OpenApiReference()
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "LibraryAPI v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

using LibraryAPI.DAL;
using LibraryAPI.Domain;
using LibraryAPI.LogicProcessors;
using LibraryAPI.Services;
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
using Npgsql;
using System;
using System.Text;

namespace LibraryAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Configuration["ConnectionString"] = Environment.GetEnvironmentVariable("ConnectionString") ?? Configuration["ConnectionString"];
            Configuration["JwtSecretKey"] = Environment.GetEnvironmentVariable("JwtSecretKey") ?? Configuration["JwtSecretKey"];
            Configuration["EmailUsername"] = Environment.GetEnvironmentVariable("EmailUsername") ?? Configuration["EmailUsername"];
            Configuration["EmailPassword"] = Environment.GetEnvironmentVariable("EmailPassword") ?? Configuration["EmailPassword"];

            GlobalSettings.ConnectionString = Configuration["ConnectionString"];
            GlobalSettings.DbProviderFactory = NpgsqlFactory.Instance;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            services.AddHttpClient();
            services.AddControllers();
            services.AddDbContext<IdentityDataContext>(options => options.UseNpgsql(Configuration["ConnectionString"], o => o.UseNodaTime()).UseLowerCaseNamingConvention());

            //configure DataContexts
            ILibraryDataContext libraryDataContext = new LibraryDataContext();

            //configure LogicProcessors
            PermissionLogicProcessor permissionLogicProcessor = new PermissionLogicProcessor(libraryDataContext);
            LibraryLogicProcessor libraryLogicProcessor = new LibraryLogicProcessor(libraryDataContext, permissionLogicProcessor);
            BookLogicProcessor bookLogicProcessor = new BookLogicProcessor(libraryDataContext, permissionLogicProcessor);
            CollectionLogicProcessor collectionLogicProcessor = new CollectionLogicProcessor(libraryDataContext, permissionLogicProcessor, bookLogicProcessor);
            AuthorLogicProcessor authorLogicProcessor = new AuthorLogicProcessor(libraryDataContext);

            //configure services
            IEmailService emailService = new EmailService(Configuration["EmailUsername"], Configuration["EmailPassword"]);

            //register
            services.AddSingleton(typeof(ILibraryDataContext), libraryDataContext);
            services.AddSingleton(typeof(IEmailService), emailService);
            services.AddSingleton(typeof(PermissionLogicProcessor), permissionLogicProcessor);
            services.AddSingleton(typeof(LibraryLogicProcessor), libraryLogicProcessor);
            services.AddSingleton(typeof(BookLogicProcessor), bookLogicProcessor);
            services.AddSingleton(typeof(CollectionLogicProcessor), collectionLogicProcessor);
            services.AddSingleton(typeof(AuthorLogicProcessor), authorLogicProcessor);

            services.AddIdentity<ApplicationUser, IdentityRole>(options => {
                options.Tokens.PasswordResetTokenProvider = nameof(SimpleTokenProvider);
            })
                .AddEntityFrameworkStores<IdentityDataContext>()
                .AddDefaultTokenProviders()
                .AddTokenProvider(nameof(SimpleTokenProvider), typeof(SimpleTokenProvider));
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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtSecretKey"])),
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
            app.UseStaticFiles();

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

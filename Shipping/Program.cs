using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using Shipping.Models;
using Shipping.UnitOfWork;
using Shipping.Repository.Employee_Repository;
using Shipping.AutoMapperProfiles;
using Microsoft.Extensions.DependencyInjection;
internal class Program
{
    private static void Main(string[] args)
    {
        string txt = "";

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.


        #region for Swagger Doc To Allow sending Token 
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Shipping System API",
                Version = "v1"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Description = "Here Enter JWT Token with bearer format like bearer [space] token"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
        });
        #endregion




        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        });

        builder.Services.AddEndpointsApiExplorer();



     

        builder.Services.AddDbContext<ShippingContext>(options =>
        {
            options.UseLazyLoadingProxies().UseSqlServer(builder.Configuration.GetConnectionString("Db"));
            //options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });



        builder.Services.AddIdentity<AppUser, UserRole>(
                    options =>
                    {
                        options.User.RequireUniqueEmail = true;
                        options.Password.RequireNonAlphanumeric = false;
                        options.Password.RequireDigit = false;
                        options.Password.RequireUppercase = false;
                        options.Password.RequireLowercase = false;
                        options.Password.RequiredLength = 8;
                    }).AddEntityFrameworkStores<ShippingContext>();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(txt,
            builder =>
            {
                builder.AllowAnyOrigin();
                builder.AllowAnyMethod();
                builder.AllowAnyHeader();
            });
        });


        #region register UnitOfWork & Configuration & myServices 
        builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
        builder.Services.AddScoped<IUnitOfWork<Employee>, UnitOfWork<Employee>>();


        //builder.Services.AddScoped<IUnitOfWork<Product>, UnitOfWork<Product>>();

        #endregion

        builder.Services.AddAuthentication(option => option.DefaultAuthenticateScheme = "schema")
                    .AddJwtBearer("schema",
                    op =>
                    {
                        string key = "Iti Pd And Bi 44 Menoufia Shipping System For GP";
                        var secertkey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));

                        op.TokenValidationParameters = new TokenValidationParameters()
                        {
                            IssuerSigningKey = secertkey,
                            ValidateIssuer = false,
                            ValidateAudience = false
                        };
                    }
                    );

        //use autoMapper
        var serviceProvider = builder.Services.BuildServiceProvider();
        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
        builder.Services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile(new MappingEmployee());
        }, typeof(Program));


        var app = builder.Build();





        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors(txt);

        app.UseAuthorization();

       
        app.MapControllers();

        app.Run();
    }
}
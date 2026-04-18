using System.Text;
using ECommerce.API.CustomMiddlewares;
using ECommerce.API.Extensions;
using ECommerce.API.Factories;
using ECommerce.Domain.Contracts;
using ECommerce.Domain.Entities.IdentityModule;
using ECommerce.Persistence.Data.DataSeed;
using ECommerce.Persistence.Data.DbContexts;
using ECommerce.Persistence.IdentityData.DataSeed;
using ECommerce.Persistence.IdentityData.DbContexts;
using ECommerce.Persistence.Repositories;
using ECommerce.Services;
using ECommerce.Services.Abstraction;
using ECommerce.Services.MappingProfiles;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace ECommerce.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            #region program code
            var builder = WebApplication.CreateBuilder(args);

            #region Register DI Container
            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Configuration.AddEnvironmentVariables();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ECommerce.API", Version = "v1" });

                c.AddSecurityDefinition(
                    "Bearer",
                    new OpenApiSecurityScheme
                    {
                        Description =
                            "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                    }
                );

                c.AddSecurityRequirement(
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer",
                                },
                            },
                            new string[] { }
                        },
                    }
                );
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(
                    "DevelopmentPolicy",
                    builder =>
                    {
                        builder.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod();
                    }
                );
            });

            builder.Services.AddDbContext<StoreDbContext>(options =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")
                );
            });

            builder.Services.AddKeyedScoped<IDataIntializer, DataIntializer>("Default");
            builder.Services.AddKeyedScoped<IDataIntializer, IdentityDataIntializer>("Identity");

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddAutoMapper(typeof(ServiceAssemblyReference).Assembly);

            builder.Services.AddScoped<IProductService, ProductService>();

            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                return ConnectionMultiplexer.Connect(
                    builder.Configuration.GetConnectionString("RedisConnection")!
                );
            });

            builder.Services.AddScoped<IBasketRepository, BasketRepository>();
            builder.Services.AddScoped<IBasketService, BasketService>();
            builder.Services.AddScoped<ICacheRepository, CacheRepository>();
            builder.Services.AddScoped<ICacheService, CacheService>();

            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory =
                    ApiResponseFactory.GenerateApiValidationResponse;
            });

            builder.Services.AddDbContext<StoreIdentityDbContext>(Options =>
            {
                Options.UseSqlServer(
                    builder.Configuration.GetConnectionString("IdentityConnection")
                );
            });

            //builder
            //    .Services.AddIdentity<ApplicationUser, IdentityRole>()
            //    .AddEntityFrameworkStores<StoreIdentityDbContext>();

            builder
                .Services.AddIdentityCore<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<StoreIdentityDbContext>();

            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

            builder
                .Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidIssuer = builder.Configuration["JWTOptions:Issuer"],
                        ValidAudience = builder.Configuration["JWTOptions:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["JWTOptions:SecretKey"]!)
                        ),
                    };
                });

            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            #endregion



            var app = builder.Build();

            await app.MigrateDataBaseAsync();
            await app.MigratIdentityeDataBaseAsync();

            await app.SeedDataAsync();
            await app.SeedIdentityDataAsync();

            #region Configure PipeLine [Middlewares]
            #region Custom Middleware
            // Configure the HTTP request pipeline.

            //app.Use(
            //    async (context, next) =>
            //    {
            //        try
            //        {
            //            await next();
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine(ex.Message); //Logging console

            //            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            //            await context.Response.WriteAsJsonAsync(
            //                new
            //                {
            //                    StatusCode = StatusCodes.Status500InternalServerError,
            //                    Error = $"An unexpected error Occured:{ex.Message}",
            //                }
            //            );
            //        }
            //    }
            //);
            #endregion


            app.UseMiddleware<ExceptionHandlerMiddleware>();
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.DisplayRequestDuration();
                    options.EnableFilter();
                    options.DocExpansion(DocExpansion.None);
                });
            }

            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseCors("DevelopmentPolicy");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            #endregion

            await app.RunAsync(); 
            #endregion
        }
    }
}

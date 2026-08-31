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
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
            
            builder.Services.AddEndpointsApiExplorer();
            //builder.Configuration.AddEnvironmentVariables();
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

            //Origins come from configuration (Cors:AllowedOrigins) so production never
            //falls back to AllowAnyOrigin, which would let any site call the API with credentials.
            var allowedOrigins =
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(
                    "DefaultPolicy",
                    policy =>
                    {
                        policy
                            .WithOrigins(allowedOrigins)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    }
                );
            });

            builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter(
                    "auth",
                    o =>
                    {
                        o.Window = TimeSpan.FromMinutes(1);
                        o.PermitLimit = 10;
                    }
                );
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });

            builder.Services.AddDbContext<StoreDbContext>(options =>
            {
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("DefaultConnection")
                );
            });

            builder.Services.AddKeyedScoped<IDataIntializer, DataIntializer>("Default");
            builder.Services.AddKeyedScoped<IDataIntializer, IdentityDataIntializer>("Identity");

            builder.Services.AddHttpContextAccessor();

            //Behind a reverse proxy / load balancer the app otherwise sees http and the
            //original client host is lost, which breaks HTTPS redirection and generated URLs.
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            //AutoMapper 15+ takes a configuration action instead of a bare assembly list.
            //The assembly argument still matters: it is what registers IValueResolver
            //implementations in DI so they can take constructor dependencies.
            builder.Services.AddAutoMapper(
                cfg => cfg.AddMaps(typeof(ServiceAssemblyReference).Assembly),
                typeof(ServiceAssemblyReference).Assembly
            );

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
                Options.UseNpgsql(
                    builder.Configuration.GetConnectionString("IdentityConnection")
                );
            });

            //builder
            //    .Services.AddIdentity<ApplicationUser, IdentityRole>()
            //    .AddEntityFrameworkStores<StoreIdentityDbContext>();

            builder
                .Services.AddIdentityCore<ApplicationUser>(options =>
                {
                    //Every lookup in the app is by email, so duplicates must be impossible.
                    options.User.RequireUniqueEmail = true;

                    options.Password.RequiredLength = 8;

                    //Without lockout, the login endpoint can be brute forced indefinitely.
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                    options.Lockout.AllowedForNewUsers = true;
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<StoreIdentityDbContext>()
                .AddSignInManager();

            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

            builder
                .Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    //Fail fast at startup rather than with an obscure NullReference on the
                    //first request if the signing key was never supplied.
                    var secretKey =
                        builder.Configuration["JWTOptions:SecretKey"]
                        ?? throw new InvalidOperationException(
                            "JWTOptions:SecretKey is not configured. "
                                + "Set it via environment variable JWTOptions__SecretKey or user secrets."
                        );

                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["JWTOptions:Issuer"],
                        ValidAudience = builder.Configuration["JWTOptions:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(secretKey)
                        ),
                    };
                });

            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            #endregion



            var app = builder.Build();

            

            

            //Schema migrations must run in every environment, otherwise a production
            //deployment starts against an unmigrated database.
            await app.MigrateDataBaseAsync();
            await app.MigratIdentityeDataBaseAsync();

            //The catalog is reference data, not a secret: seed it everywhere, otherwise a
            //deployed instance serves an empty shop.
            await app.SeedDataAsync();

            //Demo accounts use a well-known password, so they stay in Development only.
            if (app.Environment.IsDevelopment())
            {
                await app.SeedIdentityDataAsync();
            }


            
            

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


            app.UseForwardedHeaders();

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
            //#region Test
            //Console.WriteLine("JWT: " + builder.Configuration["JWTOptions:SecretKey"]);
            //Console.WriteLine("Stripe Secret: " + builder.Configuration["Stripe:SecretKey"]);
            //Console.WriteLine("Stripe Endpoint: " + builder.Configuration["Stripe:EndpointSecret"]);
            //Console.WriteLine("JWT: " + builder.Configuration["JWTOptions:SecretKey"]);
            //Console.WriteLine("ENV: " + builder.Environment.EnvironmentName);
            //#endregion

            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseCors("DefaultPolicy");

            app.UseRateLimiter();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            //Liveness probe for the hosting platform.
            app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
                .AllowAnonymous();

            //When the Angular bundle is published into wwwroot this app also serves the SPA.
            //Without the fallback, refreshing a client-side route such as /shop/5 returns 404.
            //Requests under /api keep their normal 404 so clients still see API errors.
            app.MapFallback(
                (HttpContext context) =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                        return Results.NotFound();

                    var indexPath = Path.Combine(app.Environment.WebRootPath ?? "", "index.html");

                    return File.Exists(indexPath)
                        ? Results.File(indexPath, "text/html")
                        : Results.NotFound();
                }
            );
            #endregion

            await app.RunAsync(); 
            #endregion
        }
    }
}

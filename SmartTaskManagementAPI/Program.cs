using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartTaskManagementAPI.AppUser.models;
using SmartTaskManagementAPI.AppUser.service;
using SmartTaskManagementAPI.AppUser.service.impl;
using SmartTaskManagementAPI.Authentication.service;
using SmartTaskManagementAPI.Authentication.service.impl;
using SmartTaskManagementAPI.Data;
using SmartTaskManagementAPI.Exceptions;
using SmartTaskManagementAPI.TaskCategory.repository;
using SmartTaskManagementAPI.TaskCategory.repository.impl;
using SmartTaskManagementAPI.TaskCategory.service;
using SmartTaskManagementAPI.TaskManagement.repository;
using SmartTaskManagementAPI.TaskManagement.repository.impl;
using SmartTaskManagementAPI.TaskManagement.service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using SmartTaskManagementAPI.Authentication.AuthorizationHandler;
using SmartTaskManagementAPI.System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

//inject controller
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//show authorization in swagger
//customize the swaggerGen
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo{ Title = "SmartTaskManagementAPI", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement()
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
            new string[]
            {
                //empty
            }
        }
    });

});


//Register Dbcontext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

});

//Register TaskManagement Repository in the DI container
builder.Services.AddScoped<ITaskManagementRepository, TaskManagementRepository>();

//Register TaskCategory Repository in the DI container
builder.Services.AddScoped<ITaskCategoryRepository, TaskCategoryRepository>();

//Register the TokenService
builder.Services.AddScoped<ITokenService, TokenService>();

//Register the user service
builder.Services.AddScoped<IUserService, UserService>();

//Add default Identity here
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        //if password requires some special condtion
        /*options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 6;*/
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

//Add/Registration Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserOrAdmin", policy => 
        policy.Requirements.Add(new UserRequestRequirement()));
});

builder.Services.AddSingleton<IAuthorizationHandler, UserRequestAuthorizationHandler>();
builder.Services.AddHttpContextAccessor();

//Register Authentication logic
builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme =
                options.DefaultChallengeScheme =
                    options.DefaultForbidScheme =
                        options.DefaultScheme =
                            options.DefaultSignInScheme =
                                options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
        })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:signInKey"]))
        };
        /***
         * OnChallenge: This event triggers when authentication fails, such as when the user provides no token or an invalid token.
            HandleResponse: This prevents the middleware's default behavior (sending the 401 Unauthorized with the www-authenticate header).
            Custom JSON Response: The response includes your Result structure with a custom message.
         */
        options.Events = new JwtBearerEvents
        {
            //overrides the custom error provided by the JwtBearer 
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                var result = new Result()
                {
                    flag = false,
                    code = StatusCodes.Status401Unauthorized,
                    message = "You are not authorized to access this resource."
                };
                await context.Response.WriteAsync(JsonConvert.SerializeObject(result));
            }

        };

    });

//Register the TaskManagement Service in the DI container
/*
 * If you don't have an interface for the service,
 * but instead you are injecting the concrete class TaskManagementService,
 * you can register it directly:
 */
builder.Services.AddScoped<TaskManagementService>();

//Register the TaskCategory Service in the DI container
builder.Services.AddScoped<TaskCategoryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//register middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();  // Register the custom middleware


//for authorization and authentication
app.UseAuthentication();
app.UseAuthorization();

//add this for swagger to work
app.MapControllers();

app.Run();
//call the api migration method
ApplyDataBaseSeed();

void ApplyDataBaseSeed()
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (db.Database.GetPendingMigrations().Any())
        {
            db.Database.Migrate();
        }
    }
}

public partial class Program {}



/***
 * To register the ExceptionHandlingMiddleware.cs in your Program.cs file,
 * you'll need to use the app.UseMiddleware<ExceptionHandlingMiddleware>();
 * method before the rest of the middleware pipeline executes,
 * but after app.UseHttpsRedirection() and any authentication-related middleware.

You don't need to register the middleware in the builder.Services.AddScoped<ExceptionHandlingMiddleware>() section 
because middleware is instantiated per request in the pipeline and does not 
require DI registration unless it requires dependencies.

 */

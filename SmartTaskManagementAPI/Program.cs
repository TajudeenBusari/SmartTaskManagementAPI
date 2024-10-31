using Microsoft.EntityFrameworkCore;
using SmartTaskManagementAPI.Data;
using SmartTaskManagementAPI.Exceptions;
using SmartTaskManagementAPI.TaskCategory.model;
using SmartTaskManagementAPI.TaskCategory.repository;
using SmartTaskManagementAPI.TaskCategory.repository.impl;
using SmartTaskManagementAPI.TaskCategory.service;
using SmartTaskManagementAPI.TaskManagement.repository;
using SmartTaskManagementAPI.TaskManagement.repository.impl;
using SmartTaskManagementAPI.TaskManagement.service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

//inject controller
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



//Register Dbcontext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

});

//Register TaskManagement Repository in the DI container
builder.Services.AddScoped<ITaskManagementRepository, TaskManagementRepository>();

//Register TaskCategory Repository in the DI container
builder.Services.AddScoped<ITaskCategoryRepository, TaskCategoryRepository>();

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

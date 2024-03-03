using Microsoft.AspNetCore.Http.Features;
using TodoApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json;


var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("ToDoDB");
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.Parse("8.0.36-mysql")), ServiceLifetime.Singleton);

builder.Services.AddCors(options =>
{ 
    options.AddPolicy("AllowSpecificOrigin",builder =>
    {
        builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
    });
});
// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "ToDo API"
    });
});
var app = builder.Build();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.UseCors("AllowSpecificOrigin");
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/items", (ToDoDbContext dbContext) =>dbContext.Items.ToList());
app.MapPost("/items", (ToDoDbContext dbContext, Item item) => {
    dbContext.Items.Add(item);
    dbContext.SaveChanges();
    return item;
}
);
app.MapPut("/items/{id}", (ToDoDbContext dbContext, int id, bool setComplete) => { var existingItem = dbContext.Items.Find(id);
    if (existingItem != null)
    {
        existingItem.IsComplete = setComplete;
        dbContext.Items.Update(existingItem);
        dbContext.SaveChanges();
    }
    return existingItem;});

app.MapDelete("/items/{id}", (ToDoDbContext dbContext, int id) => {var item = dbContext.Items.Find(id);
    if (item != null)
    {
        dbContext.Items.Remove(item);
        dbContext.SaveChanges();
    }});

app.Run();

builder.Services.AddSingleton<Item>();
builder.Services.AddSingleton<ToDoDbContext>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

using Core.Entities;
using Core.Interfaces;
using Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.Configure<SoapSettings>(builder.Configuration.GetSection("SoapSettings"));
builder.Services.AddHttpClient<IRechargeService, RechargeService>();

builder.Services.AddControllers();

// Add Swagger for API documentation if needed
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.


    app.UseSwagger();
    app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

using MapService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register services
builder.Services.AddControllers();
builder.Services.AddSingleton<Map<int>>(provider => new Map<int>(int.Parse));

builder.Services.AddSwaggerGen();



var app = builder.Build();


app.MapControllers();

// Configure the HTTP request pipeline.

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();
app.Run();


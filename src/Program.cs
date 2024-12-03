var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<StateManager>();
builder.Services.AddSingleton<IStatsRepository, StatsRepository>();
builder.Services.AddControllers();
builder.Services.AddCors(options => 
{
    options.AddPolicy("ReactPolicy", policy => 
    {
        policy.WithOrigins(
            "http://reddit-lb-104881215.us-east-1.elb.amazonaws.com",
            "http://localhost:65010"
        )
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();
app.UseCors("ReactPolicy");
app.UseHttpsRedirection();
app.MapControllers();

string WEB_HOST_URL = Environment.GetEnvironmentVariable("WEB_HOST_URL") ?? "";
builder.WebHost.UseUrls(WEB_HOST_URL);


app.Run();
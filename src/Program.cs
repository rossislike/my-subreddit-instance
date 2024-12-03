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
            "http://rumo-reddit-site.s3-website-us-east-1.amazonaws.com"
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
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<StateManager>();
builder.Services.AddSingleton<IStatsRepository, StatsRepository>();
// builder.Services.AddCors(options => 
// {
//     options.AddPolicy("ReactPolicy", policy => 
//     {
//         policy.AllowAnyOrigin()
//             .AllowAnyMethod()
//             .SetIsOriginAllowed((host) => true)
//             .AllowAnyHeader();
//     });
// });
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();
// app.UseCors("ReactPolicy");
app.UseHttpsRedirection();
app.MapControllers();

string ASPNETCORE_URLS = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "";
builder.WebHost.UseUrls(ASPNETCORE_URLS);


app.Run();
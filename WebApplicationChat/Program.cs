using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApplicationChat.Data;
using WebApplicationChat.Hubs;
using WebApplicationChat.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<WebApplicationContext>();

builder.Services.AddSignalR();
// Add services to the container.

builder.Services.AddControllers();
// services
//builder.Services.AddTransient<MessageService>();
builder.Services.AddTransient<ContactService>();
//builder.Services.AddTransient<UserService>();
builder.Services.AddTransient<FirebaseService>();
builder.Services.AddTransient<HubService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile("private_key.json")
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("Allow All",
        builder =>
        {
            builder.SetIsOriginAllowed(origin => true).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseCors("Allow All");
app.UseAuthorization();

app.MapControllers();
app.MapHub<WebApplicationHub>("/Chat");

app.Run();
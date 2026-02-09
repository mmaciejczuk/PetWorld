using PetWorld.Infrastructure.Ai;
using PetWorld.Application.Ai;
using PetWorld.Infrastructure.Chat;
using PetWorld.Application.Chat;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using PetWorld.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using PetWorld.Web.Components;

var builder = WebApplication.CreateBuilder(args);



// DB (MySQL)
var cs = builder.Configuration.GetConnectionString("PetWorldDb")
         ?? throw new InvalidOperationException("Missing ConnectionStrings:PetWorldDb");
builder.Services.AddDbContext<PetWorldDbContext>(options =>
    options.UseMySql(cs, new MySqlServerVersion(new Version(8, 0, 0))));
builder.Services.AddScoped<IChatMessageStore, EfChatMessageStore>();
builder.Services.AddScoped<IAiWriterCriticService, AgentFrameworkWriterCriticService>();
// Spójny port (docker + local)
builder.WebHost.UseUrls("http://0.0.0.0:5000");
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();



// Auto-migrate on startup (so docker compose up works with zero manual steps)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PetWorldDbContext>();
    db.Database.Migrate();
}// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (!app.Environment.IsDevelopment()) { app.UseHttpsRedirection(); }

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();












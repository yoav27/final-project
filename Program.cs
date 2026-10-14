using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// קריאת מחרוזת החיבור מתוך appsettings.json
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

// פתיחת חיבור בסיסית למסד הנתונים
using var conn = new NpgsqlConnection(connectionString);
conn.Open();

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

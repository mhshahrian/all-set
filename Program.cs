using AllSet.Domain;
using AllSet.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContextPool<AllSetDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity services
builder.Services.AddIdentity<AllSetUser, IdentityRole>()
    .AddEntityFrameworkStores<AllSetDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(opts =>
{
    opts.Password.RequiredLength = 1; //Int16.Parse(Configuration["Security:PasswordLength"]);
    opts.Password.RequireNonAlphanumeric = false; //Boolean.Parse(Configuration["Security:RequireNonAlphanumeric"]);
    opts.Password.RequireLowercase = false; //Boolean.Parse(Configuration["Security:RequireLowercase"]);
    opts.Password.RequireUppercase = false; //Boolean.Parse(Configuration["Security:RequireUppercase"]);
    opts.Password.RequireDigit = false; //Boolean.Parse(Configuration["Security:RequireDigit"]);
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "myPolicy", policy =>
    {
        policy.AllowAnyOrigin()
        //.WithOrigins(builder.Configuration.GetSection("AllowedCorsUrls").GetChildren().Select(c => c.Value.ToString()).ToArray())
        .AllowAnyMethod()
        .AllowAnyOrigin()
        .AllowAnyHeader();
    });
});

builder.Services.AddScoped<AvailabilityService, AvailabilityService>();
builder.Services.AddScoped<BookingService, BookingService>();
builder.Services.AddScoped<OrderService, OrderService>();

var app = builder.Build();

// Automatically Create database and tables and do the migrations
using (var serviceScope = app.Services.GetService<IServiceScopeFactory>().CreateScope())
{
    var context = serviceScope.ServiceProvider.GetRequiredService<AllSetDbContext>();
    context.Database.Migrate();
    context.SaveChanges();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("myPolicy");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


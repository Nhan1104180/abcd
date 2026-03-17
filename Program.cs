using QuanLyDonHang.Models;

var builder = WebApplication.CreateBuilder(args);

//DI Services swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//DI Service EF-context
builder.Services.AddDbContext<OrderManagementDBContext>();

//DI service controller 
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Sử dụng middleware map controller
app.MapControllers();

app.UseHttpsRedirection();


app.Run();
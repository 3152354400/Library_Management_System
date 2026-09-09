using Backend.Data;
using backend.Common.MiddleWare;
using backend.Repositories.BorrowRecordRepository;
using backend.Repositories.LibrarianRepository;
using backend.Repositories.ReaderRepository;
using backend.Repositories.Book;
using backend.Services.BorrowingService;
using backend.Services.LibrarianService;
using backend.Services.ReaderService;
using backend.Services.RecommendationService;
using backend.Repositories.RecommendationRepository;
using backend.Services.Web;
using backend.Services.Book;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using backend.Repositories.Admin;
using backend.Services.Admin;
using backend.Repositories.Space;
using backend.Services.Space;
using backend.Services.ReaderFeature;   // ScheduledTaskService
using Microsoft.AspNetCore.Mvc.ApplicationModels;

var builder = WebApplication.CreateBuilder(args);

// 输出当前环境
Console.WriteLine($"当前运行环境: {builder.Environment.EnvironmentName}");

// 添加控制器服务
builder.Services.AddControllers(options =>
    {
        options.Conventions.Add(new ThreePersonControllerConvention());
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// 添加 Swagger 支持
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 日志配置
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// 添加服务
var services = builder.Services;

// 添加 CORS 支持（便于前端访问）
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()      // 生产环境可替换为具体域名
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// 添加 Swagger 服务
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "图书馆管理系统 API",
        Version = "v1",
        Description = "基于 ASP.NET Core 的图书馆后台接口文档"
    });
});

// 注册 IHttpContextAccessor
services.AddHttpContextAccessor();

// 读取连接字符串（根据环境自动读取 appsettings.Development.json 或 appsettings.Production.json）
var connectionString = builder.Configuration.GetConnectionString("OracleDB")
                      ?? throw new InvalidOperationException("缺少 OracleDB 连接字符串配置");


// 注册 Redis
services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetConnectionString("Redis");
    if (string.IsNullOrEmpty(configuration))
    {
        throw new ArgumentException("未配置 Redis 连接字符串");
    }
    return ConnectionMultiplexer.Connect(configuration);
});
services.AddSingleton<RedisService>();
services.AddSingleton<WebSocketNotificationService>();

// 注册后台定时任务服务
services.AddHostedService<ScheduledTaskService>();

// 注册业务服务
services.AddScoped<TokenService>();
services.AddScoped<SecurityService>();
services.AddScoped<LoginService>();

// 注册 ReaderRepository 和 ReaderService
services.AddSingleton(new ReaderRepository(connectionString));
services.AddTransient<ReaderService>();
builder.Services.AddSingleton(new RecommendationRepository(connectionString));
builder.Services.AddTransient<RecommendationService>();
builder.Services.AddScoped<IReaderExtensionService>(_ => new ReaderExtensionService(connectionString));

//注册 BorrowingService 和 BorrowingRepository
services.AddSingleton(new BorrowRecordRepository(connectionString));
services.AddTransient<BorrowingService>();

//书单部分
builder.Services.AddScoped<Backend.Repositories.Book.IBooklistRepository, Backend.Repositories.Book.BooklistRepository>();
builder.Services.AddScoped<Backend.Services.Book.IBooklistService, Backend.Services.Book.BooklistService>();
builder.Services.AddSingleton<IOracleConnectionFactory, OracleConnectionFactory>();

services.AddSingleton(new LibrarianRepository(connectionString));
services.AddTransient<LibrarianService>();

// 注册服务依赖（Repository 使用 Singleton，Service 使用 Transient）
builder.Services.AddSingleton(new BookRepository(connectionString));
builder.Services.AddSingleton(new CommentRepository(connectionString));
// builder.Services.AddSingleton(new ReportRepository(connectionString));
builder.Services.AddSingleton(new BookCategoryTreeOperation(connectionString));
builder.Services.AddSingleton(new BookCategoryRepository(connectionString));
builder.Services.AddSingleton(new LogService(connectionString));
builder.Services.AddSingleton(new BookShelfRepository(connectionString));
builder.Services.AddTransient<BookService>();
builder.Services.AddTransient<CommentService>();
// builder.Services.AddTransient<ReportService>();
builder.Services.AddTransient<BookCategoryService>();
builder.Services.AddTransient<BookShelfService>();

builder.Services.AddSingleton(new ReportRepository(connectionString));
builder.Services.AddTransient<ReportService>();
builder.Services.AddSingleton(new AnnouncementRepository(connectionString));
builder.Services.AddTransient<AnnouncementService>();
builder.Services.AddSingleton(new BookAdminRepository(connectionString));
builder.Services.AddTransient<BookAdminService>();

// 完整版部署：采购分析 / 座位 / 管理员罚款与荐购服务
builder.Services.AddSingleton(new PurchaseAnalysisRepository(connectionString));
builder.Services.AddSingleton(new PurchaseAnalysisService(new PurchaseAnalysisRepository(connectionString)));
builder.Services.AddSingleton(new SpaceRepository(connectionString));
builder.Services.AddSingleton(new SpaceService(new SpaceRepository(connectionString)));
builder.Services.AddSingleton(new AdminFineService(connectionString));
builder.Services.AddSingleton(new AdminRecommendService(connectionString));

var app = builder.Build();

// 启用 Swagger（开发环境）
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// 启用 Swagger（开发环境）
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 获取 logger
var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger("StartupLogger");
logger.LogInformation($"当前运行环境: {app.Environment.EnvironmentName}");

// 开发环境启用 Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "图书馆 API v1");
        c.RoutePrefix = "api/docs";
    });
}

app.UseStaticFiles(); // 启用 wwwroot 目录下的静态文件

app.UseCors(); // 启用跨域

app.UseRouting();

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30),
});

app.UseMiddleware<ExceptionMiddleware>(); // 自定义异常中间件

app.UseMiddleware<JwtAuthenticationMiddleware>(); // JWT 认证中间件

app.UseAuthorization(); // 授权中间件（如果有）

app.MapControllers();

// 启动应用
app.Run();

public sealed class ThreePersonControllerConvention : IControllerModelConvention
{
    private static readonly HashSet<string> DisabledControllers = new(StringComparer.OrdinalIgnoreCase)
    {
        // 完整版部署：不再禁用任何控制器（采购分析/座位已恢复）
    };

    public void Apply(ControllerModel controller)
    {
        if (!DisabledControllers.Contains(controller.ControllerType.Name))
        {
            return;
        }

        foreach (var selector in controller.Selectors)
        {
            selector.AttributeRouteModel = null;
        }

        // 禁用控制器必须同时关闭每个 action 的 ApiExplorer，否则
        // [ApiController] 的 ApiExplorer 会因“属性路由被清空”而抛出异常
        foreach (var action in controller.Actions)
        {
            action.ApiExplorer.IsVisible = false;
        }
    }
}
using Serilog;
using SmartEstate.Infrastructure.Logging;
using SmartEstate.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog((_, cfg) =>
    cfg.ReadFrom.Configuration(builder.Configuration)
       .Enrich.FromLogContext()
       .Destructure.With<SensitivePropertyDestructuringPolicy>()
       .WriteTo.Console()
       .WriteTo.File(
           "logs/smartestate-workers-.log",
           rollingInterval: RollingInterval.Day,
           retainedFileCountLimit: 7,
           outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

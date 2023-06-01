using ADImportTool.Classes.Worker;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Email;

string CurrentPath = AppDomain.CurrentDomain.BaseDirectory;

IConfiguration _config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile(Path.Combine(CurrentPath, "ConnectorConfig.json"), false, true)
    .Build();

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<ADImportWorker>();
    })
    .UseWindowsService(options =>
    {
        options.ServiceName = _config.GetValue<string>("Name");
    })
    .ConfigureAppConfiguration(builder =>
    {
        builder.AddConfiguration(_config);
    })
    .ConfigureLogging(logger =>
    {
        IConfigurationSection AllLogging = _config.GetSection("Logging");
        IConfigurationSection AllMail = AllLogging.GetSection("Mail");

        //Generate Folder Path and try to Create if missing
        string LogPath = Path.Combine(CurrentPath, AllLogging.GetValue<string>("LogFolder"));
        Directory.CreateDirectory(LogPath);

        //Generate Folder Path with Filepath
        LogPath = Path.Combine(LogPath, AllLogging.GetValue<string>("LogName"));

        LoggerConfiguration RawLogger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: AllLogging.GetValue<string>("Template"))
            .WriteTo.File(LogPath, outputTemplate: AllLogging.GetValue<string>("Template"), rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
            .MinimumLevel.Information();

        string[] AllRecipient = AllMail.GetSection("Recipient").Get<string[]>();

        foreach(string SingleRecipient in AllRecipient)
        {
            RawLogger.WriteTo.Email(new EmailConnectionInfo
            {
                EmailSubject = _config.GetValue<string>("Name"),
                FromEmail = AllMail.GetValue<string>("SourceMail"),
                MailServer = AllMail.GetValue<string>("Server"),
                Port  =AllMail.GetValue<int>("Port"),
                ToEmail = SingleRecipient
            }, restrictedToMinimumLevel: LogEventLevel.Warning);
        }


        Log.Logger = RawLogger.CreateLogger();
    })
    .UseSerilog()
    .Build();

await host.RunAsync();

//Close all Sinks
Log.CloseAndFlush();
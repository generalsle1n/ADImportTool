using ADImportTool.Classes.Connectors;

namespace ADImportTool.Classes.Worker
{
    public class ADImportWorker : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<ADImportWorker> _logger;
        private ConnectorManager _connectorManager;
        private const int millisecondsMultiplyer = 60000;

        public ADImportWorker(IConfiguration config, ILogger<ADImportWorker> logger)
        {
            _config = config;
            _logger = logger;
        }

        private async Task InitConnector(CancellationToken stoppingToken)
        {
            _connectorManager = new ConnectorManager(_config, _logger);
            await _connectorManager.Initialize(stoppingToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await InitConnector(stoppingToken);
            //Calculate Millieseconds From minutes
            int timeOut = _config.GetValue<int>("RefreshTime") * millisecondsMultiplyer;
            _logger.LogInformation($"Sync cycle is set on {timeOut/ millisecondsMultiplyer} Minutes");

            while (!stoppingToken.IsCancellationRequested)
            {
                await _connectorManager.StartSyncAsync(stoppingToken);
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(timeOut, stoppingToken);
            }
        }
    }
}
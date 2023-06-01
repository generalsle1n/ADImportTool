using ADImportTool.Classes.DynamicHelper;
using ADImportTool.Classes.Store;
using ADImportTool.Classes.Worker;
using ADImportTool.Model;

namespace ADImportTool.Classes.Connectors
{
    internal class ConnectorManager
    {
        public List<IConnector> AllReadyConnectors = new List<IConnector>();
        private readonly IConfiguration _config;
        private readonly ILogger<ADImportWorker> _logger;
        private UserStore _userStore = new UserStore();
        private RuleProcessor _ruleProcessor = new RuleProcessor();
        private ADStore _adStore;

        public ConnectorManager(IConfiguration config, ILogger<ADImportWorker> logger)
        {
            _config = config;
            _logger = logger;
        }
        
        internal async Task Initialize(CancellationToken stoppingToken)
        {
            await AddConnectorsFromConfig(stoppingToken);
            _adStore = new ADStore(_config, _logger);
        }
        private async Task AddConnectorsFromConfig(CancellationToken stoppingToken)
        {
            //Read Whole Tree from Connectors
            IConfigurationSection AllSections = _config.GetSection("Connectors");

            //Process Single Connector
            foreach (IConfigurationSection SingleConnector in AllSections.GetChildren())
            {
                //Create Empty Object from Type in Config
                List<IConfigurationSection> AllConnectorSettings = SingleConnector.GetChildren().ToList();
                string TypeConnectorName = AllConnectorSettings.Where(section => section.Key.Equals("Type")).First().Value;
                DynamicConnectorHelper _helper = new DynamicConnectorHelper();
                IConnector Connector = _helper.CreateEmptyObject<IConnector>(TypeConnectorName);

                //Try to insert Config Values from Config into Object
                Connector = _helper.InsertValuesFromConfigIntoConnector(Connector, AllConnectorSettings);

                //Run Pre Sync if needed
                if (Connector.PreStartSync.Enabled)
                {
                    _logger.LogInformation($"Start PreSync for {Connector.Name} with {Connector.PreStartSync.Path} + {Connector.PreStartSync.Arguments}");
                    KeyValuePair<bool, string> Result = await Connector.StartPreSyncProccess(stoppingToken);
                }
                

                if (Connector.ValidateConfig())
                {
                    AllReadyConnectors.Add(Connector);
                    _logger.LogInformation($"Added Connector: {Connector.Name} from Config");
                }
                else
                {
                    _logger.LogError($"Cannot Add Connector: {Connector.Name} from Config, because the validation failed");
                }
            }
        }
        //Sort the Connector from priority (The highest prio get proccessed last)
        private void SortConnectorFromPriority()
        {
            AllReadyConnectors = AllReadyConnectors.OrderBy(connector => connector.Priority).ToList();
            _logger.LogInformation("Generated Connector ProcessFlow");
        }
        private void ReadUsersFromConnector(IConnector connector)
        {
            _logger.LogInformation($"Start getting Users from {connector.Name}");
            List<User> tempUsers = connector.GetAllUsers();
            foreach (User tempUser in tempUsers)
            {
                //Rule Processing Engine
                bool isError = false;
                User user = new User();
                try
                {
                    user = _ruleProcessor.ProcessManipulationUser(tempUser, connector.Attributes);
                    user = _ruleProcessor.ProcessGenerationUser(user, connector.Attributes);
                }
                catch (Exception ex)
                {
                    isError = true;
                }
                if (isError)
                {
                    _logger.LogWarning($"Error in rule Processing for User {tempUser.LastName} {tempUser.FirstName}");
                }
                else
                {
                    _userStore.AddUserToStore(user);
                }
            }
            if (connector.ManagerSettings.Enabled == true)
            {
                _userStore.ResolveManager(connector.ManagerSettings.ResolveManagerBy);
                _logger.LogInformation($"Tried to resolve Manager with {connector.ManagerSettings.ResolveManagerBy}");
            }
            _logger.LogInformation($"Finished getting User from {connector.Name}");
        }
        private async Task StartLdapProcessWithUserStore(IConnector Connector)
        {
            _logger.LogInformation($"Start Ldap Sync for Connector {Connector.Name}");
            int UserModified = 0;
            List<User> AllUserFromStore = _userStore.GetUsersFromStore();
            await _adStore.StartConnection();
            
            foreach (User SingleUser in AllUserFromStore)
            {
                List<KeyValuePair<string, string>> Conditions = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(Connector.ADPrimaryKey, SingleUser.EmployeeNumber)
                };
                bool IsUserModified = await _adStore.CompareConnectorWithLdap(SingleUser, Conditions, Connector);

                if (IsUserModified)
                {
                    UserModified++;
                }
            }

            await _adStore.StopConnection();
            _logger.LogInformation($"{UserModified} User modified in AD");
        }
        private async Task<bool> RunPreSync(IConnector Connector, CancellationToken stoppingToken)
        {
            KeyValuePair<bool, string> PreSyncResult = await Connector.StartPreSyncProccess(stoppingToken);
            if (Connector.PreStartSync.RequiredForSync == true && PreSyncResult.Key == false)
            {
                _logger.LogWarning($"PreSync failed for {Connector.Name} PreSync Log: {PreSyncResult.Value}");
                return false;
            }
            else
            {
                _logger.LogInformation($"Runned PreSync for {Connector.Name}");
                return true;
            }
        }
        //Start the sync Process
        internal async Task<bool> StartSyncAsync(CancellationToken stoppingToken)
        {
            //Sort Connector
            SortConnectorFromPriority();
            if (AllReadyConnectors.Count >= 1)
            {
                foreach (IConnector connector in AllReadyConnectors)
                {
                    bool PreSync = await RunPreSync(connector, stoppingToken);
                    if(PreSync)
                    {
                        ReadUsersFromConnector(connector);
                        await StartLdapProcessWithUserStore(connector);
                    }
                    else
                    {
                        _logger.LogError($"Skipped Connector {connector.Name} because PreSync failed");
                    }   
                }
            }
            else
            {
                _logger.LogError("There are no working Connectors available");
            }


            //Clear Store after Sync Cycle
            _userStore.ClearUserStore();
            return true;
        }
    }
}

using ADImportTool.Classes.Connectors;
using ADImportTool.Classes.Worker;
using ADImportTool.Model;
using LdapForNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static LdapForNet.Native.Native;

namespace ADImportTool.Classes.Store
{
    internal class ADStore
    {
        private IConfiguration _config;
        private ILogger<ADImportWorker> _logger;
        private ADSetting _settings = new ADSetting();
        private ILdapConnection _connection = new LdapConnection();
        private LdapCredential _credential;
        private DynamicClassHelper _classHelper = new DynamicClassHelper(); 

        public ADStore(IConfiguration config, ILogger<ADImportWorker> logger)
        {
            _config = config;
            _logger = logger;
            ReadConfig();
            _credential = new LdapCredential()
            {
                UserName = _settings.UserName,
                Password = _settings.Password
            };
            
        }

        private void ReadConfig()
        {
            PropertyInfo[] AllProperties = _settings.GetType().GetProperties();

            foreach (PropertyInfo Property in AllProperties)
            {
                Property.SetValue(_settings, _config.GetValue<string>(Property.Name));
            }
            _logger.LogInformation("Imported Ldap Config");
        }
        internal async Task StartConnection()
        {
            _logger.LogInformation($"Try to Connect {_settings.DomainName}:{_settings.Port} with User {_settings.UserName}");
            _connection.Connect(_settings.DomainName, int.Parse(_settings.Port));
            await _connection.BindAsync(LdapAuthType.Simple, _credential); 
        }
        private string BuildLdapFilter(List<KeyValuePair<string, string>> Condition)
        {
            StringBuilder WholeFilter = new StringBuilder();
            foreach(KeyValuePair<string, string> con in Condition)
            {
                WholeFilter.Append($"({con.Key}={con.Value})");
            }
            string FinishedFilter = _settings.DefaultFilter.Insert(_settings.DefaultFilter.Length -1, WholeFilter.ToString());

            return FinishedFilter;
        }
        private async Task<LdapEntry> SearchUserWithAttribute(List<KeyValuePair<string,string>> Condition)
        {
            string LdapFilter = BuildLdapFilter(Condition);
            IList<LdapEntry> Result = await _connection.SearchAsync(_settings.SearchBase, LdapFilter);
            return Result.FirstOrDefault();
        }
        private async Task<bool> CheckOperationForUser(User User, LdapEntry LdapUser, IConnector Connector)
        {
            PropertyInfo[] AllProperties = Connector.Attributes.GetType().GetProperties();
            List<KeyValuePair<AttributeSettings, string>> NewProperties = new List<KeyValuePair<AttributeSettings, string>>();
            
            foreach(PropertyInfo Property in AllProperties)
            {
                AttributeSettings Settings = _classHelper.GetAttributeSettingsFromClass(Property.GetValue(Connector.Attributes));
                if(!Settings.Destination.Equals(""))
                {
                    string UserValue = "";
                    if (Property.Name.Equals("Manager"))
                    {
                        try
                        {
                        UserValue = await GetManagerDN(User, LdapUser, Connector.ManagerSettings, Connector.ADPrimaryKey);
                        }catch(Exception ex)
                        {
                            _logger.LogWarning($"Manager cannot resovled for User {User.LastName}, {User.FirstName}");
                            UserValue = "Skipped";
                    }
                        
                    }
                    else
                    {
                        UserValue = (string)User.GetType().GetProperty(Property.Name).GetValue(User);
                    }
                    
                    string LdapValue = GetAttributeValueByKey(LdapUser.DirectoryAttributes, Settings.Destination);
                    if (!UserValue.Equals(LdapValue) && !UserValue.Equals("Skipped"))
                    {
                        NewProperties.Add(new KeyValuePair<AttributeSettings, string>(Settings, UserValue));
                    }
                }
            }

            await ClearLdapAttributes(LdapUser, Connector.ADClearProperty, User);

            if(NewProperties.Count >= 1)
            {
                return await UpdateUser(User, NewProperties, LdapUser);
            }
            else
            {
                return false;
            }
        }
        private async Task<string> GetManagerDN(User User, LdapEntry LdapUser, ManagerSettings Manager, string ADPrimaryKey)
        {
            string ManagerValue = (string)User.Manager.GetType().GetProperty(Manager.ResolveManagerBy).GetValue(User.Manager);
            string LdapFilter = $"(&(objectClass=user)({ADPrimaryKey}={ManagerValue}))";
            IList<LdapEntry> Result = await _connection.SearchAsync(_settings.SearchBase, LdapFilter);
            return Result.FirstOrDefault().Dn;
        }
        private async Task<bool> UpdateUser(User User, List<KeyValuePair<AttributeSettings, string>> NewProperties, LdapEntry LdapUser)
        {
            bool Failure = false;
            bool Skipped = false;
            bool Updated = false;
            List<LdapModifyAttribute> AllAttributes = new List<LdapModifyAttribute>();
            Exception FailureException = new Exception();
            foreach(KeyValuePair<AttributeSettings, string> Property in NewProperties)
            {
                if(!Property.Value.Equals(string.Empty))
                {
                    AllAttributes.Add(new LdapModifyAttribute
                    {
                        LdapModOperation = LdapModOperation.LDAP_MOD_REPLACE,
                        Type = Property.Key.Destination,
                        Values = new List<string> { Property.Value }
                    });
                }
                else
                {
                    _logger.LogWarning($"{Property.Key.Field} skipped for User {User.LastName}, {User.FirstName} because its not filled in Connector");
                }
                
            }

            LdapModifyEntry ModifyEntry = new LdapModifyEntry()
            {
                Dn = LdapUser.Dn,
                Attributes = AllAttributes
            };

            try
            {
                if(ModifyEntry.Attributes.Count > 0)
                {
                    await _connection.ModifyAsync(ModifyEntry);
                    Updated = true;
                }
                else
                {
                    Skipped = true;
                    _logger.LogWarning("Nothing to Update, this happens if not all data is maintained");
                }
                
            }
            catch(Exception ex)
            {
                Failure = true;
                FailureException = ex;
            }
            finally
            {
                if (Failure == false && Skipped == false)
                {
                    _logger.LogInformation($@"{User.LastName}, {User.FirstName} updated");
                }
                else if(Failure == true)
                {
                    _logger.LogError($"Something went wrong in Updateing User {User.LastName}, {User.FirstName} with Error: {FailureException.Message}");
                }
            }

            return Updated;            
        }
        private string GetAttributeValueByKey(SearchResultAttributeCollection Attribute, string Key)
        {
            bool OperationResult = Attribute.TryGetValue(Key, out DirectoryAttribute Result);
            if(OperationResult)
            {
                return Result.GetValue<string>();
            }
            else
            {
                return string.Empty;
            }
        }
        private async Task ClearLdapAttributes(LdapEntry LdapEntry, string AttributesToClear, User User)
        {
            string[] Attribute = AttributesToClear.Split(";");
            foreach(string AttributeValue in Attribute)
            {
                bool Exists = LdapEntry.DirectoryAttributes.TryGetValue(AttributeValue, out DirectoryAttribute Result);
                if (Exists)
                {
                    bool Success = true;
                    try
                    {
                        await DeleteAttribute(LdapEntry, Result);
                    }
                    catch(Exception ex) 
                    {
                        Success = false;
                    }
                    if (Success)
                    {
                        _logger.LogInformation($"Cleared Attribute {AttributeValue} for User {User.LastName}, {User.FirstName}");
                    }
                    else
                    {
                        _logger.LogWarning($"Cannot cleared Attribute {AttributeValue}, for User {User.LastName}, {User.FirstName}");
                    }
                }
            }
        }
        private async Task DeleteAttribute(LdapEntry User, DirectoryAttribute Attribute)
        {
            List<LdapModifyAttribute> AttributeToDelete = new List<LdapModifyAttribute>();
            AttributeToDelete.Add(new LdapModifyAttribute
            {
                Type = Attribute.Name,
                LdapModOperation = LdapModOperation.LDAP_MOD_DELETE,
                Values = new List<string>()
            });

            LdapModifyEntry Entry = new LdapModifyEntry()
            {
                Dn = User.Dn,
                Attributes = AttributeToDelete
            };

            await _connection.ModifyAsync(Entry);
        }
        internal async Task<bool> CompareConnectorWithLdap(User User, List<KeyValuePair<string, string>> Condition, IConnector Connector)
        {
            LdapEntry ResultUser = await SearchUserWithAttribute(Condition);
            bool IsUserModified = false;
            if(ResultUser == null)
            {
                //CreateUser(User);
            }
            else
            {
                IsUserModified =  await CheckOperationForUser(User, ResultUser, Connector);
            }

            return IsUserModified;
        }
        internal async Task StopConnection()
        {
            _connection.Dispose();
            _logger.LogInformation($"Terminated Ldap Connection from {_settings.DomainName}");
        }
    }
}

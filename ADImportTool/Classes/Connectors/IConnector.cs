using ADImportTool.Model;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace ADImportTool.Classes.Connectors
{
    interface IConnector
    {
        string Type { get; set; }
        string Name { get; set; }
        Attributes Attributes { get; set; }
        int Priority { get; set; }
        string DataSource { get; set; }
        string JsonArrayStart { get; set; }
        ManagerSettings ManagerSettings { get; set; }
        PreStartSync PreStartSync { get; set; }
        string ADPrimaryKey { get; set; }
        string ADClearProperty { get; set; }
        List<User> GetAllUsers();
        //Search in Attribute and with Value
        User GetSingleUserByAttribute(string AttributeName, string AttributeValue)
        {
            List<User> AllUsers = GetAllUsers();
            PropertyInfo property = typeof(User).GetProperty(AttributeName);

            return AllUsers.Where(user => property.GetValue(user, null).Equals(AttributeValue)).FirstOrDefault();
        }

        async Task<KeyValuePair<bool, string>> StartPreSyncProccess(CancellationToken stoppingToken)
        {
            if (!PreStartSync.Path.Equals(string.Empty))
            {
                Process PreSync = new Process();
                PreSync.StartInfo = new ProcessStartInfo
                {
                    FileName = PreStartSync.Path,
                    Arguments = PreStartSync.Arguments,
                    RedirectStandardOutput = true
                };
                //PreSync.Error
                PreSync.Start();
                await PreSync.WaitForExitAsync(stoppingToken);
                if (PreSync.ExitCode == 0)
                {
                    return new KeyValuePair<bool, string>(true, string.Empty);
                }
                else
                {
                    using (var reader = PreSync.StandardOutput)
                    {
                        string Error = await reader.ReadToEndAsync();
                        return new KeyValuePair<bool, string>(false, Error);
                    }

                }
            }
            else
            {
                return new KeyValuePair<bool, string>(true, string.Empty);
            }
            
        }
        bool ValidateConfig();
    }
}

using ADImportTool.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ADImportTool.Classes.Connectors.Implemantations
{
    internal class JsonFileConnector : IConnector
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public Attributes Attributes { get; set; }
        public int Priority { get; set; }
        public string DataSource { get; set; }
        public string JsonArrayStart { get; set; }
        public string ADPrimaryKey { get; set; }
        public string ADClearProperty { get; set; }
        public ManagerSettings ManagerSettings { get; set; }
        public PreStartSync PreStartSync { get; set; }
        private DynamicClassHelper _dynamicsClassHelper = new DynamicClassHelper();
        public List<User> GetAllUsers()
        {
            string _userFileContent = File.ReadAllText(DataSource, Encoding.Latin1);
            JsonObject WholeJsonDocument = JsonSerializer.Deserialize<JsonObject>(_userFileContent);
            JsonArray WholeJsonArray = (JsonArray)WholeJsonDocument[JsonArrayStart];
            List<User> AllUsers = new List<User>();
            foreach (JsonObject json in WholeJsonArray)
            {
                User user = new User();
                user = _dynamicsClassHelper.InsertPropertiesIntoUserInstance(user, json, Attributes);
                AllUsers.Add(user);
            }

            return AllUsers;
        }
        public bool ValidateConfig()
        {
            bool IsValid = false;
            //Common check if object is ok
            if (Type != null && Name != null && Attributes != null && Priority != null && DataSource != null && JsonArrayStart != null && ManagerSettings != null)
            {
                if (File.Exists(DataSource))
                {
                    IsValid = true;
                }
            }
            return IsValid;
        }
    }
}

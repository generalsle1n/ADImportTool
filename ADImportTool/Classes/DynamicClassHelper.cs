using ADImportTool.Classes.Connectors;
using ADImportTool.Classes.Connectors.Implemantations;
using ADImportTool.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ADImportTool.Classes
{
    internal class DynamicClassHelper
    {
        private const string _modelPath = "ADImportTool.Model.";
        private const string _connectorPath = "ADImportTool.Classes.Connectors.Implemantations.";
        internal KeyValuePair<Type,object>  CreateAttributeInstanceFromConfig(IConfigurationSection Section)
        {
            List<IConfigurationSection> ChildSection = Section.GetChildren().ToList();

            //Try to Get Type off Class
            Type ClassType = Type.GetType($"{_modelPath}{Section.Key}");
            //Create untyped Object from Type
            object AttributeClass = Activator.CreateInstance(ClassType);
            //hier wird wild :D --> Reflection, we try to get the Property and set it to an object

            ClassType.GetProperty("Settings").SetValue(AttributeClass, new AttributeSettings
            {
                Field = ChildSection.Where(child => child.Key.Equals("Field")).First().Value,
                Manipulation = ChildSection.Where(child => child.Key.Equals("Manipulation")).First().Value,
                GenerateFrom = ChildSection.Where(child => child.Key.Equals("GenerateFrom")).First().Value,
                Destination = ChildSection.Where(child => child.Key.Equals("Destination")).First().Value,
            });

            return new KeyValuePair<Type, object>(ClassType, AttributeClass);
        }
        internal KeyValuePair<Type, object> CreateManagerInstanceFromConfig(IConfigurationSection Section)
        {
            List<IConfigurationSection> ChildSection = Section.GetChildren().ToList();

            //Try to Get Type off Class
            Type ClassType = Type.GetType($"{_modelPath}{Section.Key}");
            //Create untyped Object from Type
            object ManagerClass = Activator.CreateInstance(ClassType);
            //hier wird wild :D --> Reflection, we try to get the Property and set it to an object
            foreach(IConfigurationSection Config in ChildSection)
            {
                PropertyInfo Property = ClassType.GetProperty(Config.Key);
                if(Property != null)
                {
                    if (Property.PropertyType == typeof(int))
                    {
                        Property.SetValue(ManagerClass, int.Parse(Config.Value));
                    }
                    else if (Property.PropertyType == typeof(bool))
                    {
                        Property.SetValue(ManagerClass, bool.Parse(Config.Value));
                    }
                    else
                    {
                        Property.SetValue(ManagerClass, Config.Value);
                    }
                }
            }

            return new KeyValuePair<Type, object>(ClassType, ManagerClass);
        }
        internal Attributes InsertObjectIntoAttributesInstance(Attributes Attribute, object Instance)
        {
            Type AttributeClassType = Attribute.GetType();
            Type ObjectClassType = Instance.GetType();
            AttributeClassType.GetProperty(ObjectClassType.Name).SetValue(Attribute, Instance);
            return Attribute;
        }
        internal IConnector InsertPropertiesIntoConnectorInstance(IConnector Connector, List<IConfigurationSection> Section)
        {
            Type ConnectorClass = Connector.GetType();
            foreach (ConfigurationSection config in Section)
            {
                PropertyInfo Property = ConnectorClass.GetProperty(config.Key);
                if (Property != null)
                {
                    if (Property.PropertyType == typeof(int))
                    {
                        Property.SetValue(Connector, int.Parse(config.Value));
                    }
                    else if (Property.PropertyType == typeof(bool))
                    {
                        Property.SetValue(Connector, bool.Parse(config.Value));
                    }
                    else
                    {
                        Property.SetValue(Connector, config.Value);
                    }
                }
            }
            return Connector;
        }
        internal IConnector CreateEmptyConnectorFromString(string TypeName)
        {
            Type ClassType = Type.GetType($"{_connectorPath}{TypeName}");
            //Create untyped Object from Type
            IConnector ConnectorClass = (IConnector)Activator.CreateInstance(ClassType);

            return ConnectorClass;
        }
        internal User InsertPropertiesIntoUserInstance(User User, JsonObject ValueProperties, Attributes Mapping)
        {
            List<PropertyInfo> AllProperties = Mapping.GetType().GetProperties().ToList();
            PropertyInfo[] UserPorps = User.GetType().GetProperties();
            foreach(PropertyInfo SingleProp in AllProperties)
            {
                string PropName = SingleProp.Name;

                if (!PropName.Equals("Manager"))
                {
                    object PropertyValue = SingleProp.GetValue(Mapping, null);
                    AttributeSettings Setting = GetAttributeSettingsFromClass(PropertyValue);
                    PropertyInfo SelectedProperty = UserPorps.Where(prop => prop.Name.Equals(PropName)).First();
                    string SelectedPropertyValue = GetValueByAttributeSettings(Setting, ValueProperties);
                    SelectedProperty.SetValue(User, SelectedPropertyValue);
                }     
            }
            return User;
        }
        internal AttributeSettings GetAttributeSettingsFromClass(object ClassObject)
        {
            Type ObjectType = ClassObject.GetType();
            PropertyInfo PropertySetting = ObjectType.GetProperty("Settings");
            if(PropertySetting != null)
            {
                AttributeSettings Setting = (AttributeSettings)PropertySetting.GetValue(ClassObject, null);
                return Setting;
            }
            else
            {
                return null;
            }
        }
        private string GetValueByAttributeSettings(AttributeSettings Setting, JsonObject Values)
        {
            if (!Setting.Field.Equals(""))
            {
                Values.TryGetPropertyValue(Setting.Field, out var JsonNode);
                return JsonNode.GetValue<string>();
            }
            else
            {
                return "";
            }
        }
    }
}

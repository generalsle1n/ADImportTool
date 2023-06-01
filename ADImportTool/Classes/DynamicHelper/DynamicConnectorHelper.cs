using ADImportTool.Classes.Connectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ADImportTool.Classes.DynamicHelper
{
    internal class DynamicConnectorHelper : DynamicObjectHelper
    {
        internal IConnector InsertValuesFromConfigIntoConnector(IConnector Connector, List<IConfigurationSection> Config)
        {
            foreach(IConfigurationSection SingleConfig in Config)
            {
                List<IConfigurationSection> SingleConfigChild = SingleConfig.GetChildren().ToList();
                IConfigurationSection Type = SingleConfigChild.Where(item => item.Key.Equals("Type")).FirstOrDefault();

                if(Type == null)
                {
                    Connector = InsertSingleValueIntoComplexObject<IConnector>(Connector, SingleConfig.Value ?? string.Empty, SingleConfig.Key, ParseDataIntoType: true);
                }
                else
                {
                    Connector = InsertObjectFromConfigIntoConnector(Connector, SingleConfigChild, Type.Value);
                }
            }
            return Connector;
        }

        private IConnector InsertObjectFromConfigIntoConnector(IConnector Connector, List<IConfigurationSection> Config, string Anchor)
        {
            object RootObject = CreateEmptyObject<object>(Anchor);
            PropertyInfo[] AllProperties = RootObject.GetType().GetProperties();
            Type RootObjectType = RootObject.GetType();
            foreach(PropertyInfo PropertyInfo in AllProperties)
            {
                object SingleObject = new object();
                try
                {
                    SingleObject = CreateEmptyObject<object>(PropertyInfo.PropertyType.FullName);
                }catch(Exception ex)
                {

                }

                List<IConfigurationSection> ObjectConfig = Config.Where(item => item.Key.Equals(PropertyInfo.Name)).ToList();

                //Check if Object is an Simple or Complex with Custom Properties
                if (SingleObject.GetType().GetProperties().Length >= 1)
                {
                    string SingleObjectPropertyName = SingleObject.GetType().GetProperties().FirstOrDefault().Name;

                    foreach (IConfigurationSection SingleConfig in ObjectConfig)
                    {
                        List<IConfigurationSection> AllObjectProperties = SingleConfig.GetChildren().ToList();
                        IConfigurationSection Type = AllObjectProperties.Where(item => item.Key.Equals("Type")).FirstOrDefault();
                        AllObjectProperties.Remove(Type);

                        object ChildObject = CreateEmptyObject<object>(Type.Value);

                        foreach (IConfigurationSection ObjectProperties in AllObjectProperties)
                        {
                            ChildObject = InsertSingleValueIntoComplexObject<object>(ChildObject, ObjectProperties.Value ?? string.Empty, ObjectProperties.Key);
                        }

                        SingleObject = InsertSingleValueIntoComplexObject<object>(SingleObject, ChildObject, SingleObjectPropertyName);
                        RootObject = InsertSingleValueIntoComplexObject<object>(RootObject, SingleObject, SingleConfig.Key);
                    }
                }
                else
                {
                    RootObject = InsertSingleValueIntoComplexObject<object>(RootObject, ObjectConfig.First().Value ?? string.Empty, ObjectConfig.First().Key, ParseDataIntoType: true);
                }
                
            }

            Connector = InsertSingleValueIntoComplexObject<IConnector>(Connector, RootObject, RootObjectType.Name);
            return Connector;
        }
    }
}

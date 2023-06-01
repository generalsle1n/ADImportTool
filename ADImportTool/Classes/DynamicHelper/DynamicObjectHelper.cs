using ADImportTool.Classes.Connectors;
using LdapForNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ADImportTool.Classes.DynamicHelper
{
    internal class DynamicObjectHelper
    {
        internal T CreateEmptyObject<T>(string TypeName)
        {
            Type Type = Type.GetType(TypeName);
            T Object = (T)Activator.CreateInstance(Type);

            return Object;
        }

        internal T InsertSingleValueIntoComplexObject<T>(T Object, object Value, string Destiantion, bool ParseDataIntoType = false)
        {
            Type Type = Object.GetType();
            PropertyInfo Property = Type.GetProperty(Destiantion);
            if(ParseDataIntoType)
            {
                Value = ParseUnkownData<object>(Value, Property);
            }
            Property.SetValue(Object, Value);

            return Object;
        }
        internal T ParseUnkownData<T>(T Value, PropertyInfo Property)
        {
            TypeInfo TypeInfo = Property.PropertyType.GetTypeInfo();

            TypeConverter Converter = TypeDescriptor.GetConverter(TypeInfo);
            T ParsedValue = (T)Converter.ConvertFromString(Value.ToString());

            return ParsedValue;
        }
        internal Type GetPropertyType(object Object, string PropertyName)
        {
            Type Type = Object.GetType();
            PropertyInfo Property = Type.GetProperty(PropertyName);
            return Property.PropertyType;
        }
    }
}

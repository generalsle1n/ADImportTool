using ADImportTool.Classes.Worker;
using ADImportTool.Model;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ADImportTool.Classes.Connectors
{
    internal class RuleProcessor
    {
        private const string _regExRule = "{.+?}";
        internal DynamicClassHelper _classHelper = new DynamicClassHelper();
        internal User ProcessManipulationUser(User User, Attributes Attributes)
        {
            PropertyInfo[] AllPropertyTypes = Attributes.GetType().GetProperties();
            foreach(PropertyInfo PropertyInfo in AllPropertyTypes)
            {
                object Property = PropertyInfo.GetValue(Attributes);
                string PropertyName  = PropertyInfo.Name;
                AttributeSettings Settings = _classHelper.GetAttributeSettingsFromClass(Property);

                if (!Settings.Manipulation.Equals(""))
                {
                    PropertyInfo SingleProperty = User.GetType().GetProperty(PropertyName);
                    string OrginalValue = (string)SingleProperty.GetValue(User);
                    string ManipulatedValue  = ProcessRuleManipulation(Settings.Manipulation, OrginalValue, User);
                    User = InsertValueIntoUser(User, SingleProperty.Name, ManipulatedValue);
                }
            }

            return User;
        }
        internal User ProcessGenerationUser(User User, Attributes Attributes)
        {
            PropertyInfo[] AllPropertyTypes = Attributes.GetType().GetProperties();
            foreach (PropertyInfo PropertyInfo in AllPropertyTypes)
            {
                object Property = PropertyInfo.GetValue(Attributes);
                string PropertyName = PropertyInfo.Name;
                AttributeSettings Settings = _classHelper.GetAttributeSettingsFromClass(Property);

                if (!Settings.GenerateFrom.Equals(""))
                {
                    PropertyInfo SingleProperty = User.GetType().GetProperty(PropertyName);
                    string OrginalValue = (string)SingleProperty.GetValue(User);
                    if (OrginalValue.Equals(String.Empty))
                    {
                        string Generate = ProcessRuleGeneration(Settings.GenerateFrom, User);
                        InsertValueIntoUser(User, SingleProperty.Name, Generate);
                    }
                }
            }

            return User;
        }
        private string ProcessRuleManipulation(string Rule, string Input, User user)
        {
            List<string> ParsedRule = Rule.Split("=").ToList();
            MethodInfo Method = GetMethod(ParsedRule[0]);
            string result = ExecuteMethod(Method, Input, ParsedRule, user);
            return result;
        }
        private string ProcessRuleGeneration(string Rule, User user)
        {
            StringBuilder TextGenerator = new StringBuilder(Rule);

            KeyValuePair<string, string>[] RuleObjects = GetRuleObjects(Rule);

            foreach (KeyValuePair<string, string> SingleRuleObject in RuleObjects)
            {
                string ObjectValue = (string)user.GetType().GetProperty(SingleRuleObject.Value).GetValue(user);
                if(Rule.Contains($"{SingleRuleObject.Key} ") && ObjectValue.Equals(string.Empty))
                {
                    TextGenerator.Replace(SingleRuleObject.Key + " ", "");
                }
                else
                {
                    TextGenerator.Replace(SingleRuleObject.Key, ObjectValue);
                }
            }

            return TextGenerator.ToString();
        }
        private KeyValuePair<string,string>[] GetRuleObjects(string Rule)
        {
            List<string> RuleObject = new List<string>();
            MatchCollection matches = Regex.Matches(Rule, _regExRule);

            return matches.Select(item => new KeyValuePair<string, string>(item.Value, item.Value.Replace("{","").Replace("}",""))).ToArray();
        }
        private MethodInfo GetMethod(string Method)
        {
            Type Type = this.GetType();
            MethodInfo MethodInfo = Type.GetMethod(Method, BindingFlags.NonPublic | BindingFlags.Instance);
            return MethodInfo;
        }
        private string ExecuteMethod(MethodInfo Method,string Input, List<string> Parameter, User user)
        {
            if (Method != null)
            {
                string Result = (string)Method.Invoke(this, new object[]
                {
                    Input,
                    int.Parse(Parameter[1]),
                    user
                });

                return Result;
            }
            else
            {
                return null;
            }
        }
        private string RemoveLeading(string Input, int RemoveFirst, User user)
        {
            if (!Input.Equals(""))
            {
                return Input.Substring(RemoveFirst);
            }
            else
            {
                return "";
            }
            
        }
        private string GetFirstLeading(string Input, int GetFirst, User user)
        {
            if (!Input.Equals(""))
            {
                return Input.Substring(0, GetFirst);
            }
            else
            {
                return "";
            }
        }
        private User InsertValueIntoUser(User User, string AttributeName, string AttributeValue)
        {
            PropertyInfo Property = User.GetType().GetProperty(AttributeName);
            Property.SetValue(User, AttributeValue);

            return User;
        }
    }
}

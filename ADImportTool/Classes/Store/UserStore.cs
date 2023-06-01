using ADImportTool.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ADImportTool.Classes.Store
{
    internal class UserStore
    {
        private List<User> _allUsers = new List<User>();
        public void AddUserToStore(User User)
        {
            _allUsers.Add(User);
        }

        private User GetSingleUserByAttribute(string AttributeName, string AttributeValue)
        {
            PropertyInfo property = typeof(User).GetProperty(AttributeName);
            return _allUsers.Where(user => property.GetValue(user, null).Equals(AttributeValue)).FirstOrDefault();
        }

        internal void ResolveManager(string Attribute)
        {
            foreach(User user in _allUsers)
            {
                user.Manager = GetSingleUserByAttribute(Attribute, user.UnresolvedManager);
            }
        }
        internal List<User> GetUsersFromStore()
        {
            return _allUsers;
        }

        internal void ClearUserStore()
        {
            _allUsers.Clear();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ADImportTool.Model
{
    public class User
    {
        public string Active{ get; set; }
        public string Mail { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public User Manager { get; set; }
        public string UnresolvedManager { get; set; }
        public string Position { get; set; }
        public string CostCenter { get; set; }
        public string EmployeeNumber { get; set; }
        public string TimecardNumber { get; set; }
        public string Salution { get; set; }
        public string Area { get; set; }
        public string Department { get; set; }
        public string Team { get; set; }
        public string Intent { get; set; }
    }
}

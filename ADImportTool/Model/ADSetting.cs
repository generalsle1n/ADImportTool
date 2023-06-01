using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADImportTool.Model
{
    public class ADSetting
    {
        public string DomainName { get; set; }
        public string Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string SearchBase { get; set; }
        public string DefaultFilter { get; set; }
    }
}
